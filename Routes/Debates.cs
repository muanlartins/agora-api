public static class DebatesRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    AuthService authService = new AuthService(builder);
    DebatesService debatesService = new DebatesService(builder);

    app.MapPost("/debate", async (HttpRequest request, Debate debate) => {
      User? user = authService.GetUserByRequest(request);

      if (user is null) return Results.BadRequest("Token de autorização inválido.");

      Debate createdDebate = await debatesService.CreateDebate(debate);

      return Results.Ok(createdDebate);
    });

    app.MapGet("/debate", async (HttpRequest request, string id) => {
      User? user = authService.GetUserByRequest(request);

      if (user is null) return Results.BadRequest("Token de autorização inválido.");

      Debate? debate = await debatesService.GetDebate(id);

      if (debate is null) return Results.BadRequest("Não foi possível encontrar o debate.");

      return Results.Ok(debate);
    });

    app.MapGet("/debates", async (HttpRequest request) => {
      User? user = authService.GetUserByRequest(request);

      if (user is null) return Results.BadRequest("Token de autorização inválido.");

      List<Debate> debates = await debatesService.GetAllDebates();

      return Results.Ok(debates);
    });
  }
}