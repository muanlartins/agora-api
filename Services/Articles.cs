using System.Globalization;
using System.Net;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Newtonsoft.Json;

public class ArticlesService {
  public AmazonDynamoDBClient dynamo;
  public string table;
  public ArticlesService(WebApplicationBuilder builder) {
    table = "articles";
    BasicAWSCredentials credentials = new BasicAWSCredentials(
      Environment.GetEnvironmentVariable("ACCESS_KEY"), 
      Environment.GetEnvironmentVariable("SECRET_KEY")
    );
    dynamo = new AmazonDynamoDBClient(credentials, RegionEndpoint.SAEast1);
  }

  public async Task<Article> CreateArticle(Article article) {
    string id = Guid.NewGuid().ToString();

    string createdAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
    string updatedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

    Dictionary<string, AttributeValue> createArticleItem = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } }, 
      { "title", new AttributeValue { S = article.title } },
      { "content", new AttributeValue { S = article.content } },
      { "tag", new AttributeValue { S = article.tag } },
      { "authorId", new AttributeValue { S = article.authorId } },
      { "createdAt", new AttributeValue { S = createdAt } },
      { "updatedAt", new AttributeValue { S = updatedAt } },
    };
    
    PutItemRequest createArticleRequest = new PutItemRequest {
      TableName = table,
      Item = createArticleItem,
    };

    await dynamo.PutItemAsync(createArticleRequest);

    return new Article(id, article.title, article.content, article.tag, article.authorId, createdAt, updatedAt);
  }

  public async Task<List<Article>> GetAllArticles() {
    ScanRequest getAllArticlesRequest = new ScanRequest { 
      TableName = table
    };

    ScanResponse response = await dynamo.ScanAsync(getAllArticlesRequest);

    List<Article> articles = new List<Article>();

    foreach (Dictionary<string, AttributeValue> article in response.Items) 
      articles.Add(JsonConvert.DeserializeObject<Article>(Document.FromAttributeMap(article).ToJsonPretty())!);

    return articles;
  }

  public async Task<bool> UpdateArticle(Article updatedArticle) {
    string updatedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

    Dictionary<string, AttributeValue> updateArticleKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = updatedArticle.id } } 
    };

    Dictionary<string,string> expressionAttributeNames = new Dictionary<string, string>() {
      {"#title", "title"},
      {"#content", "content"},
      {"#tag", "tag"},
      {"#authorId", "authorId"},
      {"#updatedAt", "updatedAt"},
    };

    Dictionary<string,AttributeValue> expressionAttributeValues = new Dictionary<string, AttributeValue> {
        { ":title", new AttributeValue { S = updatedArticle.title } },
        { ":content", new AttributeValue { S = updatedArticle.content } },
        { ":tag", new AttributeValue { S = updatedArticle.tag } },
        { ":authorId", new AttributeValue { S = updatedArticle.authorId } },
        { ":updatedAt", new AttributeValue { S = updatedAt } },
    };

    string updateExpression = "SET #title = :title, #content = :content, #tag = :tag, #authorId = :authorId, #updatedAt = :updatedAt";
    
    UpdateItemRequest updateArticleRequest = new UpdateItemRequest {
      TableName = table,
      Key = updateArticleKey,
      ExpressionAttributeNames = expressionAttributeNames,
      ExpressionAttributeValues = expressionAttributeValues,
      UpdateExpression = updateExpression,
    };

    UpdateItemResponse updateArticleResponse = await dynamo.UpdateItemAsync(updateArticleRequest);

    if (updateArticleResponse.HttpStatusCode.Equals(HttpStatusCode.OK)) return true;
    return false;
  }

  public async Task<bool> DeleteArticle(string id) {
    Dictionary<string, AttributeValue> deleteArticleKey = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } } 
    };

    DeleteItemRequest deleteArticleRequest = new DeleteItemRequest {
        TableName = table,
        Key = deleteArticleKey,
    };

    var deleteArticleResponse = await dynamo.DeleteItemAsync(deleteArticleRequest);

    if (deleteArticleResponse.HttpStatusCode.Equals(HttpStatusCode.OK)) return true;
    return false;
  }

  public async Task<Article?> GetArticle(string id) {
    Dictionary<string, AttributeValue> Key = new Dictionary<string, AttributeValue>() { 
      { "id", new AttributeValue { S = id } } 
    };
    
    GetItemRequest request = new GetItemRequest {
      TableName = table,
      Key = Key,
    };

    GetItemResponse response = await dynamo.GetItemAsync(request);

    if (!response.IsItemSet) return null;

    return JsonConvert.DeserializeObject<Article>(Document.FromAttributeMap(response.Item).ToJsonPretty());
  }
}