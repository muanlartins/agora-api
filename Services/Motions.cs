using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Newtonsoft.Json;

public class MotionsService{
    public AmazonDynamoDBClient client;

    public string table;
    public MotionsService(WebApplicationBuilder builder) {
    table = "motions";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
        Environment.GetEnvironmentVariable("ACCESS_KEY"), 
        Environment.GetEnvironmentVariable("SECRET_KEY")
    );
    client = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
    }
    
    // TODO CreateMotion

    public async Task<Motion> CreateMotion(Motion motion) {
    string id = Guid.NewGuid().ToString();

    Dictionary<string, AttributeValue> createMotionItem = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } }, 
      { "type", new AttributeValue { S = motion.type.ToString() } },
      { "infoSlide", new AttributeValue { S = motion.infoSlide } },
      { "text", new AttributeValue { S = motion.text } },
      { "theme", new AttributeValue { S = motion.theme.ToString() } },
      { "tournament", new AttributeValue { S = motion.tournament } },
      { "round", new AttributeValue { S = motion.round } },
      
    };

    PutItemRequest createMotionRequest = new PutItemRequest {
        TableName = table,
        Item = createMotionItem,
    };

    await client.PutItemAsync(createMotionRequest);
    return new Motion(id, motion.type, motion.infoSlide, motion.text, motion.theme, motion.tournament, motion.round);

  }

  // TODO GetAllMotion
  public async Task<List<Motion>> GetAllMotion() {
    ScanRequest getAllMotionRequest = new ScanRequest { 
      TableName = table
    };

    ScanResponse response = await client.ScanAsync(getAllMotionRequest);

    List<Motion> motions = new List<Motion>();

    foreach (Dictionary<string, AttributeValue> motion in response.Items) 
      motions.Add(JsonConvert.DeserializeObject<Motion>(Document.FromAttributeMap(motion).ToJsonPretty())!);

    return motions;
  }  
  // TODO GetMotion
    public async Task<Motion?> GetMotion(string id) {
    Dictionary<string, AttributeValue> Key = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } } 
    };
    
    GetItemRequest getMotionRequest = new GetItemRequest {
      TableName = table,
      Key = Key,
    };

    GetItemResponse response = await client.GetItemAsync(getMotionRequest);

    if (!response.IsItemSet) return null;

    return JsonConvert.DeserializeObject<Motion>(Document.FromAttributeMap(response.Item).ToJsonPretty());
  }
  
  // TODO UpdateMotion*
  // TODO DeleteMotion*

// TODO GetMotionByTournament
public async Task<List<Motion>> GetMotionsByTournament(string tournament)
{
    ScanRequest scanRequest = new ScanRequest
    {
        TableName = table,
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":tournament", new AttributeValue { S = tournament } }
        },
        FilterExpression = "tournament = :tournament"
    };

    ScanResponse response = await client.ScanAsync(scanRequest);

    List<Motion> motionsByTournament = new List<Motion>();

    foreach (Dictionary<string, AttributeValue> motionData in response.Items)
    {
        Motion motion = JsonConvert.DeserializeObject<Motion>(Document.FromAttributeMap(motionData).ToJsonPretty())!;
        motionsByTournament.Add(motion);
    }

    return motionsByTournament;
}

  // TODO GetNRandomMotion
    /*
      - theme
      - type
      - tournament
    */
    public async Task<List<Motion>> GetNRandomMotion(int n = 1, string? theme = null, string? type = null, string? tournament = null)
    {
    ScanRequest scanRequest = new ScanRequest
    {
        TableName = table,
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>(),
        FilterExpression = ""
    };

    if (!string.IsNullOrEmpty(theme))
    {
        scanRequest.ExpressionAttributeValues[":theme"] = new AttributeValue { S = theme };
        scanRequest.FilterExpression += "theme = :theme";
    }

    if (!string.IsNullOrEmpty(type))
    {
        if (!string.IsNullOrEmpty(scanRequest.FilterExpression))
            scanRequest.FilterExpression += " AND ";
        scanRequest.ExpressionAttributeValues[":type"] = new AttributeValue { S = type };
        scanRequest.FilterExpression += "type = :type";
    }

    if (!string.IsNullOrEmpty(tournament))
    {
        if (!string.IsNullOrEmpty(scanRequest.FilterExpression))
            scanRequest.FilterExpression += " AND ";
        scanRequest.ExpressionAttributeValues[":tournament"] = new AttributeValue { S = tournament };
        scanRequest.FilterExpression += "tournament = :tournament";
    }

    ScanResponse response = await client.ScanAsync(scanRequest);

    List<Motion> motions = new List<Motion>();

    Random random = new Random();
    int totalItems = response.Items.Count;

    if (totalItems > n)
    {
        HashSet<int> selectedIndices = new HashSet<int>();
        while (selectedIndices.Count < n)
        {
            int randomIndex = random.Next(totalItems);
            if (!selectedIndices.Contains(randomIndex))
            {
                selectedIndices.Add(randomIndex);
                Dictionary<string, AttributeValue> motionData = response.Items[randomIndex];
                Motion motion = JsonConvert.DeserializeObject<Motion>(Document.FromAttributeMap(motionData).ToJsonPretty())!;
                motions.Add(motion);
            }
        }
    }
    else
    { // In case we have less than n motions using the filter
        foreach (Dictionary<string, AttributeValue> motionData in response.Items)
        {
            Motion motion = JsonConvert.DeserializeObject<Motion>(Document.FromAttributeMap(motionData).ToJsonPretty())!;
            motions.Add(motion);
        }
    }

    return motions;
  }

}