using System;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NmapMaui.Models;
using NmapMaui.Services;

namespace NmapMaui.ViewModels
{
    public partial class PasswordGenViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly ILoggingService _logging;

        [ObservableProperty] private int length = 16;
        [ObservableProperty] private bool useUppercase = true;
        [ObservableProperty] private bool useLowercase = true;
        [ObservableProperty] private bool useNumbers = true;
        [ObservableProperty] private bool useSpecial = true;
        [ObservableProperty] private string result = string.Empty;

        public PasswordGenViewModel(DatabaseService db, AuthService auth, ILoggingService logging)
        {
            _db = db;
            _auth = auth;
            _logging = logging;
        }

        [RelayCommand]
        private async Task GenerateAsync()
        {
            if (_auth.CurrentUser == null) { Result = "Please log in."; return; }
            if (Length <= 0) { Result = "Length must be positive."; return; }

            var chars = (UseUppercase ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "") +
                        (UseLowercase ? "abcdefghijklmnopqrstuvwxyz" : "") +
                        (UseNumbers ? "0123456789" : "") +
                        (UseSpecial ? "!@#$%^&*()_+-=[]{}|;:,.<>?" : "");
            if (string.IsNullOrEmpty(chars)) { Result = "Select at least one character type."; return; }

            var rnd = new Random();
            var sb = new StringBuilder(Length);
            for (int i = 0; i < Length; i++) sb.Append(chars[rnd.Next(chars.Length)]);
            Result = sb.ToString();

            _db.SetCurrentUser(_auth.CurrentUser.Username, _auth.CurrentUser.Id);
            await _logging.LogAsync("PasswordGen", "Password", $"len={Length}");
            await _db.AddItemAsync(new PassGen { Input = Length.ToString(), Output = Result });
        }
    }
}
