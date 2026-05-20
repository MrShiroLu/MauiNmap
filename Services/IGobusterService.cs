using System;
using System.Threading;
using System.Threading.Tasks;
using NmapMaui.Models;

namespace NmapMaui.Services
{
    public interface IGobusterService
    {
        Task<ScanResult> RunDirectoryScanAsync(string url, string wordlist, Action<string>? onOutputLine = null, CancellationToken cancellationToken = default);
    }
}
