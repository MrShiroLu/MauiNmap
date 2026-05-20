using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NmapMaui.Models;
using NmapMaui.Services;

namespace NmapMaui.ViewModels
{
    public partial class GobusterViewModel : ObservableObject
    {
        private readonly IGobusterService _gobuster;
        private readonly IApiClient _api;
        private readonly AuthService _auth;
        private readonly DatabaseService _db;
        private readonly IExportService _export;
        private readonly ILoggingService _logging;
        private ScanResult? _lastResult;

        [ObservableProperty] private string url = string.Empty;
        [ObservableProperty] private string wordlist = string.Empty;
        [ObservableProperty] private string output = "Scan results...";
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool useRemoteApi;

        public GobusterViewModel(IGobusterService gobuster, IApiClient api, AuthService auth, DatabaseService db, IExportService export, ILoggingService logging)
        {
            _gobuster = gobuster;
            _api = api;
            _auth = auth;
            _db = db;
            _export = export;
            _logging = logging;
        }

        [RelayCommand]
        private async Task ScanAsync()
        {
            if (_auth.CurrentUser == null) { StatusMessage = "Please log in."; return; }
            if (string.IsNullOrWhiteSpace(Url) || string.IsNullOrWhiteSpace(Wordlist))
            { StatusMessage = "URL and wordlist required."; return; }

            _db.SetCurrentUser(_auth.CurrentUser.Username, _auth.CurrentUser.Id);
            IsBusy = true;
            Output = string.Empty;

            void Append(string line) =>
                MainThread.BeginInvokeOnMainThread(() => Output += line + Environment.NewLine);

            try
            {
                await _logging.LogAsync("Gobuster", "Network", $"{Url} -w {Wordlist} (Remote={UseRemoteApi})");
                
                if (UseRemoteApi)
                {
                    StatusMessage = "Requesting remote scan via API...";
                    var apiLog = await _api.GobusterScanAsync(Url, Wordlist);
                    if (apiLog != null)
                    {
                        var result = new ScanResult
                        {
                            Target = apiLog.Target,
                            ScanTime = apiLog.Timestamp,
                            ScanType = "Gobuster Dir (API)",
                            Result = apiLog.Output,
                            IsSuccess = apiLog.Success
                        };
                        _lastResult = result;
                        Output = apiLog.Output;
                        await _db.AddItemAsync(new Gobuster { Target = Url, Wordlist = Wordlist, Result = apiLog.Output });
                        StatusMessage = apiLog.Success ? "Remote scan finished." : "Remote scan failed.";
                    }
                    else
                    {
                        StatusMessage = "Remote scan failed (no response from API).";
                    }
                }
                else
                {
                    var result = await _gobuster.RunDirectoryScanAsync(Url, Wordlist, Append);
                    _lastResult = result;
                    await _db.AddItemAsync(new Gobuster { Target = Url, Wordlist = Wordlist, Result = result.Result });
                    StatusMessage = result.IsSuccess ? "Scan finished." : "Scan failed.";
                }

                if (_lastResult != null)
                {
                    await _logging.LogAsync("Gobuster.Complete", "Network", $"success={_lastResult.IsSuccess}", _lastResult.IsSuccess ? "Info" : "Warning");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        [RelayCommand] private Task ExportJsonAsync() => ExportAsync(ExportFormat.Json);
        [RelayCommand] private Task ExportCsvAsync() => ExportAsync(ExportFormat.Csv);
        [RelayCommand] private Task ExportPdfAsync() => ExportAsync(ExportFormat.Pdf);

        private async Task ExportAsync(ExportFormat fmt)
        {
            if (_lastResult == null) { StatusMessage = "Run a scan first."; return; }
            try
            {
                var path = await _export.ExportAsync(new[] { _lastResult }, fmt, "gobuster");
                StatusMessage = $"Saved: {path}";
            }
            catch (Exception ex) { StatusMessage = $"Export error: {ex.Message}"; }
        }
    }
}
