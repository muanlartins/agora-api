
public abstract class UtilsService {
  public static bool IsPathOpen(string path) {
    if (path.Equals("/auth/login")) return true;
    if (path.Equals("/participants")) return true;

    return false;
  }
}