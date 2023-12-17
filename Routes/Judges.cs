public static class JudgesRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    AuthService authService = new AuthService(builder);
    JudgesService judgesService = new JudgesService(builder);

    app.MapPost("/judge", async (HttpRequest request, Judge judge) => {
      User? user = authService.GetUserByRequest(request);

      if (user is null) return Results.BadRequest("Token de autorização inválido.");

      Judge createdJudge = await judgesService.CreateJudge(judge);

      return Results.Ok(createdJudge);
    });

    app.MapGet("/judges", async (HttpRequest request) => {
      User? user = authService.GetUserByRequest(request);

      if (user is null) return Results.BadRequest("Token de autorização inválido.");

      List<Judge> judges = await judgesService.GetAllJudges();

      return Results.Ok(judges);
    });
  }
}