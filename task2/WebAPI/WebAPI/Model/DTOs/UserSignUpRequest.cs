namespace WebAPI.Model.DTOs;
public class UserSignUpRequest
{
  public string? Name { get; set; }
  public string? Username { get; set; }
  public string? Password { get; set; }

  public bool IsDataCompleted()
  {
    if (string.IsNullOrWhiteSpace(this.Username) ||
        string.IsNullOrWhiteSpace(this.Password) ||
        string.IsNullOrWhiteSpace(this.Name)) return false;
    return true;
  }
}
