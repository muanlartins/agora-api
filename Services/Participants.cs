
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
      builder.Configuration["AWS:AccessKey"], 
      builder.Configuration["AWS:SecretKey"]
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
    Dictionary<string, AttributeValue> createParticipantItem = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = participant.id } }, 
      { "tournament", new AttributeValue { S = participant.tournament } }, 
      { "name", new AttributeValue { S = participant.name } },
      { "society", new AttributeValue { S = participant.society } },
      { "subscribedAt", new AttributeValue { S = participant.subscribedAt } },
      { "hasPfp", new AttributeValue { BOOL = false } }
    };
    
    PutItemRequest createParticipantRequest = new PutItemRequest {
      TableName = table,
      Item = createParticipantItem,
    };

    await dynamo.PutItemAsync(createParticipantRequest);

    return participant;
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