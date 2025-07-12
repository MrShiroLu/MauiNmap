using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using NmapMaui.Models;
using System.Collections.Generic;
using System.Linq;

namespace NmapMaui.Services
{
    public class NetworkScanner : INetworkScanner
    {
        private readonly DatabaseService _databaseService;
        private readonly AuthService _authService;

        public NetworkScanner(DatabaseService databaseService, AuthService authService)
        {
            _databaseService = databaseService;
            _authService = authService;
        }

        public async Task<ScanResult> ScanPortAsync(string host, int port)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port);
                var timeoutTask = Task.Delay(2000); // 2 second timeout

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    System.Diagnostics.Debug.WriteLine($"Port {port} on {host} timed out.");
                    return new ScanResult
                    {
                        Target = $"{host}:{port}",
                        Result = "Port scan timed out",
                        ScanTime = DateTime.Now,
                        ScanType = "Port Scan",
                        IsSuccess = false
                    };
                }

                if (connectTask.IsFaulted)
                {
                    throw connectTask.Exception.InnerException;
                }

                return new ScanResult
                {
                    Target = $"{host}:{port}",
                    Result = "Port is open",
                    ScanTime = DateTime.Now,
                    ScanType = "Port Scan",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new ScanResult
                {
                    Target = $"{host}:{port}",
                    Result = $"Port is closed or unreachable: {ex.Message}",
                    ScanTime = DateTime.Now,
                    ScanType = "Port Scan",
                    IsSuccess = false
                };
            }
        }

        public async Task<ScanResult> ScanPortRangeAsync(string host, int startPort, int endPort)
        {
            var result = new ScanResult
            {
                Target = host,
                ScanTime = DateTime.Now,
                ScanType = "Port Range Scan",
                Result = "Scanning ports...",
                IsSuccess = true
            };

            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "nmap",
                    Arguments = $"-sC -T4 -p{startPort}-{endPort} {host}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new System.Diagnostics.Process { StartInfo = startInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    result.Result = output;
                }
                else
                {
                    result.Result = $"Nmap scan failed: {error}";
                    result.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                result.Result = $"Error during port range scan: {ex.Message}";
                result.IsSuccess = false;
            }

            return result;
        }

        public async Task<ScanResult> PingHostAsync(string host)
        {
            try
            {
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = await ping.SendPingAsync(host);
                
                return new ScanResult
                {
                    Target = host,
                    Result = $"Reply from {host}: time={reply.RoundtripTime}ms",
                    ScanTime = DateTime.Now,
                    ScanType = "Ping",
                    IsSuccess = reply.Status == IPStatus.Success
                };
            }
            catch (Exception ex)
            {
                return new ScanResult
                {
                    Target = host,
                    Result = $"Ping failed: {ex.Message}",
                    ScanTime = DateTime.Now,
                    ScanType = "Ping",
                    IsSuccess = false
                };
            }
        }

        public async Task<ScanResult> ResolveDnsAsync(string hostname)
        {
            try
            {
                var addresses = await System.Net.Dns.GetHostAddressesAsync(hostname);
                var result = new ScanResult
                {
                    Target = hostname,
                    ScanTime = DateTime.Now,
                    ScanType = "DNS Resolution",
                    Result = "Resolving...",
                    IsSuccess = true
                };

                var addressList = new System.Text.StringBuilder();
                foreach (var address in addresses)
                {
                    addressList.AppendLine(address.ToString());
                }
                result.Result = addressList.ToString();

                return result;
            }
            catch (Exception ex)
            {
                return new ScanResult
                {
                    Target = hostname,
                    Result = $"DNS resolution failed: {ex.Message}",
                    ScanTime = DateTime.Now,
                    ScanType = "DNS Resolution",
                    IsSuccess = false
                };
            }
        }

        private async void SaveNmapScanResult(string result)
        {
            if (_authService.CurrentUser == null) return;

            var nmapResult = new Nmap
            {
                NmapPort = result,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _authService.CurrentUser.Username
            };
            await _databaseService.AddItemAsync(nmapResult);
        }
    }
} 