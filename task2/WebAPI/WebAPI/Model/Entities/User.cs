namespace WebAPI.Model.Entities;
public class User
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public string? Username { get; set; }
  public string? Password { get; set; }

  public bool IsDataCompleted() {
    if (string.IsNullOrWhiteSpace(this.Username) ||
        string.IsNullOrWhiteSpace(this.Password) ||
        string.IsNullOrWhiteSpace(this.Name)) return false;
    return true;
  }
}
