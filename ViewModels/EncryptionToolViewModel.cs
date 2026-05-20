using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NmapMaui.Models;
using NmapMaui.Services;

namespace NmapMaui.ViewModels
{
    public partial class EncryptionToolViewModel : ObservableObject
    {
        private readonly ICryptographyService _crypto;
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly ILoggingService _logging;

        [ObservableProperty] private string input = string.Empty;
        [ObservableProperty] private string key = string.Empty;
        [ObservableProperty] private string result = "Result will appear here...";
        [ObservableProperty] private bool isBusy;

        public EncryptionToolViewModel(ICryptographyService crypto, DatabaseService db, AuthService auth, ILoggingService logging)
        {
            _crypto = crypto;
            _db = db;
            _auth = auth;
            _logging = logging;
        }

        [RelayCommand] private Task EncryptAsync() => RunAsync(encrypt: true);
        [RelayCommand] private Task DecryptAsync() => RunAsync(encrypt: false);

        private async Task RunAsync(bool encrypt)
        {
            if (_auth.CurrentUser == null) { Result = "Please log in."; return; }
            if (string.IsNullOrWhiteSpace(Input) || string.IsNullOrWhiteSpace(Key)) { Result = "Input and key required."; return; }
            try
            {
                IsBusy = true;
                _db.SetCurrentUser(_auth.CurrentUser.Username, _auth.CurrentUser.Id);
                var output = encrypt
                    ? await _crypto.EncryptAsync(Input, Key)
                    : await _crypto.DecryptAsync(Input, Key);
                Result = output;
                await _logging.LogAsync(encrypt ? "Encrypt" : "Decrypt", "Crypto");
                await _db.AddItemAsync(new Encryption { Input = Input, Output = output, IsEncrypt = encrypt });
            }
            catch (Exception ex)
            {
                Result = $"Error: {ex.Message}";
            }
            finally { IsBusy = false; }
        }
    }
}
