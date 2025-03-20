using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Model.Dat;
using WebAPI.Model.DTOs;
using WebAPI.Model.Entities;

namespace WebAPI.Controllers;

[ApiController]
[Route("[Controller]")]
public class ProductsController : ControllerBase
{
    private readonly WebAppDbContext _context;
    private readonly ILogger<ProductsController> _logger;
    public ProductsController(WebAppDbContext context , ILogger<ProductsController> logger)
    {
      _logger = logger;
      _context = context;
    }

    [HttpPost]
    [Route("")]
    [Authorize]
    public async Task<ActionResult<int>> AddProduct(AddOrUpdateProductRequest addProductReq)
    {
      var (isValid, ErrorMessage) = addProductReq.IsValidRequest();
      if (!isValid)
        return BadRequest(ErrorMessage);

      try
      {
        var product = new Product { 
          ProductName = addProductReq.ProductName,
          Description = addProductReq.ProductDescription,
          Price = addProductReq.Price,
          Stock = addProductReq.Stock,
          CreatedAt = DateTime.Now,
        };
        _logger.LogInformation("Creating Product Entity");
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return Ok(product.Id);
      }
      catch (Exception ex)
      {
        return StatusCode(500, ex.Message);
      }
    }

    [HttpGet]
    [Route("")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts(){
      var records = await _context.Products.ToListAsync();

      if (records == null || records.Count == 0)
        return Ok(new List<Product>());

      return Ok(records);
    }

    [HttpGet]
    [Route("{id}")]
    [Authorize]
    public async Task<ActionResult<Product>> GetProductById(int id)
    {
      if (id <= 0)
        return BadRequest("Invalid Product Id");

      try
      {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
          return NotFound($"Product With Id {id} Not Found");

        return Ok(product);
      }
      catch (Exception)
      {
        return StatusCode(500, "Failed To Retrieve Product");
      }
    }

  [HttpPut]
  [Route("{id}")]
  [Authorize]
  public async Task<ActionResult<int>> UpdateProduct(int id, AddOrUpdateProductRequest updateProductReq)
  {
    if (id <= 0)
      return BadRequest("Invalid Id");

    var (isValid, errorMsg) = updateProductReq.IsValidRequest();
    if (!isValid)
      return BadRequest(errorMsg);

    try
    {
      var product = await _context.Products.FindAsync(id);
      if (product == null)
        return NotFound($"Product with Id {id} not found");

      bool dataChanged = false;

      // check updates
      if (!string.IsNullOrWhiteSpace(updateProductReq.ProductName) && product.ProductName != updateProductReq.ProductName)
      {
        product.ProductName = updateProductReq.ProductName;
        dataChanged = true;
      }

      if (!string.IsNullOrWhiteSpace(updateProductReq.ProductDescription) && product.Description != updateProductReq.ProductDescription)
      {
        product.Description = updateProductReq.ProductDescription;
        dataChanged = true;
      }

      if (updateProductReq.Price > 0 && product.Price != updateProductReq.Price)
      {
        product.Price = updateProductReq.Price;
        dataChanged = true;
      }

      if (updateProductReq.Stock >= 0 && product.Stock != updateProductReq.Stock)
      {
        product.Stock = updateProductReq.Stock;
        dataChanged = true;
      }

      if (dataChanged)
      {
        await _context.SaveChangesAsync();
      }

      return Ok(product.Id);
    }
    catch (DbUpdateConcurrencyException)
    {
      if (!await _context.Products.AnyAsync(p => p.Id == id))
        return NotFound("Product no longer exists");
      return Conflict("Product was updated by another process");
    }
    catch (Exception)
    {
      return StatusCode(500, "Failed to update product");
    }
  }

  [HttpDelete]
  [Route("{id}")]
  [Authorize]
  public async Task<ActionResult<int>> DeleteProduct(int id)
  {
    if (id <= 0)
      return BadRequest("Invalid Id");

    var product = await _context.Products.FindAsync(id);
    if (product == null)
      return NotFound($"Product with Id {id} not found");

    try
    {
      _context.Products.Remove(product);
      await _context.SaveChangesAsync();
      return Ok(product.Id);
    }
    catch (Exception)
    {
      return StatusCode(500, "Error deleting product");
    }
  }
}
