using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NmapMaui.Models;

namespace NmapMaui.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly DatabaseService _db;
        private readonly AuthService _auth;

        public LoggingService(DatabaseService db, AuthService auth)
        {
            _db = db;
            _auth = auth;
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
    }
}
