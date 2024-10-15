
using System.Net;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Newtonsoft.Json;

public class ParticipantsService {
  public AmazonDynamoDBClient dynamo;
  public string table;
  public AmazonS3Client s3;
  public ParticipantsService(WebApplicationBuilder builder) {
    table = "participants";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
      Environment.GetEnvironmentVariable("ACCESS_KEY"), 
      Environment.GetEnvironmentVariable("SECRET_KEY")
    );
    dynamo = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
    s3 = new AmazonS3Client(credentials, RegionEndpoint.SAEast1);
  }

  public async Task<List<Participant>> GetAllParticipants() {
    ScanRequest getAllParticipantsRequest = new ScanRequest { 
      TableName = table
    };

    ScanResponse response = await dynamo.ScanAsync(getAllParticipantsRequest);

    List<Participant> participants = new List<Participant>();

    foreach (Dictionary<string, AttributeValue> participant in response.Items) 
      participants.Add(JsonConvert.DeserializeObject<Participant>(Document.FromAttributeMap(participant).ToJsonPretty())!);

    return participants;
  }

  public async Task<Participant> CreateParticipant(Participant participant) {
    string id = Guid.NewGuid().ToString();

    Dictionary<string, AttributeValue> createParticipantItem = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } }, 
      { "tournament", new AttributeValue { S = participant.tournament } }, 
      { "name", new AttributeValue { S = participant.name } },
      { "society", new AttributeValue { S = participant.society } },
      { "subscribedAt", new AttributeValue { S = participant.subscribedAt } },
      { "hasPfp", new AttributeValue { BOOL = participant.hasPfp } },
      { "category", new AttributeValue { S = participant.category.ToString() } },
    };
    
    if (participant.roles is not null && participant.roles.Length > 0) {
      List<AttributeValue> roles = new List<AttributeValue>();
      foreach (TournamentRole role in participant.roles) 
        roles.Add(new AttributeValue { S = role.ToString() });

      createParticipantItem.Add("roles", new AttributeValue { L = roles });
    }

    if (participant.duoId is not null) {
      createParticipantItem.Add("duoId", new AttributeValue { S = participant.duoId });
    }

    if (participant.emoji is not null) {
      createParticipantItem.Add("emoji", new AttributeValue { S = participant.emoji });
    }
    
    PutItemRequest createParticipantRequest = new PutItemRequest {
      TableName = table,
      Item = createParticipantItem,
    };

    await dynamo.PutItemAsync(createParticipantRequest);

    return new Participant(
      id, 
      participant.tournament, 
      participant.name, 
      participant.society, 
      participant.subscribedAt, 
      false,
      category: participant.category,
      roles: participant.roles,
      duoId: participant.duoId,
      emoji: participant.emoji
    );
  }

  public async Task<bool> RegisterDuo(string participantOneId, string participantTwoId) {
    Dictionary<string, AttributeValue> updateParticipantOneKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = participantOneId } } 
    };
    Dictionary<string, AttributeValue> updateParticipantTwoKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = participantTwoId } } 
    };

    Dictionary<string,string> expressionAttributeNames = new Dictionary<string, string>() {
      { "#DI", "duoId" },
    };

    Dictionary<string,AttributeValue> expressionOneAttributeValues = new Dictionary<string, AttributeValue> {
        { ":di", new AttributeValue { S = participantTwoId } },
    };

    Dictionary<string,AttributeValue> expressionTwoAttributeValues = new Dictionary<string, AttributeValue> {
        { ":di", new AttributeValue { S = participantOneId } },
    };

    string updateExpression = "SET #DI = :di";

    UpdateItemRequest updateParticipantOneRequest = new UpdateItemRequest {
      TableName = table,
      Key = updateParticipantOneKey,
      ExpressionAttributeNames = expressionAttributeNames,
      ExpressionAttributeValues = expressionOneAttributeValues,
      UpdateExpression = updateExpression,
    };

    UpdateItemRequest updateParticipantTwoRequest = new UpdateItemRequest {
      TableName = table,
      Key = updateParticipantTwoKey,
      ExpressionAttributeNames = expressionAttributeNames,
      ExpressionAttributeValues = expressionTwoAttributeValues,
      UpdateExpression = updateExpression,
    };

    UpdateItemResponse updateParticipantOneResponse = await dynamo.UpdateItemAsync(updateParticipantOneRequest);
    UpdateItemResponse updateParticipantTwoResponse = await dynamo.UpdateItemAsync(updateParticipantTwoRequest);

    if (
      updateParticipantOneResponse.HttpStatusCode.Equals(HttpStatusCode.OK) && 
      updateParticipantTwoResponse.HttpStatusCode.Equals(HttpStatusCode.OK)
    ) return true;

    return false;
  }

  public async Task<bool> UpdateParticipant(Participant updatedParticipant) {
    Dictionary<string, AttributeValue> updateParticipantKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = updatedParticipant.id } } 
    };

    Dictionary<string,string> expressionAttributeNames = new Dictionary<string, string>() {
      { "#T", "tournament" },
      { "#N", "name" },
      { "#S", "society" },
      { "#SA", "subscribedAt" },
      { "#HP", "hasPfp" },
      { "#C", "category" },
    };

    Dictionary<string,AttributeValue> expressionAttributeValues = new Dictionary<string, AttributeValue> {
        { ":t", new AttributeValue { S = updatedParticipant.tournament } },
        { ":n", new AttributeValue { S = updatedParticipant.name } },
        { ":s", new AttributeValue { S = updatedParticipant.society } },
        { ":sa", new AttributeValue { S = updatedParticipant.subscribedAt } },
        { ":hp", new AttributeValue { BOOL = updatedParticipant.hasPfp } },
        { ":c", new AttributeValue { S = updatedParticipant.category.ToString() } },
    };

    if (updatedParticipant.roles is not null && updatedParticipant.roles.Length > 0) {
      List<AttributeValue> roles = new List<AttributeValue>();
      foreach (TournamentRole role in updatedParticipant.roles) 
        roles.Add(new AttributeValue { S = role.ToString() });

      expressionAttributeNames.Add("#R", "roles");
      expressionAttributeValues.Add(":r", new AttributeValue { L = roles });
    }

    if (updatedParticipant.duoId is not null) {
      expressionAttributeNames.Add("#DI", "duoId");
      expressionAttributeValues.Add(":di", new AttributeValue { S = updatedParticipant.duoId });
    }

    if (updatedParticipant.emoji is not null) {
      expressionAttributeNames.Add("#E", "emoji");
      expressionAttributeValues.Add(":e", new AttributeValue { S = updatedParticipant.emoji });
    }

    string updateExpression = "SET #T = :t, #N = :n, #S = :s, #SA = :sa, #HP = :hp, #C = :c";

    if (updatedParticipant.roles is not null && updatedParticipant.roles.Length > 0) updateExpression += ", #R = :r";
    if (updatedParticipant.duoId is not null) updateExpression += ", #DI = :di";
    if (updatedParticipant.emoji is not null) updateExpression += ", #E = :e";
    
    UpdateItemRequest updateParticipantRequest = new UpdateItemRequest {
      TableName = table,
      Key = updateParticipantKey,
      ExpressionAttributeNames = expressionAttributeNames,
      ExpressionAttributeValues = expressionAttributeValues,
      UpdateExpression = updateExpression,
    };

    UpdateItemResponse updateParticipantResponse = await dynamo.UpdateItemAsync(updateParticipantRequest);

    if (updateParticipantResponse.HttpStatusCode.Equals(HttpStatusCode.OK)) return true;
    return false;
  }

    public async Task<bool> DeleteParticipant(string id) {
    Dictionary<string, AttributeValue> deleteParticipantKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } } 
    };

    DeleteItemRequest deleteParticipantRequest = new DeleteItemRequest {
        TableName = table,
        Key = deleteParticipantKey,
    };

    var deleteParticipantResponse = await dynamo.DeleteItemAsync(deleteParticipantRequest);

    if (deleteParticipantResponse.HttpStatusCode.Equals(HttpStatusCode.OK)) return true;
    return false;
  }

  public async Task<bool> UploadParticipantPfp(string tournament, string fileName, MemoryStream fileStream) {
    TransferUtility fileTransferUtility = new TransferUtility(s3);

    string fileKey = $"assets/participants/{tournament}/{fileName}";

    await fileTransferUtility.UploadAsync(fileStream, "agoradebates.com", fileKey);

    Dictionary<string, AttributeValue> updateParticipantKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = fileName } } 
    };

    Dictionary<string,string> expressionAttributeNames = new Dictionary<string, string>() {
      { "#HP", "hasPfp"},
    };

    Dictionary<string,AttributeValue> expressionAttributeValues = new Dictionary<string, AttributeValue> {
      { ":hp", new AttributeValue { BOOL = true } }
    };

    string updateExpression = "SET #HP = :hp";
    
    UpdateItemRequest updateParticipantRequest = new UpdateItemRequest {
      TableName = table,
      Key = updateParticipantKey,
      ExpressionAttributeNames = expressionAttributeNames,
      ExpressionAttributeValues = expressionAttributeValues,
      UpdateExpression = updateExpression,
    };

    await dynamo.UpdateItemAsync(updateParticipantRequest);

    return true;
  }
}