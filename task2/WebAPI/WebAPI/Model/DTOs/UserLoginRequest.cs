namespace WebAPI.Model.DTOs;
public class UserLoginRequest
{
  public string? Username { get; set; }
  public string? Password { get; set; }
  public string? OtpCode { get; set; } 
}
