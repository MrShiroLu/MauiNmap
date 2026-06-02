using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NmapMaui.Models;
using NmapMaui.Services;

namespace NmapMaui.ViewModels
{
    public partial class PingToolViewModel : ObservableObject
    {
        private readonly INetworkScanner _scanner;
        private readonly ILoggingService _logging;
        private readonly DatabaseService _db;
        private readonly AuthService _auth;

        [ObservableProperty]
        private string host = string.Empty;

        [ObservableProperty]
        private string result = "Ping results will appear here...";

        [ObservableProperty]
        private bool isBusy;

        public PingToolViewModel(INetworkScanner scanner, ILoggingService logging, DatabaseService db, AuthService auth)
        {
            _scanner = scanner;
            _logging = logging;
            _db = db;
            _auth = auth;
        }

        [RelayCommand]
        private async Task PingAsync()
        {
            if (_auth.CurrentUser == null) { Result = "Please log in."; return; }
            if (string.IsNullOrWhiteSpace(Host)) { Result = "Please enter a host."; return; }

            try
            {
                IsBusy = true;
                Result = "Pinging...";
                _db.SetCurrentUser(_auth.CurrentUser.Username, _auth.CurrentUser.Id);
                await _logging.LogAsync("Ping", "Network", Host);
                var r = await _scanner.PingHostAsync(Host);
                Result = r.Result;
                await _db.AddItemAsync(new Ping { Input = Host, Date = DateTime.UtcNow });
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
