using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
  options.SaveToken = true;
  options.TokenValidationParameters = new TokenValidationParameters {
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
    ValidateIssuer = false,
    ValidateAudience = false,
  };
});

builder.Services.AddCors(
  policyBuilder => 
    policyBuilder.AddDefaultPolicy(
      policy =>
        policy.WithOrigins("*").AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
    )
);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

WebApplication app = builder.Build();

app.Use(async (context, next) => {
  if (
    HttpMethod.Options.ToString() == context.Request.Method.ToString() || 
    UtilsService.IsPathOpen(context.Request.Path, context.Request.Method)
  ) {
    await next.Invoke();
    return;
  }

  AuthService authService = new AuthService(builder);
  User? user = authService.GetUserByRequest(context.Request);

  if (user is null) {
    context.Response.Clear();
    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
    await context.Response.WriteAsync("Token de autorização inválido.");

    return;
  }

  context.Items["user"] = user;

  await next.Invoke();
});

app.UseAuthentication();
app.UseCors();

AuthRoute.GetRoutes(app, builder);
UsersRoute.GetRoutes(app, builder);
MembersRoute.GetRoutes(app, builder);
DebatesRoute.GetRoutes(app, builder);
ArticlesRoute.GetRoutes(app, builder);
ParticipantsRoute.GetRoutes(app, builder);
TournamentsRoute.GetRoutes(app, builder);
GoalsRoute.GetRoutes(app, builder);

app.Run();
