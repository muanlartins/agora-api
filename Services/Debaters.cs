using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Newtonsoft.Json;

public class DebatersService {
  public AmazonDynamoDBClient client;
  public string table;
  public DebatersService(WebApplicationBuilder builder) {
    table = "debaters";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
      builder.Configuration["AWS:AccessKey"], 
      builder.Configuration["AWS:SecretKey"]
    );
    client = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
  }

  public async Task<Debater> CreateDebater(Debater debater) {
    string id = Guid.NewGuid().ToString();

    Dictionary<string, AttributeValue> createDebaterItem = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } }, 
      { "name", new AttributeValue { S = debater.name } },
    };
    
    PutItemRequest createDebaterRequest = new PutItemRequest {
      TableName = table,
      Item = createDebaterItem,
    };

    await client.PutItemAsync(createDebaterRequest);

    return new Debater(id, debater.name);
  }

  public async Task<List<Debater>> GetAllDebaters() {
    ScanRequest getAllDebatersRequest = new ScanRequest { 
      TableName = table
    };

    ScanResponse response = await client.ScanAsync(getAllDebatersRequest);

    List<Debater> debaters = new List<Debater>();

    foreach (Dictionary<string, AttributeValue> debater in response.Items) 
      debaters.Add(JsonConvert.DeserializeObject<Debater>(Document.FromAttributeMap(debater).ToJsonPretty())!);

    return debaters;
  }
}