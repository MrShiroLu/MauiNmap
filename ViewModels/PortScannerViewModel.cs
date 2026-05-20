using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NmapMaui.Models;
using NmapMaui.Services;

namespace NmapMaui.ViewModels
{
    public partial class PortScannerViewModel : ObservableObject
    {
        private readonly INetworkScanner _scanner;
        private readonly IApiClient _api;
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly IExportService _export;
        private readonly ILoggingService _logging;
        private ScanResult? _lastResult;

        [ObservableProperty] private string host = string.Empty;
        [ObservableProperty] private string startPort = "1";
        [ObservableProperty] private string endPort = "1024";
        [ObservableProperty] private string output = "Scan results will appear here...";
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool useRemoteApi;

        public PortScannerViewModel(INetworkScanner scanner, IApiClient api, DatabaseService db, AuthService auth, IExportService export, ILoggingService logging)
        {
            _scanner = scanner;
            _api = api;
            _db = db;
            _auth = auth;
            _export = export;
            _logging = logging;
        }

        [RelayCommand]
        private async Task ScanAsync()
        {
            if (_auth.CurrentUser == null) { StatusMessage = "Please log in."; return; }
            if (string.IsNullOrWhiteSpace(Host)) { StatusMessage = "Host required."; return; }
            if (!int.TryParse(StartPort, out var sp) || !int.TryParse(EndPort, out var ep))
            { StatusMessage = "Invalid port numbers."; return; }
            if (sp < 1 || ep > 65535 || sp > ep) { StatusMessage = "Port range out of bounds."; return; }

            _db.SetCurrentUser(_auth.CurrentUser.Username, _auth.CurrentUser.Id);
            IsBusy = true;
            Output = string.Empty;
            StatusMessage = "Scanning...";

            void Append(string line) =>
                MainThread.BeginInvokeOnMainThread(() => Output += line + Environment.NewLine);

            try
            {
                await _logging.LogAsync("PortScan", "Network", $"{Host} {sp}-{ep} (Remote={UseRemoteApi})");
                
                if (UseRemoteApi)
                {
                    StatusMessage = "Requesting remote scan via API...";
                    var apiLog = await _api.NmapScanAsync(Host, sp, ep);
                    if (apiLog != null)
                    {
                        var result = new ScanResult
                        {
                            Target = apiLog.Target,
                            ScanTime = apiLog.Timestamp,
                            ScanType = "Port Range Scan (API)",
                            Result = apiLog.Output,
                            IsSuccess = apiLog.Success
                        };
                        _lastResult = result;
                        Output = apiLog.Output;
                        StatusMessage = apiLog.Success ? "Remote scan finished." : "Remote scan failed.";
                    }
                    else
                    {
                        StatusMessage = "Remote scan failed (no response from API).";
                    }
                }
                else
                {
                    var result = await _scanner.ScanPortRangeAsync(Host, sp, ep, Append);
                    _lastResult = result;
                    StatusMessage = result.IsSuccess ? "Scan finished." : "Scan failed.";
                }

                if (_lastResult != null)
                {
                    await _logging.LogAsync("PortScan.Complete", "Network", $"success={_lastResult.IsSuccess}", _lastResult.IsSuccess ? "Info" : "Warning");
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
                var path = await _export.ExportAsync(new[] { _lastResult }, fmt, "portscan");
                StatusMessage = $"Saved: {path}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Export error: {ex.Message}";
            }
        }
    }
}
