using System.Globalization;
using System.Net;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Newtonsoft.Json;

public class GoalsService {
  public AmazonDynamoDBClient dynamo;
  public string table;
  public GoalsService(WebApplicationBuilder builder) {
    table = "goals";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
      Environment.GetEnvironmentVariable("ACCESS_KEY"), 
      Environment.GetEnvironmentVariable("SECRET_KEY")
    );
    dynamo = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
  }

  public async Task<Goal> CreateGoal(Goal goal) {
    string id = Guid.NewGuid().ToString();

    Dictionary<string, AttributeValue> createGoalItem = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } }, 
      { "title", new AttributeValue { S = goal.title } },
      { "currentCount", new AttributeValue { N = goal.currentCount.ToString() } },
      { "totalCount", new AttributeValue { N = goal.totalCount.ToString() } },
      { "type", new AttributeValue { S = goal.type } },
    };
    
    PutItemRequest createGoalRequest = new PutItemRequest {
      TableName = table,
      Item = createGoalItem,
    };

    if (goal.description is not null) {
      createGoalItem.Add("description", new AttributeValue { S = goal.description });
    }

    await dynamo.PutItemAsync(createGoalRequest);

    return new Goal(id, goal.title, goal.currentCount, goal.totalCount, goal.type, goal.description);
  }

  public async Task<List<Goal>> GetAllGoals() {
    ScanRequest getAllGoalsRequest = new ScanRequest { 
      TableName = table
    };

    ScanResponse response = await dynamo.ScanAsync(getAllGoalsRequest);

    List<Goal> goals = new List<Goal>();

    foreach (Dictionary<string, AttributeValue> goal in response.Items) 
      goals.Add(JsonConvert.DeserializeObject<Goal>(Document.FromAttributeMap(goal).ToJsonPretty())!);

    return goals;
  }

  public async Task<bool> UpdateGoal(Goal updatedGoal) {
    Dictionary<string, AttributeValue> updateGoalKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = updatedGoal.id } } 
    };

    Dictionary<string,string> expressionAttributeNames = new Dictionary<string, string>() {
      {"#title", "title"},
      {"#currentCount", "currentCount"},
      {"#totalCount", "totalCount"},
      {"#type", "type"},
    };

    Dictionary<string,AttributeValue> expressionAttributeValues = new Dictionary<string, AttributeValue> {
        { ":title", new AttributeValue { S = updatedGoal.title } },
        { ":currentCount", new AttributeValue { N = updatedGoal.currentCount.ToString() } },
        { ":totalCount", new AttributeValue { N = updatedGoal.totalCount.ToString() } },
        { ":type", new AttributeValue { S = updatedGoal.type } },
    };

    if (updatedGoal.description is not null) {
      expressionAttributeNames.Add("#description", "description");
      expressionAttributeValues.Add(":description", new AttributeValue { S = updatedGoal.description });
    }

    string updateExpression = "SET #title = :title, #currentCount = :currentCount, #totalCount = :totalCount, #type = :type";

    if (updatedGoal.description is not null) updateExpression += ", #description = :description";
    
    UpdateItemRequest updateGoalRequest = new UpdateItemRequest {
      TableName = table,
      Key = updateGoalKey,
      ExpressionAttributeNames = expressionAttributeNames,
      ExpressionAttributeValues = expressionAttributeValues,
      UpdateExpression = updateExpression,
    };

    UpdateItemResponse updateGoalResponse = await dynamo.UpdateItemAsync(updateGoalRequest);

    if (updateGoalResponse.HttpStatusCode.Equals(HttpStatusCode.OK)) return true;
    return false;
  }

  public async Task<bool> DeleteGoal(string id) {
    Dictionary<string, AttributeValue> deleteGoalKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } } 
    };

    DeleteItemRequest deleteGoalRequest = new DeleteItemRequest {
        TableName = table,
        Key = deleteGoalKey,
    };

    var deleteGoalResponse = await dynamo.DeleteItemAsync(deleteGoalRequest);

    if (deleteGoalResponse.HttpStatusCode.Equals(HttpStatusCode.OK)) return true;
    return false;
  }

  public async Task<Goal?> GetGoal(string id) {
    Dictionary<string, AttributeValue> Key = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } } 
    };
    
    GetItemRequest request = new GetItemRequest {
      TableName = table,
      Key = Key,
    };

    GetItemResponse response = await dynamo.GetItemAsync(request);

    if (!response.IsItemSet) return null;

    return JsonConvert.DeserializeObject<Goal>(Document.FromAttributeMap(response.Item).ToJsonPretty());
  }
}