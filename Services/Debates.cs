using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Newtonsoft.Json;

public class DebatesService {
  public AmazonDynamoDBClient client;
  public string table;
  public DebatesService(WebApplicationBuilder builder) {
    table = "debates";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
      builder.Configuration["AWS:AccessKey"], 
      builder.Configuration["AWS:SecretKey"]
    );
    client = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
  }

  public async Task<Debate> CreateDebate(Debate debate) {
    string id = Guid.NewGuid().ToString();

    Dictionary<string, AttributeValue> chair = GetMemberDictionary(debate.chair);

    Dictionary<string, AttributeValue> createDebateItem = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } }, 
      { "date", new AttributeValue { S = debate.date } },
      { "time", new AttributeValue { S = debate.time } },
      { "style", new AttributeValue { S = debate.style.ToString() } },
      { "venue", new AttributeValue { S = debate.venue.ToString() } },
      { "motionType", new AttributeValue { S = debate.motionType.ToString() } },
      { "motionTheme", new AttributeValue { S = debate.motionTheme.ToString() } },
      { "motion", new AttributeValue { S = debate.motion } },
      { "chair", new AttributeValue { M = chair } },
    };

    if (debate.infoSlides is not null) {
      List<AttributeValue> infoSlides = new List<AttributeValue>();
      foreach (string infoSlide in debate.infoSlides) 
        infoSlides.Add(new AttributeValue { S = infoSlide });

      createDebateItem.Add("infoSlides", new AttributeValue { L = infoSlides });
    }

    if (debate.debaters is not null) {
      List<AttributeValue> debaters = new List<AttributeValue>();
      foreach (Member debater in debate.debaters) 
        debaters.Add(new AttributeValue { M = GetMemberDictionary(debater) });

      createDebateItem.Add("debaters", new AttributeValue { L = debaters });
    }

    if (debate.wings is not null) {
      List<AttributeValue> wings = new List<AttributeValue>();
      foreach (Member wing in debate.wings) 
        wings.Add(new AttributeValue { M = GetMemberDictionary(wing) });

      createDebateItem.Add("wings", new AttributeValue { L = wings });
    }

    if (debate.sps is not null) {
      List<AttributeValue> sps = new List<AttributeValue>();
      foreach (int sp in debate.sps) 
        sps.Add(new AttributeValue { N = sp.ToString() });

      createDebateItem.Add("sps", new AttributeValue { L = sps });
    }
    
    PutItemRequest createDebateRequest = new PutItemRequest {
      TableName = table,
      Item = createDebateItem,
    };

    await client.PutItemAsync(createDebateRequest);

    return new Debate(
      id,
      debate.date,
      debate.time,
      debate.style,
      debate.venue,
      debate.motionType,
      debate.motionTheme,
      debate.motion,
      debate.infoSlides,
      debate.debaters,
      debate.sps,
      debate.chair,
      debate.wings
    );
  }

  public async Task<Debate?> GetDebate(string id) {
    Dictionary<string, AttributeValue> Key = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } } 
    };
    
    GetItemRequest getDebateRequest = new GetItemRequest {
      TableName = table,
      Key = Key,
    };

    GetItemResponse response = await client.GetItemAsync(getDebateRequest);

    if (!response.IsItemSet) return null;

    return JsonConvert.DeserializeObject<Debate>(Document.FromAttributeMap(response.Item).ToJsonPretty());
  }

  public async Task<List<Debate>> GetAllDebates() {
    ScanRequest getAllDebatesRequest = new ScanRequest { 
      TableName = table
    };

    ScanResponse response = await client.ScanAsync(getAllDebatesRequest);

    List<Debate> debates = new List<Debate>();

    foreach (Dictionary<string, AttributeValue> debate in response.Items) 
      debates.Add(JsonConvert.DeserializeObject<Debate>(Document.FromAttributeMap(debate).ToJsonPretty())!);

    return debates;
  }

  public Dictionary<string, AttributeValue> GetMemberDictionary(Member member) {
    return new Dictionary<string, AttributeValue>() {
      { "id", new AttributeValue { S = member.id } },
      { "name", new AttributeValue { S = member.name } },
      { "society", new AttributeValue { S = member.society.ToString() } }
    };
  }
}