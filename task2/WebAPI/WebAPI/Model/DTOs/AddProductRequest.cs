using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Generic;

namespace WebAPI.Model.DTOs;
public class AddOrUpdateProductRequest
{
  public string? ProductName { get; set; }
  public string? ProductDescription { get; set; }
  public decimal Price { get; set; }
  public int Stock { get; set; }

  public (bool IsValid, string ErrorMessage) IsValidRequest()
  {
    if (string.IsNullOrWhiteSpace(ProductName))
      return (false, "Product name is required.");
    if (ProductName.Length > 50)
      return (false, "Product name cannot exceed 50 characters.");
    if (ProductDescription != null && ProductDescription.Length > 500)
      return (false, "Product description cannot exceed 500 characters.");
    if (Price <= 0)
      return (false, "Price must be greater than 0.");
    if (Stock < 0)
      return (false, "Stock cannot be negative.");
    return (true, string.Empty);
  }
}
