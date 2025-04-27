namespace SecureLoginSys.Model.DTOs;

public record LoginViewModel
{
  public string? Username { get; set; }
  public string? Password { get; set; }
}
