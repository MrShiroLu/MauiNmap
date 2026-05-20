using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NmapMaui.Services;

namespace NmapMaui.ViewModels
{
    public partial class PasswordLeakCheckViewModel : ObservableObject
    {
        private readonly PasswordLeakCheckService _leak;
        private readonly ILoggingService _logging;

        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string result = string.Empty;
        [ObservableProperty] private bool isBusy;

        public PasswordLeakCheckViewModel(PasswordLeakCheckService leak, ILoggingService logging)
        {
            _leak = leak;
            _logging = logging;
        }

        [RelayCommand]
        private async Task CheckAsync()
        {
            if (string.IsNullOrWhiteSpace(Password)) { Result = "Enter a password."; return; }
            try
            {
                IsBusy = true;
                var count = await _leak.CheckPasswordLeakAsync(Password);
                Result = count > 0
                    ? $"Found in data breaches {count:N0} times!"
                    : "Not found in any data breaches.";
                await _logging.LogAsync("LeakCheck", "Password", $"count={count}", count > 0 ? "Warning" : "Info");
            }
            catch (Exception ex)
            {
                Result = $"Error: {ex.Message}";
            }
            finally { IsBusy = false; }
        }
    }
}
