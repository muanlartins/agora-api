using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Newtonsoft.Json;

public class CasefileService
{
    public AmazonDynamoDBClient client;

    public string table;
    private readonly MotionsService motionService; // Injetando MotionService

    public CasefileService(MotionsService motionService)
    {
        this.motionService = motionService;
    }
    private async Task<Motion> GetMotion(string id)
    {
        return await motionService.GetMotion(id);
    }
    public CasefileService(WebApplicationBuilder builder) {
    table = "casefiles";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
        builder.Configuration["AWS:AccessKey"], 
        builder.Configuration["AWS:SecretKey"]
    );
    client = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
    }
    private List<Casefile> casefiles = new List<Casefile>();

    // TODO CreateCaseFile
    public async Task<Casefile> CreateCasefile(Casefile casefile)
    {
        string id = Guid.NewGuid().ToString();

        Dictionary<string, AttributeValue> createCasefileItem = new Dictionary<string, AttributeValue>()
        {
            { "id", new AttributeValue { S = id } },
            { "oppArgument", new AttributeValue { S = casefile.OppArgument } },
            { "govArgument", new AttributeValue { S = casefile.GovArgument } },
            { "describeArgument", new AttributeValue { S = casefile.DescribeArgument ?? "" } },
            { "motionId", new AttributeValue { S = casefile.Motion.Id } }
        };

        PutItemRequest createCasefileRequest = new PutItemRequest
        {
            TableName = table,
            Item = createCasefileItem,
        };

        await client.PutItemAsync(createCasefileRequest);
        return new Casefile(casefile.OppArgument, casefile.GovArgument, casefile.DescribeArgument, casefile.Motion);
    }
    // Todo GetAllCaseFile
    public async Task<List<Casefile>> GetAllCasefiles()
    {
    ScanRequest getAllCasefilesRequest = new ScanRequest
    {
        TableName = table
    };

    ScanResponse response = await client.ScanAsync(getAllCasefilesRequest);

    List<Casefile> casefiles = new List<Casefile>();

    foreach (Dictionary<string, AttributeValue> casefileData in response.Items)
    {
        casefiles.Add(ParseCasefileFromDynamoDBAsync(casefileData));
    }

    return casefiles;
}

    private async Task<Casefile> ParseCasefileFromDynamoDBAsync(Dictionary<string, AttributeValue> casefileData)
    {
        string oppArgument = casefileData["oppArgument"].S;
        string govArgument = casefileData["govArgument"].S;
        string describeArgument = casefileData.ContainsKey("describeArgument") ? casefileData["describeArgument"].S : null;

        // Supondo que Motion seja uma classe com uma propriedade "id"
        string motionId = casefileData["motionId"].S;
        Motion motion = await GetMotion(motionId); // Supondo que você tenha um método para buscar uma Motion pelo ID

        return new Casefile(oppArgument, govArgument, describeArgument, motion);
    }
    // TODO GetCasefile
        public async Task<Casefile?> GetCasefile(string id)
    {
        Dictionary<string, AttributeValue> key = new Dictionary<string, AttributeValue>()
        {
            { "id", new AttributeValue { S = id } }
        };

        GetItemRequest getCasefileRequest = new GetItemRequest
        {
            TableName = table,
            Key = key,
        };

        GetItemResponse response = await client.GetItemAsync(getCasefileRequest);

        if (!response.IsItemSet)
            return null;

        return ParseCasefileFromDynamoDBAsync(response.Item);
    }
    //TO-DO UpdateCasefile*
    //TO-DO DeleteCasefile*
}
