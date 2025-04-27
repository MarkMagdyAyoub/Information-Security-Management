using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureLoginSys.Model.Entities;

namespace SecureLoginSys.Model.Data.Config;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("Users");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id).ValueGeneratedOnAdd();

    builder
      .Property(x => x.Username)
      .HasColumnType("VARCHAR")
      .HasMaxLength(60)
      .IsRequired();

    builder.HasIndex(x => x.Username).IsUnique();

    builder
      .Property(x => x.AuthMethod)
      .HasColumnType("VARCHAR")
      .HasMaxLength(30)
      .IsRequired();

    builder
      .Property(x => x.CreatedAt)
      .HasColumnType("DATETIME")
      .IsRequired();

    builder
      .Property(x => x.PasswordHash)
      .HasColumnType("VARCHAR")
      .HasMaxLength(90); // bcrypt size hash = 72 character

    builder
      .Property(x => x.GitHubId)
      .HasColumnType("VARCHAR")
      .HasMaxLength(255)
      .IsRequired(false);

    builder
      .HasMany(x => x.loginLogInfos)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId);
  }
}
