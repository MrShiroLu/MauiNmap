using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NmapMaui.Data;
using NmapMaui.Models;

namespace NmapMaui.Services
{
    // Migrated from sqlite-net-pcl to EF Core. The public surface is preserved so
    // existing pages/view-models keep working; internally everything now goes
    // through AppDbContext via IDbContextFactory.
    public class DatabaseService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private string _currentUser;
        private int _currentUserId;
        private bool _initialized;

        public DatabaseService(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            EnsureCreated();
        }

        public void SetCurrentUser(string username, int userId)
        {
            _currentUser = username;
            _currentUserId = userId;
        }

        private void EnsureCreated()
        {
            if (_initialized) return;
            using var ctx = _factory.CreateDbContext();
            ctx.Database.EnsureCreated();
            _initialized = true;
        }

        public async Task<List<T>> GetItemsAsync<T>() where T : class, new()
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            return await ctx.Set<T>().ToListAsync();
        }

        public async Task<int> AddItemAsync<T>(T item) where T : BaseModel, new()
        {
            if (string.IsNullOrEmpty(_currentUser))
                throw new InvalidOperationException("Current user must be set before adding items");

            item.CreatedBy = _currentUser;
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            await using var ctx = await _factory.CreateDbContextAsync();
            ctx.Set<T>().Add(item);
            await ctx.SaveChangesAsync();
            return item.Id;
        }

        public async Task<int> UpdateItemAsync<T>(T item) where T : BaseModel, new()
        {
            if (string.IsNullOrEmpty(_currentUser))
                throw new InvalidOperationException("Current user must be set before updating items");

            item.UpdatedAt = DateTime.UtcNow;
            await using var ctx = await _factory.CreateDbContextAsync();
            ctx.Set<T>().Update(item);
            return await ctx.SaveChangesAsync();
        }

        public async Task<int> DeleteItemAsync<T>(int id) where T : class, new()
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var entity = await ctx.Set<T>().FindAsync(id);
            if (entity == null) return 0;
            ctx.Set<T>().Remove(entity);
            return await ctx.SaveChangesAsync();
        }

        public Task<List<Base64>> GetBase64ItemsAsync() => GetItemsAsync<Base64>();
        public Task<List<Dns>> GetDnsItemsAsync() => GetItemsAsync<Dns>();
        public Task<List<Encryption>> GetEncryptionItemsAsync() => GetItemsAsync<Encryption>();
        public Task<List<Hash>> GetHashItemsAsync() => GetItemsAsync<Hash>();
        public Task<List<Nmap>> GetNmapItemsAsync() => GetItemsAsync<Nmap>();
        public Task<List<PassGen>> GetPassGenItemsAsync() => GetItemsAsync<PassGen>();
        public Task<List<PassStr>> GetPassStrItemsAsync() => GetItemsAsync<PassStr>();
        public Task<List<Ping>> GetPingItemsAsync() => GetItemsAsync<Ping>();
    }
}
