using Microsoft.EntityFrameworkCore;
using NmapMaui.Api.Models;

namespace NmapMaui.Api.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }
    public DbSet<ScanLog> ScanLogs => Set<ScanLog>();
}
