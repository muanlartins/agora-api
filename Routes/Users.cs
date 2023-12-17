using Newtonsoft.Json;

public static class UsersRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    UsersService usersService = new UsersService(builder);
    AuthService authService = new AuthService(builder);

    app.MapGet("/user", (HttpRequest request) => {
      User? user = authService.GetUserByRequest(request);

      if (user is null) return Results.BadRequest("Token de autorização inválido.");

      return Results.Ok(user);
    });

    app.MapPost("/user", async (Credentials credentials) => {
      bool? created = await usersService.CreateUser(credentials);

      if (created is not null) return Results.Ok(created);
      return Results.BadRequest("Um usuário com esse login já existe.");
    });

    app.MapGet("/user/verify", async (string login) => {
      bool available = await usersService.VerifyLoginAvailablity(login);

      return available;
    });

    app.MapPost("/user/update", async (HttpRequest request) => {
      User? user = authService.GetUserByRequest(request);
      if (user is null) return Results.BadRequest("Authorization token invalid.");

      using (StreamReader r = new StreamReader(request.Body)) {
        string bodyString = await r.ReadToEndAsync();

        User updatedUser = JsonConvert.DeserializeObject<User>(bodyString)!;

        bool updated = await usersService.UpdateUser(user, updatedUser);

        if (updated) return Results.Ok(updated);
        return Results.BadRequest("Não foi possível atualizar o usuário");
      }
    });
  }
}