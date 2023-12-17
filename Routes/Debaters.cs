public static class DebatersRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    AuthService authService = new AuthService(builder);
    DebatersService debatersService = new DebatersService(builder);

    app.MapPost("/debater", async (HttpRequest request, Debater debater) => {
      User? user = authService.GetUserByRequest(request);

      if (user is null) return Results.BadRequest("Token de autorização inválido.");

      Debater createdDebater = await debatersService.CreateDebater(debater);

      return Results.Ok(createdDebater);
    });

    app.MapGet("/debaters", async (HttpRequest request) => {
      User? user = authService.GetUserByRequest(request);

      if (user is null) return Results.BadRequest("Token de autorização inválido.");

      List<Debater> debaters = await debatersService.GetAllDebaters();

      return Results.Ok(debaters);
    });
  }
}