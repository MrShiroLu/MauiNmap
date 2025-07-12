using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using NmapMaui.Models;
using System;

namespace NmapMaui.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private string _currentUser;
        private int _currentUserId;

        public DatabaseService()
        {
            InitializeDatabase();
        }

        public void SetCurrentUser(string username, int userId)
        {
            _currentUser = username;
            _currentUserId = userId;
        }

        private async void InitializeDatabase()
        {
            if (_database is not null)
                return;

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "maui_db.db");
            System.Diagnostics.Debug.WriteLine($"Database path: {dbPath}");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<Base64>();
            await _database.CreateTableAsync<Dns>();
            await _database.CreateTableAsync<Encryption>();
            await _database.CreateTableAsync<Hash>();
            await _database.CreateTableAsync<Nmap>();
            await _database.CreateTableAsync<PassGen>();
            await _database.CreateTableAsync<PassStr>();
            await _database.CreateTableAsync<Ping>();
            await _database.CreateTableAsync<User>();
        }

        // Generic method to get all items of a type
        public async Task<List<T>> GetItemsAsync<T>() where T : new()
        {
            return await _database.Table<T>().ToListAsync();
        }

        // Generic method to add a new item
        public async Task<int> AddItemAsync<T>(T item) where T : BaseModel, new()
        {
            if (string.IsNullOrEmpty(_currentUser))
            {
                throw new InvalidOperationException("Current user must be set before adding items");
            }

            item.CreatedBy = _currentUser;
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            return await _database.InsertAsync(item);
        }

        // Generic method to update an existing item
        public async Task<int> UpdateItemAsync<T>(T item) where T : BaseModel, new()
        {
            if (string.IsNullOrEmpty(_currentUser))
            {
                throw new InvalidOperationException("Current user must be set before updating items");
            }

            item.UpdatedAt = DateTime.UtcNow;
            return await _database.UpdateAsync(item);
        }

        // Generic method to delete an item by ID
        public async Task<int> DeleteItemAsync<T>(int id) where T : new()
        {
            return await _database.DeleteAsync<T>(id);
        }

        // Specific methods to get all items for each table
        public Task<List<Base64>> GetBase64ItemsAsync()
        {
            return _database.Table<Base64>().ToListAsync();
        }

        public Task<List<Dns>> GetDnsItemsAsync()
        {
            return _database.Table<Dns>().ToListAsync();
        }

        public Task<List<Encryption>> GetEncryptionItemsAsync()
        {
            return _database.Table<Encryption>().ToListAsync();
        }

        public Task<List<Hash>> GetHashItemsAsync()
        {
            return _database.Table<Hash>().ToListAsync();
        }

        public Task<List<Nmap>> GetNmapItemsAsync()
        {
            return _database.Table<Nmap>().ToListAsync();
        }

        public Task<List<PassGen>> GetPassGenItemsAsync()
        {
            return _database.Table<PassGen>().ToListAsync();
        }

        public Task<List<PassStr>> GetPassStrItemsAsync()
        {
            return _database.Table<PassStr>().ToListAsync();
        }

        public Task<List<Ping>> GetPingItemsAsync()
        {
            return _database.Table<Ping>().ToListAsync();
        }

    }
} 