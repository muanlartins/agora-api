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

    Dictionary<string, AttributeValue> pm = GetDebaterDictionary(debate.pm);
    Dictionary<string, AttributeValue> lo = GetDebaterDictionary(debate.lo);
    Dictionary<string, AttributeValue> ag = GetDebaterDictionary(debate.ag);
    Dictionary<string, AttributeValue> ao = GetDebaterDictionary(debate.ao);
    Dictionary<string, AttributeValue> eg = GetDebaterDictionary(debate.eg);
    Dictionary<string, AttributeValue> eo = GetDebaterDictionary(debate.eo);
    Dictionary<string, AttributeValue> wg = GetDebaterDictionary(debate.wg);
    Dictionary<string, AttributeValue> wo = GetDebaterDictionary(debate.wo);
    Dictionary<string, AttributeValue> chair = GetJudgeDictionary(debate.chair);
    List<AttributeValue> wings = new List<AttributeValue>();
    if (debate.wings is not null) 
      foreach (Judge wing in debate.wings) 
        wings.Add(new AttributeValue { M = GetJudgeDictionary(wing) });

    List<AttributeValue> infoSlides = new List<AttributeValue>();
    if (debate.infoSlides is not null) 
      foreach (string infoSlide in debate.infoSlides) 
        infoSlides.Add(new AttributeValue { S = infoSlide });
    else infoSlides.Add(new AttributeValue { S = "" });

    Dictionary<string, AttributeValue> createDebateItem = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } }, 
      { "pm", new AttributeValue { M = pm } },
      { "lo", new AttributeValue { M = lo } },
      { "ag", new AttributeValue { M = ag } },
      { "ao", new AttributeValue { M = ao } },
      { "eg", new AttributeValue { M = eg } },
      { "eo", new AttributeValue { M = eo } },
      { "wg", new AttributeValue { M = wg } },
      { "wo", new AttributeValue { M = wo } },
      { "pmSp", new AttributeValue { N = debate.pmSp.ToString() } },
      { "loSp", new AttributeValue { N = debate.loSp.ToString() } },
      { "agSp", new AttributeValue { N = debate.agSp.ToString() } },
      { "aoSp", new AttributeValue { N = debate.aoSp.ToString() } },
      { "egSp", new AttributeValue { N = debate.egSp is not null ? debate.egSp.ToString() : "0" } },
      { "eoSp", new AttributeValue { N = debate.eoSp is not null ? debate.eoSp.ToString() : "0" } },
      { "wgSp", new AttributeValue { N = debate.wgSp is not null ? debate.wgSp.ToString() : "0" } },
      { "woSp", new AttributeValue { N = debate.woSp is not null ? debate.woSp.ToString() : "0" } },
      { "og", new AttributeValue { S = debate.og } },
      { "oo", new AttributeValue { S = debate.oo } },
      { "cg", new AttributeValue { S = debate.cg is not null ? debate.cg : "" } },
      { "co", new AttributeValue { S = debate.co is not null ? debate.co : "" } },
      { "chair", new AttributeValue { M = chair } },
      { "wings", new AttributeValue { L = wings } },
      { "motion", new AttributeValue { S = debate.motion } },
      { "infoSlides", new AttributeValue { L = infoSlides } },
      { "date", new AttributeValue { S = debate.date } },
      { "thematic", new AttributeValue { S = debate.thematic } },
      { "prefix", new AttributeValue { S = debate.prefix } },
      { "tournament", new AttributeValue { S = debate.tournament is not null ? debate.tournament : "" } },
    };
    
    PutItemRequest createDebateRequest = new PutItemRequest {
      TableName = table,
      Item = createDebateItem,
    };

    await client.PutItemAsync(createDebateRequest);

    return new Debate(
      id, 
      debate.pm, 
      debate.lo,
      debate.ag,
      debate.ao,
      debate.eg,
      debate.eo,
      debate.wg,
      debate.wo,
      debate.pmSp,
      debate.loSp,
      debate.agSp,
      debate.aoSp,
      debate.egSp,
      debate.eoSp,
      debate.wgSp,
      debate.woSp,
      debate.og,
      debate.oo,
      debate.cg,
      debate.co,
      debate.chair,
      debate.wings,
      debate.motion,
      debate.infoSlides,
      debate.date,
      debate.thematic,
      debate.prefix,
      debate.tournament
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

  public Dictionary<string, AttributeValue> GetJudgeDictionary(Judge? judge) {
    if (judge is not null) 
      return new Dictionary<string, AttributeValue>() {
        { "id", new AttributeValue { S = judge.id } },
        { "name", new AttributeValue { S = judge.name } },
      };

    return new Dictionary<string, AttributeValue>() {
      { "id", new AttributeValue { S = "" } },
      { "name", new AttributeValue { S = "" } },
    };
  }
}