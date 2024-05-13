using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

public class AuthService {
  WebApplicationBuilder builder;
  public AuthService(WebApplicationBuilder builder) {
    this.builder = builder;
  }

  public User? GetUserByRequest(HttpRequest request) {
    string[]? authorizationHeader = request.Headers.Authorization;

    if (authorizationHeader is null) return null;

    string authorization = authorizationHeader[0];

    if (authorization.Split(' ').Length < 2) return null;

    string token = authorization.Split(' ')[1];

    try {
      JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
      TokenValidationParameters validationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = false,
        ValidateAudience = false,
      };

      tokenHandler.ValidateToken(
        token, 
        validationParameters, 
        out SecurityToken validatedToken
      );
      
      if (
        validatedToken is JwtSecurityToken jwtSecurityToken && 
        jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)
      ) return JsonConvert.DeserializeObject<User>(validatedToken.ToString()!.Split(".")[1]);

      return null;
    } catch {
      return null;
    }
  }

  public string GenerateUserToken(User user) {
    return GenerateJwtToken(user);
  }

  private string GenerateJwtToken(User user) {
    var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = GenerateClaimsIdentity(user),
      Expires = DateTime.UtcNow.AddDays(1),
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
  }

  private ClaimsIdentity GenerateClaimsIdentity(User user) {
    return new ClaimsIdentity(new Claim[] {
      new Claim("login", user.login),
      new Claim("role", user.role.ToString()),
    });
  }
}