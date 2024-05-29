
public abstract class UtilsService {
  public static bool IsPathOpen(string path) {
    if (path.Equals("/auth/login")) return true;
    if (path.Equals("/participants")) return true;
    if (path.StartsWith("/tournament")) return true;

    return false;
  }
}