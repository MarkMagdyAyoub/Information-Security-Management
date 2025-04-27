using System.ComponentModel.DataAnnotations;

namespace SecureLoginSys.Model.DTOs;

public record SignUpViewModel
{
  public string? Username { get; set; }
  public string? Email { get; set; }
  public string? Password { get; set; }
}
