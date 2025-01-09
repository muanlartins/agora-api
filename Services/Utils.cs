
using System.Security.Cryptography;
using System.Text;

public abstract class UtilsService {
  public UtilsService() {}

  public static bool IsPathOpen(HttpRequest request) {
    string path = request.Path;

    bool isPathPublic = path.StartsWith("/public");
    bool isPreflightRequest = HttpMethod.Options.ToString() == request.Method.ToString();

    return isPathPublic || isPreflightRequest;
  }

  public static string Hash(WebApplicationBuilder builder, string data) {
    string hmacKey = Environment.GetEnvironmentVariable("PASSWORD_SALT")!;
    byte[] hmacKeyBytes = Encoding.UTF8.GetBytes(hmacKey);
    HMACSHA256 hmac = new HMACSHA256(hmacKeyBytes);
    byte[] dataBytes = Encoding.UTF8.GetBytes(data);
    byte[] encryptedDataBytes = hmac.ComputeHash(dataBytes);
    return BitConverter.ToString(encryptedDataBytes).Replace("-", "").ToLower();
  }
}