using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NmapMaui.Data;
using NmapMaui.Models;

namespace NmapMaui.Services
{
    public class AuthService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        public User? CurrentUser { get; private set; }

        public AuthService(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            using var ctx = _factory.CreateDbContext();
            ctx.Database.EnsureCreated();
        }

        // --- Hashing ---

        // BCrypt ile güvenli hash (work factor=12 → ~300ms, brute force pratik değil)
        private static string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        // Hem BCrypt ($2a$/b$/y$) hem de eski SHA-256 (base64) desteklenir.
        // Eski hash tespit edilirse, giriş başarılı olunca otomatik BCrypt'e yükseltilir.
        private static bool VerifyPassword(string password, string storedHash)
        {
            if (storedHash.StartsWith("$2a$") ||
                storedHash.StartsWith("$2b$") ||
                storedHash.StartsWith("$2y$"))
            {
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }

            // Eski SHA-256 formatı (geriye dönük uyumluluk)
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes) == storedHash;
        }

        // --- Public API ---

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
                    PasswordHash = HashPassword(password), // BCrypt
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

        public async Task<User?> LoginUser(string username, string password)
        {
            await using var ctx = await _factory.CreateDbContextAsync();

            // BCrypt karşılaştırması WHERE clause'da yapılamaz; önce kullanıcıyı çek
            var user = await ctx.Users.FirstOrDefaultAsync(u =>
                u.Username == username && u.IsActive);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                CurrentUser = null;
                return null;
            }

            // Eski SHA-256 hash varsa → BCrypt'e yükselt (sessiz migration)
            if (!user.PasswordHash.StartsWith("$2"))
                user.PasswordHash = HashPassword(password);

            user.LastLoginAt = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
            CurrentUser = user;
            return user;
        }

        public async Task<bool> ChangePassword(string username, string currentPassword, string newPassword)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var user = await ctx.Users.FirstOrDefaultAsync(u =>
                u.Username == username && u.IsActive);

            if (user == null || !VerifyPassword(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = HashPassword(newPassword);
            await ctx.SaveChangesAsync();
            return true;
        }

        public void Logout() => CurrentUser = null;
    }
}
