using Microsoft.EntityFrameworkCore;
using WebAPI.Model.Entities;

namespace WebAPI.Model.Dat;
public class WebAppDbContext : DbContext
{
  public DbSet<User> Users { get; set; }
  public DbSet<Product> Products { get; set; }

  public WebAppDbContext(DbContextOptions options) : base(options) { }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(WebAppDbContext).Assembly);
    base.OnModelCreating(modelBuilder);
  }
}
