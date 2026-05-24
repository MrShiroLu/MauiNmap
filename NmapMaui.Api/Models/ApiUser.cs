namespace NmapMaui.Api.Models;

/// <summary>
/// MSSQL'deki kullanıcı tablosu.
/// Şifreler BCrypt ile saklanır (work factor 12).
/// </summary>
public class ApiUser
{
    public int      Id           { get; set; }
    public string   Username     { get; set; } = "";
    /// <summary>BCrypt hash — asla plain text değil.</summary>
    public string   PasswordHash { get; set; } = "";
    public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool     IsActive     { get; set; } = true;
}
