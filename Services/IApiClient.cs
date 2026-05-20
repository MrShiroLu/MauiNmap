using System.Threading.Tasks;

namespace NmapMaui.Services
{
    public interface IApiClient
    {
        Task<string> CheckHealthAsync();
        Task<ApiScanLog?> NmapScanAsync(string host, int startPort, int endPort);
        Task<ApiScanLog?> GobusterScanAsync(string url, string wordlist);
    }

    public class ApiScanLog
    {
        public int Id { get; set; }
        public string Tool { get; set; } = "";
        public string Target { get; set; } = "";
        public string Args { get; set; } = "";
        public string Output { get; set; } = "";
        public bool Success { get; set; }
        public System.DateTime Timestamp { get; set; }
    }
}
