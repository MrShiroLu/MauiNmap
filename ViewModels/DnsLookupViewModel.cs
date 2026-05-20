using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NmapMaui.Models;
using NmapMaui.Services;

namespace NmapMaui.ViewModels
{
    public partial class DnsLookupViewModel : ObservableObject
    {
        private readonly INetworkScanner _scanner;
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly ILoggingService _logging;

        [ObservableProperty] private string hostname = string.Empty;
        [ObservableProperty] private string result = "DNS lookup results will appear here...";
        [ObservableProperty] private bool isBusy;

        public DnsLookupViewModel(INetworkScanner scanner, DatabaseService db, AuthService auth, ILoggingService logging)
        {
            _scanner = scanner;
            _db = db;
            _auth = auth;
            _logging = logging;
        }

        [RelayCommand]
        private async Task LookupAsync()
        {
            if (_auth.CurrentUser == null) { Result = "Please log in."; return; }
            if (string.IsNullOrWhiteSpace(Hostname)) { Result = "Hostname required."; return; }

            try
            {
                IsBusy = true;
                _db.SetCurrentUser(_auth.CurrentUser.Username, _auth.CurrentUser.Id);
                Result = "Looking up...";
                await _logging.LogAsync("DnsLookup", "Network", Hostname);
                var r = await _scanner.ResolveDnsAsync(Hostname);
                Result = r.Result;
                await _db.AddItemAsync(new Dns { Hostname = Hostname, IpAddress = r.Result });
            }
            catch (Exception ex)
            {
                Result = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
