using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAPI.Model.Entities;

namespace WebAPI.Model.Data.Config;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("Users");
    
    builder.HasKey(x => x.Id);
    
    builder
      .Property(x => x.Id)
      .ValueGeneratedOnAdd();
    
    builder
      .Property(x => x.Username)
      .HasColumnType("VARCHAR")
      .HasMaxLength(50)
      .IsRequired();

    builder
      .HasIndex(x => x.Username)
      .IsUnique();

    builder
      .Property(x => x.Name)
      .HasColumnType("VARCHAR")
      .HasMaxLength(50)
      .IsRequired();
    
    builder
      .Property(x => x.Password)
      .HasColumnType("VARCHAR")
      .HasMaxLength(64)
      .IsRequired();
  }
}
