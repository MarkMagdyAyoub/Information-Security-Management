namespace secureLogin2.Model.DTOs;

public record TokenResponse
{
  public string? Token { get; set; }
  public string? Message { get; set; }
}
