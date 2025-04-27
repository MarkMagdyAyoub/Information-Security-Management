using BCrypt.Net;
using System.Text.RegularExpressions;

namespace SecureLoginSys.Controllers.Utilities;

public static class PasswordHelper
{
  public static bool IsValidPassword(this string password, out string errorMsg) {
    errorMsg = string.Empty;
    
    if (password.Length < 8)
    {
      errorMsg = "Password must be at least 8 characters long.";
      return false;
    }

    if (!Regex.IsMatch(password, @"[A-Z]"))
    {
      errorMsg = "Password must contain at least one uppercase letter.";
      return false;
    }

    if (!Regex.IsMatch(password, @"[a-z]"))
    {
      errorMsg = "Password must contain at least one lowercase letter.";
      return false;
    }

    if (!Regex.IsMatch(password, @"[0-9]"))
    {
      errorMsg = "Password must contain at least one number.";
      return false;
    }

    if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]"))
    {
      errorMsg = "Password must contain at least one special character.";
      return false;
    }

    return true;
  }

  public static string HashPassword(this string password)
  {
    return BCrypt.Net.BCrypt.HashPassword(password);
  }

  public static bool VerifyPassword(this string password, string hashedPassword)
  {
    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
  }
}
