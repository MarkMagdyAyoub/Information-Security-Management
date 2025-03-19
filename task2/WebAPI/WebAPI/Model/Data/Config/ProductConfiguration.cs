using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAPI.Model.Entities;

namespace WebAPI.Model.Data.Config;
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
  public void Configure(EntityTypeBuilder<Product> builder)
  {
    builder.ToTable("Products");
    
    builder.HasKey(x => x.Id);
    
    builder
      .Property(x => x.Id)
      .ValueGeneratedOnAdd();
    
    builder
      .Property(x => x.ProductName)
      .HasColumnType("VARCHAR")
      .HasMaxLength(50)
      .IsRequired();
    
    builder
      .Property(x => x.Description)
      .HasColumnType("VARCHAR(MAX)")
      .IsRequired(false);
    
    builder
      .Property(x => x.Price)
      .HasColumnType("DECIMAL(9,2)")
      .IsRequired();
    
    builder
      .Property(x => x.Stock)
      .HasColumnType("INT")
      .IsRequired();
    
    builder
      .Property(x => x.CreatedAt)
      .HasColumnType("DATETIME")
      .IsRequired();
  }
}
