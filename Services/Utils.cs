
using System.Security.Cryptography;
using System.Text;

public abstract class UtilsService {
  public UtilsService() {}

  public static bool IsPathOpen(string path, string method) {
    if (path.Equals("/auth/login")) return true;

    if (method != HttpMethod.Get.ToString()) return false;

    if (path.Equals("/participants")) return true;
    if (path.StartsWith("/tournament")) return true;
    if (path.StartsWith("/member/")) return true;

    return false;
  }

  public static string Hash(WebApplicationBuilder builder, string data) {
    string hmacKey = builder.Configuration["PasswordSalt"];
    byte[] hmacKeyBytes = Encoding.UTF8.GetBytes(hmacKey);
    HMACSHA256 hmac = new HMACSHA256(hmacKeyBytes);
    byte[] dataBytes = Encoding.UTF8.GetBytes(data);
    byte[] encryptedDataBytes = hmac.ComputeHash(dataBytes);
    return BitConverter.ToString(encryptedDataBytes).Replace("-", "").ToLower();
  }
}