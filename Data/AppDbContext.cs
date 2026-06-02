using Microsoft.EntityFrameworkCore;
using NmapMaui.Models;

namespace NmapMaui.Data
{
    public class AppDbContext : DbContext
    {
        public const string ConnectionString =
            "Server=(localdb)\\mssqllocaldb;Database=NmapMauiClient;Trusted_Connection=True;MultipleActiveResultSets=true";

        public DbSet<User> Users => Set<User>();
        public DbSet<Hash> Hashes => Set<Hash>();
        public DbSet<Encryption> Encryptions => Set<Encryption>();
        public DbSet<Base64> Base64s => Set<Base64>();
        public DbSet<Nmap> NmapResults => Set<Nmap>();
        public DbSet<Ping> Pings => Set<Ping>();
        public DbSet<Dns> DnsLookups => Set<Dns>();
        public DbSet<PassGen> PassGens => Set<PassGen>();
        public DbSet<PassStr> PassStrs => Set<PassStr>();
        public DbSet<Gobuster> GobusterRuns => Set<Gobuster>();
        public DbSet<Netcat> NetcatRuns => Set<Netcat>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlServer(ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
