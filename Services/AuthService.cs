using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NmapMaui.Data;
using NmapMaui.Models;

namespace NmapMaui.Services
{
    // Rewritten on top of EF Core via IDbContextFactory<AppDbContext> so the auth
    // store lives in the same database file as everything else (maui_db_ef.db).
    public class AuthService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        public User CurrentUser { get; private set; }

        public AuthService(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            using var ctx = _factory.CreateDbContext();
            ctx.Database.EnsureCreated();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public async Task<bool> RegisterUser(string username, string password)
        {
            try
            {
                await using var ctx = await _factory.CreateDbContextAsync();
                if (await ctx.Users.AnyAsync(u => u.Username == username))
                    return false;

                ctx.Users.Add(new User
                {
                    Username = username,
                    PasswordHash = HashPassword(password),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
                await ctx.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User> LoginUser(string username, string password)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var hash = HashPassword(password);
            var user = await ctx.Users.FirstOrDefaultAsync(u =>
                u.Username == username && u.PasswordHash == hash && u.IsActive);

            if (user == null)
            {
                CurrentUser = null;
                return null;
            }

            user.LastLoginAt = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
            CurrentUser = user;
            return user;
        }

        public async Task<bool> ChangePassword(string username, string currentPassword, string newPassword)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var currentHash = HashPassword(currentPassword);
            var user = await ctx.Users.FirstOrDefaultAsync(u =>
                u.Username == username && u.PasswordHash == currentHash && u.IsActive);
            if (user == null) return false;

            user.PasswordHash = HashPassword(newPassword);
            await ctx.SaveChangesAsync();
            return true;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
