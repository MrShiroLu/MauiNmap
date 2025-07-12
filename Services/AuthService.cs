using System;
using System.Threading.Tasks;
using NmapMaui.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using System.IO;
using Microsoft.Maui.Controls;

namespace NmapMaui.Services
{
    public class AuthService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        public User CurrentUser { get; private set; }

        public AuthService()
        {
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "maui_db.db");
            _connectionString = $"Data Source={_dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Users';";
            var tableName = command.ExecuteScalar()?.ToString();

            if (string.IsNullOrEmpty(tableName))
            {
                command.CommandText = @"
                    CREATE TABLE Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL,
                        LastLoginAt TEXT,
                        IsActive INTEGER NOT NULL
                    );";
                command.ExecuteNonQuery();
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public async Task<bool> RegisterUser(string username, string password)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Users (Username, PasswordHash, CreatedAt, IsActive)
                    VALUES (@username, @passwordHash, @createdAt, 1);";
                
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@passwordHash", HashPassword(password));
                command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow.ToString("o"));
                
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User> LoginUser(string username, string password)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Users 
                WHERE Username = @username 
                AND PasswordHash = @passwordHash 
                AND IsActive = 1;";
            
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@passwordHash", HashPassword(password));
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var user = new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    LastLoginAt = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                    IsActive = reader.GetInt32(5) == 1
                };

                // Update last login time
                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = @"
                    UPDATE Users 
                    SET LastLoginAt = @lastLoginAt 
                    WHERE Id = @id;";
                updateCommand.Parameters.AddWithValue("@lastLoginAt", DateTime.UtcNow.ToString("o"));
                updateCommand.Parameters.AddWithValue("@id", user.Id);
                await updateCommand.ExecuteNonQueryAsync();

                CurrentUser = user;
                return user;
            }
            CurrentUser = null;
            return null;
        }

        public async Task<bool> ChangePassword(string username, string currentPassword, string newPassword)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Verify current password
            var loginUser = await LoginUser(username, currentPassword);
            if (loginUser == null)
            {
                return false; // Current password is incorrect
            }

            // Update new password
            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = @"
                UPDATE Users 
                SET PasswordHash = @newPasswordHash 
                WHERE Id = @id;";
            updateCommand.Parameters.AddWithValue("@newPasswordHash", HashPassword(newPassword));
            updateCommand.Parameters.AddWithValue("@id", loginUser.Id);
            
            var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
} 