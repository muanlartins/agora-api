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
        policy.WithOrigins("*").AllowAnyOrigin().AllowAnyHeader()
    )
);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseCors();

AuthRoute.GetRoutes(app, builder);
UsersRoute.GetRoutes(app, builder);
JudgesRoute.GetRoutes(app, builder);
DebatersRoute.GetRoutes(app, builder);
DebatesRoute.GetRoutes(app, builder);

app.Run();
