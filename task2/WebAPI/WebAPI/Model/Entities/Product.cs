namespace WebAPI.Model.Entities;
public class Product
{
  public int Id { get; set; }
  public string? ProductName { get; set; }
  public string? Description { get; set; }
  public decimal Price { get; set; }
  public int Stock { get; set; }
  public DateTime CreatedAt { get; set; }
}
