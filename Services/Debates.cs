using System.Net;
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

    if (debate.infoSlides is not null && debate.infoSlides.Length > 0) {
      List<AttributeValue> infoSlides = new List<AttributeValue>();
      foreach (string infoSlide in debate.infoSlides) 
        infoSlides.Add(new AttributeValue { S = infoSlide });

      createDebateItem.Add("infoSlides", new AttributeValue { L = infoSlides });
    }

    if (debate.debaters is not null && debate.debaters.Length > 0) {
      List<AttributeValue> debaters = new List<AttributeValue>();
      foreach (Member debater in debate.debaters) 
        debaters.Add(new AttributeValue { M = GetMemberDictionary(debater) });

      createDebateItem.Add("debaters", new AttributeValue { L = debaters });
    }

    if (debate.wings is not null && debate.wings.Length > 0) {
      List<AttributeValue> wings = new List<AttributeValue>();
      foreach (Member wing in debate.wings) 
        wings.Add(new AttributeValue { M = GetMemberDictionary(wing) });

      createDebateItem.Add("wings", new AttributeValue { L = wings });
    }

    if (debate.sps is not null && debate.sps.Length > 0) {
      List<AttributeValue> sps = new List<AttributeValue>();
      foreach (int sp in debate.sps) 
        sps.Add(new AttributeValue { N = sp.ToString() });

      createDebateItem.Add("sps", new AttributeValue { L = sps });
    }

    if (debate.points is not null && debate.points.Length > 0) {
      List<AttributeValue> points = new List<AttributeValue>();
      foreach (int point in debate.points) 
        points.Add(new AttributeValue { N = point.ToString() });

      createDebateItem.Add("points", new AttributeValue { L = points });
    }

    if (debate.tournament is not null) {
      createDebateItem.Add("tournament", new AttributeValue { S = debate.tournament });
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
      debate.points,
      debate.sps,
      debate.chair,
      debate.wings,
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

  public async Task<bool> UpdateDebate(Debate updatedDebate) {
    Dictionary<string, AttributeValue> updateDebateKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = updatedDebate.id } } 
    };

    Dictionary<string,string> expressionAttributeNames = new Dictionary<string, string>() {
      {"#date", "date"},
      {"#time", "time"},
      {"#style", "style"},
      {"#venue", "venue"},
      {"#motionType", "motionType"},
      {"#motionTheme", "motionTheme"},
      {"#motion", "motion"},
      {"#chair", "chair"},
    };

    Dictionary<string,AttributeValue> expressionAttributeValues = new Dictionary<string, AttributeValue> {
      { ":date", new AttributeValue { S = updatedDebate.date } },
      { ":time", new AttributeValue { S = updatedDebate.time } },
      { ":style", new AttributeValue { S = updatedDebate.style.ToString() } },
      { ":venue", new AttributeValue { S = updatedDebate.venue.ToString() } },
      { ":motionType", new AttributeValue { S = updatedDebate.motionType.ToString() } },
      { ":motionTheme", new AttributeValue { S = updatedDebate.motionTheme.ToString() } },
      { ":motion", new AttributeValue { S = updatedDebate.motion } },
      { ":chair", new AttributeValue { M = GetMemberDictionary(updatedDebate.chair) } },
    };

    if (updatedDebate.infoSlides is not null && updatedDebate.infoSlides.Length > 0) {
      List<AttributeValue> infoSlides = new List<AttributeValue>();
      foreach (string infoSlide in updatedDebate.infoSlides) 
        infoSlides.Add(new AttributeValue { S = infoSlide });

      expressionAttributeNames.Add("#infoSlides", "infoSlides");
      expressionAttributeValues.Add(":infoSlides", new AttributeValue { L = infoSlides });
    }

    if (updatedDebate.debaters is not null && updatedDebate.debaters.Length > 0) {
      List<AttributeValue> debaters = new List<AttributeValue>();
      foreach (Member debater in updatedDebate.debaters) 
        debaters.Add(new AttributeValue { M = GetMemberDictionary(debater) });

      expressionAttributeNames.Add("#debaters", "debaters");
      expressionAttributeValues.Add(":debaters", new AttributeValue { L = debaters });
    }

    if (updatedDebate.wings is not null && updatedDebate.wings.Length > 0) {
      List<AttributeValue> wings = new List<AttributeValue>();
      foreach (Member wing in updatedDebate.wings) 
        wings.Add(new AttributeValue { M = GetMemberDictionary(wing) });

      expressionAttributeNames.Add("#wings", "wings");
      expressionAttributeValues.Add(":wings", new AttributeValue { L = wings });
    }

    if (updatedDebate.sps is not null && updatedDebate.sps.Length > 0) {
      List<AttributeValue> sps = new List<AttributeValue>();
      foreach (int sp in updatedDebate.sps) 
        sps.Add(new AttributeValue { N = sp.ToString() });

      expressionAttributeNames.Add("#sps", "sps");
      expressionAttributeValues.Add(":sps", new AttributeValue { L = sps });
    }

    if (updatedDebate.points is not null && updatedDebate.points.Length > 0) {
      List<AttributeValue> points = new List<AttributeValue>();
      foreach (int point in updatedDebate.points) 
        points.Add(new AttributeValue { N = point.ToString() });

      expressionAttributeNames.Add("#points", "points");
      expressionAttributeValues.Add(":points", new AttributeValue { L = points });
    }

    if (updatedDebate.tournament is not null) {
      expressionAttributeNames.Add("#tournament", "tournament");
      expressionAttributeValues.Add(":tournament", new AttributeValue { S = updatedDebate.tournament });
    }

    string updateExpression = "SET #date = :date, #time = :time, #style = :style, #venue = :venue, #motionType = :motionType, #motionTheme = :motionTheme, #motion = :motion, #chair = :chair";

    if (updatedDebate.infoSlides is not null && updatedDebate.infoSlides.Length > 0) updateExpression += ", #infoSlides = :infoSlides";
    if (updatedDebate.debaters is not null && updatedDebate.debaters.Length > 0) updateExpression += ", #debaters = :debaters";
    if (updatedDebate.wings is not null && updatedDebate.wings.Length > 0) updateExpression += ", #wings = :wings";
    if (updatedDebate.sps is not null && updatedDebate.sps.Length > 0) updateExpression += ", #sps = :sps";
    if (updatedDebate.points is not null && updatedDebate.points.Length > 0) updateExpression += ", #points = :points";
    if (updatedDebate.tournament is not null) updateExpression += ", #tournament = :tournament";
    
    UpdateItemRequest updateDebateRequest = new UpdateItemRequest {
      TableName = table,
      Key = updateDebateKey,
      ExpressionAttributeNames = expressionAttributeNames,
      ExpressionAttributeValues = expressionAttributeValues,
      UpdateExpression = updateExpression,
    };

    UpdateItemResponse updateDebateResponse = await client.UpdateItemAsync(updateDebateRequest);

    if (updateDebateResponse.HttpStatusCode.Equals(HttpStatusCode.OK)) return true;
    return false;
  }

  public async Task<bool> DeleteDebate(string id) {
    Dictionary<string, AttributeValue> deleteDebateKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } } 
    };

    DeleteItemRequest deleteDebateRequest = new DeleteItemRequest {
        TableName = table,
        Key = deleteDebateKey,
    };

    var deleteDebateResponse = await client.DeleteItemAsync(deleteDebateRequest);

    if (deleteDebateResponse.HttpStatusCode.Equals(HttpStatusCode.OK)) return true;
    return false;
  }

  private Dictionary<string, AttributeValue> GetMemberDictionary(Member member) {
    return new Dictionary<string, AttributeValue>() {
      { "id", new AttributeValue { S = member.id } },
      { "name", new AttributeValue { S = member.name } },
      { "society", new AttributeValue { S = member.society.ToString() } }
    };
  }
}