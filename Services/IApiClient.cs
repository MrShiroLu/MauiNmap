using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NmapMaui.Services
{
    public interface IApiClient
    {
        Task<string>      CheckHealthAsync();

        /// <summary>API'ye login olur, JWT token döner. Token Preferences'a otomatik kaydedilir.</summary>
        Task<string?>     LoginAsync(string username, string password);

        /// <summary>API'ye kullanıcı kaydı yapar.</summary>
        Task<bool>        RegisterAsync(string username, string password);

        Task<ApiScanLog?> NmapScanAsync(string host, int startPort, int endPort);
        Task<ApiScanLog?> GobusterScanAsync(string url, string wordlist);
    }

    public class ApiScanLog
    {
        public int      Id        { get; set; }
        public string   Tool      { get; set; } = "";
        public string   Target    { get; set; } = "";
        public string   Args      { get; set; } = "";
        public string   Output    { get; set; } = "";
        public bool     Success   { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public record ApiTokenResponse(string Token, string Username, DateTime Expiry);
}
