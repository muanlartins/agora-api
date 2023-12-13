using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
  options.SaveToken = true;
  options.TokenValidationParameters = new TokenValidationParameters {
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
    ValidateIssuer = false,
    ValidateAudience = false,
  };
});

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

WebApplication app = builder.Build();

app.UseAuthentication();

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
  User? user = authService.GetUserByRequest(request);

  if (user is null) return Results.BadRequest("Token de autorização inválido.");

  User? updatedUser = await usersService.GetUser(user.login);

  if (updatedUser is null) return Results.BadRequest("Usuário não encontrado");

  return Results.Ok(authService.GenerateUserToken(updatedUser));
});

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

app.Run();
