using System.ComponentModel.DataAnnotations;

namespace SecureLoginSys.Model.Entities;
public class User 
{
  public int Id { get; set; }
  public string? Username { get; set; }
  public string? Email { get; set; }
  public string? PasswordHash { get; set; } 
  public string? AuthMethod { get; set; } 
  public string? GitHubId { get; set; } 
  public DateTime CreatedAt { get; set; }
  public List<LoginLogInfo> loginLogInfos { get; set; } = new List<LoginLogInfo>();
}
