namespace SecureLoginSys.Model.Entities;
public class LoginLogInfo
{
  public int Id { get; set; }
  public int UserId { get; set; }
  public DateTime Timestamp { get; set; }
  public string? IpAddress { get; set; }
  public User? User { get; set; }
}
