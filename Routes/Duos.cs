public static class DuosRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    AuthService authService = new AuthService(builder);
    DuosService duoService = new DuosService(builder);

    app.MapPost("/duo", async (HttpRequest request, Duo duo) => {
      User? user = authService.GetUserByRequest(request);

      if (user is null) return Results.BadRequest("Token de autorização inválido.");

      Duo createdDuo = await duoService.CreateDuo(duo);

      return Results.Ok(createdDuo);
    });

    app.MapGet("/duos", async (HttpRequest request) => {
      User? user = authService.GetUserByRequest(request);

      if (user is null) return Results.BadRequest("Token de autorização inválido.");

      List<Duo> duos = await duoService.GetAllDuos();

      return Results.Ok(duos);
    });
  }
}