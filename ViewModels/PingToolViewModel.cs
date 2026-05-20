using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NmapMaui.Services;

namespace NmapMaui.ViewModels
{
    // Example MVVM ViewModel — pattern to follow when converting remaining pages.
    // XAML binds Host, Result, IsBusy, PingCommand. No business logic should live in the
    // .xaml.cs code-behind beyond InitializeComponent + BindingContext assignment.
    public partial class PingToolViewModel : ObservableObject
    {
        private readonly INetworkScanner _scanner;
        private readonly ILoggingService _logging;

        [ObservableProperty]
        private string host = string.Empty;

        [ObservableProperty]
        private string result = "Ping results will appear here...";

        [ObservableProperty]
        private bool isBusy;

        public PingToolViewModel(INetworkScanner scanner, ILoggingService logging)
        {
            _scanner = scanner;
            _logging = logging;
        }

        [RelayCommand]
        private async Task PingAsync()
        {
            if (string.IsNullOrWhiteSpace(Host))
            {
                Result = "Please enter a host.";
                return;
            }

            try
            {
                IsBusy = true;
                Result = "Pinging...";
                await _logging.LogAsync("Ping", "Network", Host);
                var r = await _scanner.PingHostAsync(Host);
                Result = r.Result;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
