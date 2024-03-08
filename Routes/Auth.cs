public static class AuthRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    UsersService usersService = new UsersService(builder);
    AuthService authService = new AuthService(builder);

    app.MapPost("/auth/login", async (Credentials credentials) => {
      bool credentialsVerified = await usersService.VerifyUserCredentials(credentials);

      if (!credentialsVerified) return Results.BadRequest("O login ou senha está incorreta.");

      User? user = await usersService.GetUser(credentials.login);

      if (user is null) return Results.BadRequest("Um erro inesperado aconteceu.");

      return Results.Ok(authService.GenerateUserToken(user));
    });

    app.MapGet("/auth/refresh", async (HttpRequest request) => {
      User user = (User)request.HttpContext.Items["user"]!;

      User? updatedUser = await usersService.GetUser(user.login);

      if (updatedUser is null) return Results.BadRequest("Usuário não encontrado");

      return Results.Ok(authService.GenerateUserToken(updatedUser));
    });
  }
}