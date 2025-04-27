using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SecureLoginSys.Model.Entities;

namespace SecureLoginSys.Model.Data.Config;

public class LoginLogInfoConfiguration : IEntityTypeConfiguration<LoginLogInfo>
{
  public void Configure(EntityTypeBuilder<LoginLogInfo> builder)
  {
    builder.ToTable("LoginLogInfos");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id).ValueGeneratedOnAdd();

    builder
      .Property(x => x.IpAddress)
      .HasColumnType("VARCHAR")
      .HasMaxLength(60);
    builder
      .Property(x => x.Timestamp)
      .HasColumnType("DATETIME");

  }
}
