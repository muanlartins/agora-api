public static class TournamentsRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    UsersService usersService = new UsersService(builder);
    AuthService authService = new AuthService(builder);
    TournamentsService tournamentsService = new TournamentsService(builder);

    app.MapGet("/tournament/{tournament}", async (HttpRequest request, string tournament) => {
      object data = await tournamentsService.GetTournamentTabbyData(tournament);

      return Results.Ok(data);
    });
  }
}