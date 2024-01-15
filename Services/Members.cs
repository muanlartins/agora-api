using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Newtonsoft.Json;

public class MembersService {
  public AmazonDynamoDBClient client;
  public string table;
  public MembersService(WebApplicationBuilder builder) {
    table = "members";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
      builder.Configuration["AWS:AccessKey"], 
      builder.Configuration["AWS:SecretKey"]
    );
    client = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
  }

  public async Task<Member> CreateMember(Member member) {
    string id = Guid.NewGuid().ToString();

    Dictionary<string, AttributeValue> createMemberItem = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } }, 
      { "name", new AttributeValue { S = member.name } },
      { "society", new AttributeValue { S = member.society.ToString() } },
    };
    
    PutItemRequest createMemberRequest = new PutItemRequest {
      TableName = table,
      Item = createMemberItem,
    };

    await client.PutItemAsync(createMemberRequest);

    return new Member(id, member.name, member.society);
  }

  public async Task<List<Member>> GetAllMembers() {
    ScanRequest getAllMembersRequest = new ScanRequest { 
      TableName = table
    };

    ScanResponse response = await client.ScanAsync(getAllMembersRequest);

    List<Member> members = new List<Member>();

    foreach (Dictionary<string, AttributeValue> member in response.Items) 
      members.Add(JsonConvert.DeserializeObject<Member>(Document.FromAttributeMap(member).ToJsonPretty())!);

    return members;
  }
}