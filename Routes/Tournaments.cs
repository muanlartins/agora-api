public static class TournamentsRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    UsersService usersService = new UsersService(builder);
    AuthService authService = new AuthService(builder);
    TournamentsService tournamentsService = new TournamentsService(builder);

    app.MapGet("/tournaments", async (HttpRequest request) => {
      Dictionary<string, object> data = await tournamentsService.GetAllTournamentsTabbyData();

      return Results.Ok(data);
    });

    app.MapGet("/tournaments/list", async (HttpRequest request) => {
      List<string> data = await tournamentsService.GetAllTournamentOptions();

      return Results.Ok(data);
    });

    app.MapGet("/tournament/{tournament}", async (HttpRequest request, string tournament) => {
      object data = await tournamentsService.GetTournamentTabbyData(tournament);

      return Results.Ok(data);
    });
  }
}