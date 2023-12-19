using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Newtonsoft.Json;

public class DuosService {
  public AmazonDynamoDBClient client;
  public string table;
  public DuosService(WebApplicationBuilder builder) {
    table = "duos";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
      builder.Configuration["AWS:AccessKey"], 
      builder.Configuration["AWS:SecretKey"]
    );
    client = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
  }

  public async Task<Duo> CreateDuo(Duo duo) {
    string id = Guid.NewGuid().ToString();

    Dictionary<string, AttributeValue> a = GetDebaterDictionary(duo.a);
    Dictionary<string, AttributeValue> b = GetDebaterDictionary(duo.b);

    Dictionary<string, AttributeValue> createDuoItem = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } }, 
      { "a", new AttributeValue { M = a } },
      { "b", new AttributeValue { M = b } },
    };
    
    PutItemRequest createDuoRequest = new PutItemRequest {
      TableName = table,
      Item = createDuoItem,
    };

    await client.PutItemAsync(createDuoRequest);

    return new Duo(id, duo.a, duo.b);
  }

  public async Task<List<Duo>> GetAllDuos() {
    ScanRequest getAllDuosRequest = new ScanRequest { 
      TableName = table
    };

    ScanResponse response = await client.ScanAsync(getAllDuosRequest);

    List<Duo> duos = new List<Duo>();

    foreach (Dictionary<string, AttributeValue> duo in response.Items) 
      duos.Add(JsonConvert.DeserializeObject<Duo>(Document.FromAttributeMap(duo).ToJsonPretty())!);

    return duos;
  }

  public Dictionary<string, AttributeValue> GetDebaterDictionary(Debater? debater) {
    if (debater is not null) 
      return new Dictionary<string, AttributeValue>() {
        { "id", new AttributeValue { S = debater.id } },
        { "name", new AttributeValue { S = debater.name } }
      };

    return new Dictionary<string, AttributeValue>() {
      { "id", new AttributeValue { S = "" } },
      { "name", new AttributeValue { S = "" } }
    };
  }
}