using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Newtonsoft.Json;
using System.Net;

public class UsersService {
  public AmazonDynamoDBClient client;
  public string table;
  public UsersService(WebApplicationBuilder builder) {
    table = "users";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
      builder.Configuration["AWS:AccessKey"], 
      builder.Configuration["AWS:SecretKey"]
    );
    client = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
  }

  public async Task<bool?> CreateUser(Credentials credentials) {
    Dictionary<string, AttributeValue> getUserKey = new Dictionary<string, AttributeValue>() { 
      { "login", new AttributeValue { S = credentials.login } } 
    };
    
    GetItemRequest getUserRequest = new GetItemRequest {
      TableName = table,
      Key = getUserKey,
    };

    GetItemResponse getUserResponse = await client.GetItemAsync(getUserRequest);

    if (getUserResponse.IsItemSet) return null;

    Dictionary<string, AttributeValue> createUserItem = new Dictionary<string, AttributeValue>() { 
      { "login", new AttributeValue { S = credentials.login } }, 
      { "password", new AttributeValue { S = credentials.password } }
    };
    
    PutItemRequest createUserRequest = new PutItemRequest {
      TableName = table,
      Item = createUserItem,
    };

    await client.PutItemAsync(createUserRequest);

    return true;
  }

  public async Task<User?> GetUser(string login) {
    Dictionary<string, AttributeValue> Key = new Dictionary<string, AttributeValue>() { 
      { "login", new AttributeValue { S = login } } 
    };
    
    GetItemRequest request = new GetItemRequest {
      TableName = table,
      Key = Key,
    };

    GetItemResponse response = await client.GetItemAsync(request);

    if (!response.IsItemSet) return null;

    Document document = Document.FromAttributeMap(response.Item);

    return JsonConvert.DeserializeObject<User>(document.ToJsonPretty());
  }

  public async Task<bool> UpdateUser(User user, User updatedUser) {
    Dictionary<string, AttributeValue> updateUserKey = new Dictionary<string, AttributeValue>() { 
      { "login", new AttributeValue { S = user.login } } 
    };

    Dictionary<string,string> expressionAttributeNames = new Dictionary<string, string>() {
      {"#F", "fullName"},
      {"#N", "nickname"}
    };

    Dictionary<string,AttributeValue> expressionAttributeValues = new Dictionary<string, AttributeValue> {
        { ":f", new AttributeValue { S = updatedUser.fullName } },
        { ":n", new AttributeValue { S = updatedUser.nickname } }
    };

    string updateExpression = "SET #F = :f, #N = :n";
    
    UpdateItemRequest updateUserRequest = new UpdateItemRequest {
      TableName = table,
      Key = updateUserKey,
      ExpressionAttributeNames = expressionAttributeNames,
      ExpressionAttributeValues = expressionAttributeValues,
      UpdateExpression = updateExpression,
    };

    UpdateItemResponse updateUserResponse = await client.UpdateItemAsync(updateUserRequest);

    if (updateUserResponse.HttpStatusCode.Equals(HttpStatusCode.OK)) return true;
    return false;
  }

  public async Task<bool> VerifyLoginAvailablity(string login) {
    Dictionary<string, AttributeValue> Key = new Dictionary<string, AttributeValue>() { 
      { "login", new AttributeValue { S = login } } 
    };
    
    GetItemRequest request = new GetItemRequest {
      TableName = table,
      Key = Key,
    };

    GetItemResponse response = await client.GetItemAsync(request);

    return !response.IsItemSet;
  }

  public async Task<bool> VerifyUserCredentials(Credentials credentials) {
    Dictionary<string, AttributeValue> loginKey = new Dictionary<string, AttributeValue>() { 
      { "login", new AttributeValue { S = credentials.login } } 
    };
    
    GetItemRequest getUserRequest = new GetItemRequest {
      TableName = table,
      Key = loginKey,
    };

    GetItemResponse response = await client.GetItemAsync(getUserRequest);

    if (!response.IsItemSet) return false;

    Document document = Document.FromAttributeMap(response.Item);

    User user = JsonConvert.DeserializeObject<User>(document.ToJsonPretty())!;

    return user.login.Equals(credentials.login) && user.password.Equals(credentials.password);
  }
}