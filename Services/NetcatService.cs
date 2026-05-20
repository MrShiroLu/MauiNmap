using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NmapMaui.Models;

namespace NmapMaui.Services
{
    // Cross-platform "netcat-like" banner-grab implementation. Avoids depending on the
    // `nc` binary so this works on Windows/Android targets too. Opens a TCP socket,
    // optionally sends a probe payload, and streams whatever the server emits.
    public class NetcatService : INetcatService
    {
        public async Task<ScanResult> BannerGrabAsync(string host, int port, Action<string>? onOutputLine = null, CancellationToken cancellationToken = default)
        {
            var result = new ScanResult
            {
                Target = $"{host}:{port}",
                ScanTime = DateTime.Now,
                ScanType = "Netcat",
                IsSuccess = true,
                Result = string.Empty
            };

            var collected = new StringBuilder();

            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port);
                var timeoutTask = Task.Delay(5000, cancellationToken);
                if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
                {
                    result.IsSuccess = false;
                    result.Result = "Connection timed out.";
                    return result;
                }

                onOutputLine?.Invoke($"[+] Connected to {host}:{port}");

                using var stream = client.GetStream();
                stream.ReadTimeout = 3000;

                // Probe to elicit a banner from HTTP-ish services. Harmless to most TCP daemons.
                var probe = Encoding.ASCII.GetBytes($"HEAD / HTTP/1.0\r\nHost: {host}\r\n\r\n");
                await stream.WriteAsync(probe, 0, probe.Length, cancellationToken);

                using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
                var deadline = DateTime.UtcNow.AddSeconds(5);
                Task<string?>? readTask = null;
                while (DateTime.UtcNow < deadline)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    
                    if (readTask == null)
                    {
                        readTask = reader.ReadLineAsync();
                    }

                    var completed = await Task.WhenAny(readTask, Task.Delay(1000, cancellationToken));
                    if (completed == readTask)
                    {
                        var line = await readTask;
                        readTask = null; // Reset for the next line
                        if (line == null) break;
                        collected.AppendLine(line);
                        onOutputLine?.Invoke(line);
                    }
                }

                result.Result = collected.ToString();
                if (string.IsNullOrWhiteSpace(result.Result))
                    result.Result = "(no banner received)";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Result = $"Netcat error: {ex.Message}";
            }

            return result;
        }
    }
}
