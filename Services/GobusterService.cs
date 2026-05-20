using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NmapMaui.Models;

namespace NmapMaui.Services
{
    public class GobusterService : IGobusterService
    {
        public async Task<ScanResult> RunDirectoryScanAsync(string url, string wordlist, Action<string>? onOutputLine = null, CancellationToken cancellationToken = default)
        {
            var result = new ScanResult
            {
                Target = url,
                ScanTime = DateTime.Now,
                ScanType = "Gobuster Dir",
                IsSuccess = true,
                Result = string.Empty
            };

            var collected = new StringBuilder();
            var errors = new StringBuilder();

            Process? process = null;
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "gobuster",
                    Arguments = $"dir -u {url} -w \"{wordlist}\" -q",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process = new Process { StartInfo = startInfo };
                process.Start();

                var stdoutTask = Task.Run(async () =>
                {
                    string? line;
                    while ((line = await process.StandardOutput.ReadLineAsync()) != null)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        collected.AppendLine(line);
                        onOutputLine?.Invoke(line);
                    }
                }, cancellationToken);

                var stderrTask = Task.Run(async () =>
                {
                    string? line;
                    while ((line = await process.StandardError.ReadLineAsync()) != null)
                    {
                        errors.AppendLine(line);
                        onOutputLine?.Invoke("[stderr] " + line);
                    }
                }, cancellationToken);

                using (cancellationToken.Register(() => { try { process.Kill(entireProcessTree: true); } catch { } }))
                {
                    await Task.WhenAll(stdoutTask, stderrTask);
                    await process.WaitForExitAsync(cancellationToken);
                }

                result.Result = collected.ToString();
                if (process.ExitCode != 0)
                {
                    result.IsSuccess = false;
                    if (errors.Length > 0)
                        result.Result += Environment.NewLine + "[errors] " + errors;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Result = $"Gobuster failed: {ex.Message} (is gobuster installed?)";
            }
            finally
            {
                if (process != null)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(entireProcessTree: true);
                        }
                    }
                    catch { }
                    process.Dispose();
                }
            }

            return result;
        }
    }
}
