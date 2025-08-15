using Microsoft.EntityFrameworkCore;

namespace UptimeMonitor.Api.Models;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }
    public DbSet<MonitorTarget> Targets => Set<MonitorTarget>();
    public DbSet<CheckLog> Logs => Set<CheckLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<MonitorTarget>().HasIndex(x => x.Url);
        b.Entity<CheckLog>().HasIndex(x => new { x.MonitorTargetId, x.CheckedAt });
    }
}
