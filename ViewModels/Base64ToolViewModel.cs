using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NmapMaui.Models;
using NmapMaui.Services;

namespace NmapMaui.ViewModels
{
    public partial class Base64ToolViewModel : ObservableObject
    {
        private readonly ICryptographyService _crypto;
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly ILoggingService _logging;

        [ObservableProperty] private string input = string.Empty;
        [ObservableProperty] private string result = "Result will appear here...";
        [ObservableProperty] private bool isBusy;

        public Base64ToolViewModel(ICryptographyService crypto, DatabaseService db, AuthService auth, ILoggingService logging)
        {
            _crypto = crypto;
            _db = db;
            _auth = auth;
            _logging = logging;
        }

        [RelayCommand] private Task EncodeAsync() => RunAsync(encode: true);
        [RelayCommand] private Task DecodeAsync() => RunAsync(encode: false);

        private async Task RunAsync(bool encode)
        {
            if (_auth.CurrentUser == null) { Result = "Please log in."; return; }
            if (string.IsNullOrWhiteSpace(Input)) { Result = "Enter text first."; return; }
            try
            {
                IsBusy = true;
                _db.SetCurrentUser(_auth.CurrentUser.Username, _auth.CurrentUser.Id);
                Result = encode ? "Encoding..." : "Decoding...";
                var output = encode
                    ? await _crypto.EncodeBase64Async(Input)
                    : await _crypto.DecodeBase64Async(Input);
                Result = output;
                await _logging.LogAsync(encode ? "Base64Encode" : "Base64Decode", "Crypto");
                await _db.AddItemAsync(new Base64 { Input = Input, Output = output, IsEncode = encode });
            }
            catch (Exception ex)
            {
                Result = $"Error: {ex.Message}";
            }
            finally { IsBusy = false; }
        }
    }
}
