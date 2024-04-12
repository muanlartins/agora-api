using System.Net;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Newtonsoft.Json;

public class MembersService {
  public AmazonDynamoDBClient dynamo;
  public string table;
  public AmazonS3Client s3;
  public MembersService(WebApplicationBuilder builder) {
    table = "members";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
      builder.Configuration["AWS:AccessKey"], 
      builder.Configuration["AWS:SecretKey"]
    );
    dynamo = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
    s3 = new AmazonS3Client(credentials, RegionEndpoint.SAEast1);
  }

  public async Task<Member> CreateMember(Member member) {
    string id = Guid.NewGuid().ToString();

    Dictionary<string, AttributeValue> createMemberItem = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } }, 
      { "name", new AttributeValue { S = member.name } },
      { "society", new AttributeValue { S = member.society.ToString() } },
      { "isTrainee", new AttributeValue { BOOL = member.isTrainee } },
      { "hasPfp", new AttributeValue { BOOL = member.hasPfp } },
      { "blocked", new AttributeValue { BOOL = member.blocked } },
    };
    
    PutItemRequest createMemberRequest = new PutItemRequest {
      TableName = table,
      Item = createMemberItem,
    };

    await dynamo.PutItemAsync(createMemberRequest);

    return new Member(id, member.name, member.society, member.isTrainee, member.hasPfp, member.blocked);
  }

  public async Task<List<Member>> GetAllMembers() {
    ScanRequest getAllMembersRequest = new ScanRequest { 
      TableName = table
    };

    ScanResponse response = await dynamo.ScanAsync(getAllMembersRequest);

    List<Member> members = new List<Member>();

    foreach (Dictionary<string, AttributeValue> member in response.Items) 
      members.Add(JsonConvert.DeserializeObject<Member>(Document.FromAttributeMap(member).ToJsonPretty())!);

    return members;
  }

  public async Task<bool> UpdateMember(Member updatedMember) {
    Dictionary<string, AttributeValue> updateMemberKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = updatedMember.id } } 
    };

    Dictionary<string,string> expressionAttributeNames = new Dictionary<string, string>() {
      {"#N", "name"},
      {"#S", "society"},
      { "#IT", "isTrainee"},
      { "#HP", "hasPfp"},
      { "#B", "blocked"},
    };

    Dictionary<string,AttributeValue> expressionAttributeValues = new Dictionary<string, AttributeValue> {
        { ":n", new AttributeValue { S = updatedMember.name } },
        { ":s", new AttributeValue { S = updatedMember.society.ToString() } },
        { ":it", new AttributeValue { BOOL = updatedMember.isTrainee } },
        { ":hp", new AttributeValue { BOOL = updatedMember.hasPfp } },
        { ":b", new AttributeValue { BOOL = updatedMember.blocked } }
    };

    string updateExpression = "SET #N = :n, #S = :s, #IT = :it, #HP = :hp, #B = :b";
    
    UpdateItemRequest updateMemberRequest = new UpdateItemRequest {
      TableName = table,
      Key = updateMemberKey,
      ExpressionAttributeNames = expressionAttributeNames,
      ExpressionAttributeValues = expressionAttributeValues,
      UpdateExpression = updateExpression,
    };

    UpdateItemResponse updateMemberResponse = await dynamo.UpdateItemAsync(updateMemberRequest);

    if (updateMemberResponse.HttpStatusCode.Equals(HttpStatusCode.OK)) return true;
    return false;
  }

  public async Task<bool> DeleteMember(string id) {
    Dictionary<string, AttributeValue> deleteMemberKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } } 
    };

    DeleteItemRequest deleteMemberRequest = new DeleteItemRequest {
        TableName = table,
        Key = deleteMemberKey,
    };

    var deleteMemberResponse = await dynamo.DeleteItemAsync(deleteMemberRequest);

    if (deleteMemberResponse.HttpStatusCode.Equals(HttpStatusCode.OK)) return true;
    return false;
  }

  public async Task<Member?> GetMember(string id) {
    Dictionary<string, AttributeValue> Key = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } } 
    };
    
    GetItemRequest request = new GetItemRequest {
      TableName = table,
      Key = Key,
    };

    GetItemResponse response = await dynamo.GetItemAsync(request);

    if (!response.IsItemSet) return null;

    return JsonConvert.DeserializeObject<Member>(Document.FromAttributeMap(response.Item).ToJsonPretty());
  }

  public async Task<bool> UploadMemberPfp(string fileName, MemoryStream fileStream) {
    TransferUtility fileTransferUtility = new TransferUtility(s3);

    string fileKey = $"assets/pfps/{fileName}";

    await fileTransferUtility.UploadAsync(fileStream, "agorasdufrj", fileKey);

    Dictionary<string, AttributeValue> updateMemberKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = fileName } } 
    };

    Dictionary<string,string> expressionAttributeNames = new Dictionary<string, string>() {
      { "#HP", "hasPfp"},
    };

    Dictionary<string,AttributeValue> expressionAttributeValues = new Dictionary<string, AttributeValue> {
      { ":hp", new AttributeValue { BOOL = true } }
    };

    string updateExpression = "SET #HP = :hp";
    
    UpdateItemRequest updateMemberRequest = new UpdateItemRequest {
      TableName = table,
      Key = updateMemberKey,
      ExpressionAttributeNames = expressionAttributeNames,
      ExpressionAttributeValues = expressionAttributeValues,
      UpdateExpression = updateExpression,
    };

    await dynamo.UpdateItemAsync(updateMemberRequest);

    return true;
  }
}