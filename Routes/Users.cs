using Newtonsoft.Json;

public static class UsersRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    UsersService usersService = new UsersService(builder);
    AuthService authService = new AuthService(builder);

    app.MapGet("/user", (HttpRequest request) => {
      User user = (User)request.HttpContext.Items["user"]!;

      return Results.Ok(user);
    });

    app.MapPost("/user", async (Credentials credentials, string jwtKey) => {
      if (Environment.GetEnvironmentVariable("JWT_KEY") != jwtKey) return Results.BadRequest("Chave JWT errada.");

      bool? created = await usersService.CreateUser(credentials);

      if (created is not null) return Results.Ok(created);
      return Results.BadRequest("Um usuário com esse login já existe.");
    });

    app.MapGet("/user/verify", async (string login) => {
      bool available = await usersService.VerifyLoginAvailablity(login);

      return available;
    });
  }
}