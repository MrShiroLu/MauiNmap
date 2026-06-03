using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NmapMaui.Data;
using NmapMaui.Models;

namespace NmapMaui.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly IDbContextFactory<AppDbContext> _factory;

        public LoggingService(DatabaseService db, AuthService auth, IDbContextFactory<AppDbContext> factory)
        {
            _db = db;
            _auth = auth;
            _factory = factory;
        }

        public async Task LogAsync(string action, string category, string details = "", string level = "Info")
        {
            try
            {
                var user = _auth.CurrentUser;
                if (user != null)
                    _db.SetCurrentUser(user.Username, user.Id);
                else
                    _db.SetCurrentUser("system", 0);

                var entry = new ActivityLog
                {
                    Action = action,
                    Category = category,
                    Details = details,
                    Level = level,
                    Timestamp = DateTime.UtcNow
                };
                await _db.AddItemAsync(entry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoggingService failed: {ex.Message}");
            }
        }

        public Task<List<ActivityLog>> GetAllAsync() => _db.GetItemsAsync<ActivityLog>();

        public async Task<List<ActivityLog>> GetRecentAsync(int count = 200)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            return await ctx.ActivityLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
