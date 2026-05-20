using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NmapMaui.Models;
using NmapMaui.Services;

namespace NmapMaui.ViewModels
{
    public partial class HashCalculatorViewModel : ObservableObject
    {
        private readonly ICryptographyService _crypto;
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly ILoggingService _logging;

        [ObservableProperty] private string input = string.Empty;
        [ObservableProperty] private string selectedAlgorithm = "SHA256";
        [ObservableProperty] private string result = "Hash result will appear here...";
        [ObservableProperty] private string hashToVerify = string.Empty;
        [ObservableProperty] private string verificationResult = "Verification result will appear here...";
        [ObservableProperty] private bool isBusy;

        public HashCalculatorViewModel(ICryptographyService crypto, DatabaseService db, AuthService auth, ILoggingService logging)
        {
            _crypto = crypto;
            _db = db;
            _auth = auth;
            _logging = logging;
        }

        [RelayCommand]
        private async Task CalculateAsync()
        {
            if (!Ready()) return;
            try
            {
                IsBusy = true;
                Result = "Calculating...";
                var hash = await _crypto.ComputeHashAsync(Input, SelectedAlgorithm);
                Result = hash;
                await _logging.LogAsync("HashCalc", "Crypto", SelectedAlgorithm);
                await _db.AddItemAsync(new Hash { Input = Input, Algorithm = SelectedAlgorithm, Output = hash });
            }
            catch (Exception ex)
            {
                Result = $"Error: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task VerifyAsync()
        {
            if (!Ready()) return;
            if (string.IsNullOrWhiteSpace(HashToVerify)) { VerificationResult = "Provide a hash to verify."; return; }
            try
            {
                IsBusy = true;
                var computed = await _crypto.ComputeHashAsync(Input, SelectedAlgorithm);
                var match = string.Equals(computed, HashToVerify.Trim(), StringComparison.OrdinalIgnoreCase);
                VerificationResult = match ? $"VERIFIED ({SelectedAlgorithm})\n{computed}" : $"MISMATCH\nComputed: {computed}\nProvided: {HashToVerify}";
                await _logging.LogAsync("HashVerify", "Crypto", $"{SelectedAlgorithm} match={match}", match ? "Info" : "Warning");
            }
            finally { IsBusy = false; }
        }

        private bool Ready()
        {
            if (_auth.CurrentUser == null) { Result = "Please log in."; return false; }
            if (string.IsNullOrWhiteSpace(Input)) { Result = "Enter text first."; return false; }
            _db.SetCurrentUser(_auth.CurrentUser.Username, _auth.CurrentUser.Id);
            return true;
        }
    }
}
