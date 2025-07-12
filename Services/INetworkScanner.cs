using System.Threading.Tasks;
using NmapMaui.Models;

namespace NmapMaui.Services
{
    public interface INetworkScanner
    {
        Task<ScanResult> ScanPortAsync(string host, int port);
        Task<ScanResult> ScanPortRangeAsync(string host, int startPort, int endPort);
        Task<ScanResult> PingHostAsync(string host);
        Task<ScanResult> ResolveDnsAsync(string hostname);
    }
} 