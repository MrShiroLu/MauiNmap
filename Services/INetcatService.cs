using System;
using System.Threading;
using System.Threading.Tasks;
using NmapMaui.Models;

namespace NmapMaui.Services
{
    public interface INetcatService
    {
        Task<ScanResult> BannerGrabAsync(string host, int port, Action<string>? onOutputLine = null, CancellationToken cancellationToken = default);
    }
}
