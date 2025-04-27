using Microsoft.EntityFrameworkCore;
using SecureLoginSys.Model.Entities;

namespace SecureLoginSys.Model.Data;
public class WebAppDbContext : DbContext
{
  public DbSet<User> Users { get; set; }
  public DbSet<LoginLogInfo> LoginLogInfos { get; set; }
  public WebAppDbContext(DbContextOptions options) : base(options) { }
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    var constr = config.GetSection("constr").Value;
    optionsBuilder.UseSqlServer(constr);
    base.OnConfiguring(optionsBuilder);
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(WebAppDbContext).Assembly);
    base.OnModelCreating(modelBuilder);
  }
}
