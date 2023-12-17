using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Newtonsoft.Json;

public class JudgesService {
  public AmazonDynamoDBClient client;
  public string table;
  public JudgesService(WebApplicationBuilder builder) {
    table = "judges";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
      builder.Configuration["AWS:AccessKey"], 
      builder.Configuration["AWS:SecretKey"]
    );
    client = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
  }

  public async Task<Judge> CreateJudge(Judge judge) {
    string id = Guid.NewGuid().ToString();

    Dictionary<string, AttributeValue> createJudgeItem = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } }, 
      { "name", new AttributeValue { S = judge.name } },
    };
    
    PutItemRequest createJudgeRequest = new PutItemRequest {
      TableName = table,
      Item = createJudgeItem,
    };

    await client.PutItemAsync(createJudgeRequest);

    return new Judge(id, judge.name);
  }

  public async Task<List<Judge>> GetAllJudges() {
    ScanRequest getAllJudgesRequest = new ScanRequest { 
      TableName = table
    };

    ScanResponse response = await client.ScanAsync(getAllJudgesRequest);

    List<Judge> judges = new List<Judge>();

    foreach (Dictionary<string, AttributeValue> judge in response.Items) 
      judges.Add(JsonConvert.DeserializeObject<Judge>(Document.FromAttributeMap(judge).ToJsonPretty())!);

    return judges;
  }
}