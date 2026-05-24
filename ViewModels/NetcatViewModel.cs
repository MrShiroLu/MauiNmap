using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NmapMaui.Models;
using NmapMaui.Services;

namespace NmapMaui.ViewModels
{
    public partial class NetcatViewModel : ObservableObject
    {
        private readonly INetcatService _netcat;
        private readonly AuthService _auth;
        private readonly DatabaseService _db;
        private readonly IExportService _export;
        private readonly ILoggingService _logging;
        private ScanResult? _lastResult;

        [ObservableProperty] private string host = string.Empty;
        [ObservableProperty] private string port = string.Empty;
        [ObservableProperty] private string output = "Output...";
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string statusMessage = string.Empty;

        public NetcatViewModel(INetcatService netcat, AuthService auth, DatabaseService db, IExportService export, ILoggingService logging)
        {
            _netcat = netcat;
            _auth = auth;
            _db = db;
            _export = export;
            _logging = logging;
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            if (_auth.CurrentUser == null) { StatusMessage = "Please log in."; return; }
            if (string.IsNullOrWhiteSpace(Host) || !int.TryParse(Port, out var p))
            { StatusMessage = "Valid host and port required."; return; }

            _db.SetCurrentUser(_auth.CurrentUser.Username, _auth.CurrentUser.Id);
            IsBusy = true;
            Output = string.Empty;

            void Append(string line) =>
                MainThread.BeginInvokeOnMainThread(() => Output += line + Environment.NewLine);

            try
            {
                await _logging.LogAsync("Netcat", "Network", $"{Host}:{p}");
                var result = await _netcat.BannerGrabAsync(Host, p, Append);
                _lastResult = result;
                await _db.AddItemAsync(new Netcat { Host = Host, Port = p, Result = result.Result });
                await _logging.LogAsync("Netcat.Complete", "Network", $"success={result.IsSuccess}", result.IsSuccess ? "Info" : "Warning");
                StatusMessage = result.IsSuccess ? "Done." : "Failed.";
            }
            catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
            finally { IsBusy = false; }
        }

        [RelayCommand] private Task ExportJsonAsync() => ExportAsync(ExportFormat.Json);
        [RelayCommand] private Task ExportCsvAsync() => ExportAsync(ExportFormat.Csv);
        [RelayCommand] private Task ExportPdfAsync() => ExportAsync(ExportFormat.Pdf);

        private async Task ExportAsync(ExportFormat fmt)
        {
            if (_lastResult == null) { StatusMessage = "Run connect first."; return; }
            try
            {
                var path = await _export.ExportAsync(new[] { _lastResult }, fmt, "netcat");
                if (string.IsNullOrEmpty(path))
                {
                    StatusMessage = "Export cancelled.";
                    return;
                }
                StatusMessage = $"Saved: {path}";
            }
            catch (Exception ex) { StatusMessage = $"Export error: {ex.Message}"; }
        }
    }
}
