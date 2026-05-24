using Microsoft.EntityFrameworkCore;
using NmapMaui.Api.Models;

namespace NmapMaui.Api.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

    public DbSet<ScanLog> ScanLogs => Set<ScanLog>();
    public DbSet<ApiUser> Users    => Set<ApiUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Username unique index
        modelBuilder.Entity<ApiUser>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // ScanLog.Output sütunu → büyük Nmap çıktıları için nvarchar(max)
        modelBuilder.Entity<ScanLog>()
            .Property(s => s.Output)
            .HasColumnType("nvarchar(max)");
    }
}
