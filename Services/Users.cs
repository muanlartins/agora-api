using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

public class UsersService {
  WebApplicationBuilder builder;
  public AmazonDynamoDBClient client;
  public string table;
  public UsersService(WebApplicationBuilder builder) {
    table = "users";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
      builder.Configuration["AWS:AccessKey"], 
      builder.Configuration["AWS:SecretKey"]
    );
    client = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
    this.builder = builder;
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

    User user = JsonConvert.DeserializeObject<User>(Document.FromAttributeMap(response.Item).ToJsonPretty())!;

    string hmacKey = builder.Configuration["PasswordSalt"];
    byte[] hmacKeyBytes = Encoding.UTF8.GetBytes(hmacKey);
    HMACSHA256 hmac = new HMACSHA256(hmacKeyBytes);
    byte[] passwordBytes = Encoding.UTF8.GetBytes(credentials.password);
    byte[] encryptedPasswordBytes = hmac.ComputeHash(passwordBytes);
    string encryptedPassword = BitConverter.ToString(encryptedPasswordBytes).Replace("-", "").ToLower();

    return user.login.Equals(credentials.login) && user.password.Equals(encryptedPassword);
  }
}