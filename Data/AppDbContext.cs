using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using NmapMaui.Models;

namespace NmapMaui.Data
{
    // EF Core DbContext introduced to satisfy the project proposal's commitment to
    // Code-First EF. It points at a separate database file so it can coexist with
    // the legacy sqlite-net-pcl `DatabaseService` while pages are migrated over.
    public class AppDbContext : DbContext
    {
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
                string dbFolder;
                try
                {
                    dbFolder = FileSystem.AppDataDirectory;
                }
                catch
                {
                    // Fallback for design-time (EF Core Migrations CLI)
                    dbFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
                var dbPath = Path.Combine(dbFolder, "maui_db_ef.db");
                options.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // sqlite-net-pcl attributes are not understood by EF Core, so primary
            // keys are configured explicitly via the BaseModel.Id convention (Id is
            // recognized as PK by EF). Nothing else custom needed for now.
            base.OnModelCreating(modelBuilder);
        }
    }
}
