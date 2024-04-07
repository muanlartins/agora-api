using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public static class ArticlesRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    AuthService authService = new AuthService(builder);
    ArticlesService articlesService = new ArticlesService(builder);

    app.MapPost("/article", async (HttpRequest request, Article article) => {
      Article createdArticle = await articlesService.CreateArticle(article);

      return Results.Ok(createdArticle);
    });

    app.MapGet("/articles", async (HttpRequest request) => {
      List<Article> articles = await articlesService.GetAllArticles();

      return Results.Ok(articles);
    });

    app.MapGet("/article", async (HttpRequest request, string id) => {
      Article? article = await articlesService.GetArticle(id);

      if (article is null) return Results.BadRequest("Não foi possível encontrar o artigo.");

      return Results.Ok(article);
    });

    app.MapPut("/article", async (HttpRequest request) => {
      using (StreamReader r = new StreamReader(request.Body)) {
        string bodyString = await r.ReadToEndAsync();

        Article updatedArticle = JsonConvert.DeserializeObject<Article>(bodyString)!;

        bool updated = await articlesService.UpdateArticle(updatedArticle);

        if (updated) return Results.Ok("Membro atualizado com sucesso.");
        return Results.BadRequest("Não foi possível atualizar o artigo.");
      }
    });

    app.MapDelete("/article/{id}", async (HttpRequest request, string id) => {
      bool updated = await articlesService.DeleteArticle(id);

      if (updated) return Results.Ok("Membro deletado com sucesso.");
      return Results.BadRequest("Não foi possível deletar o artigo.");
    });
  }
}