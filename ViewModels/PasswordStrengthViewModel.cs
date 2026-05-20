using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NmapMaui.Models;
using NmapMaui.Services;

namespace NmapMaui.ViewModels
{
    public partial class PasswordStrengthViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly ILoggingService _logging;

        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string strengthLabel = "Password Strength: Not Checked";
        [ObservableProperty] private double progress;
        [ObservableProperty] private string feedback = string.Empty;

        public PasswordStrengthViewModel(DatabaseService db, AuthService auth, ILoggingService logging)
        {
            _db = db;
            _auth = auth;
            _logging = logging;
        }

        [RelayCommand]
        private async Task CheckAsync()
        {
            if (_auth.CurrentUser == null) { Feedback = "Please log in."; return; }
            if (string.IsNullOrWhiteSpace(Password)) { Feedback = "Enter a password."; return; }

            var (strength, score, fb) = Calculate(Password);
            StrengthLabel = $"Password Strength: {strength}";
            Progress = score;
            Feedback = fb;

            _db.SetCurrentUser(_auth.CurrentUser.Username, _auth.CurrentUser.Id);
            await _logging.LogAsync("PasswordStrength", "Password", strength);
            await _db.AddItemAsync(new PassStr { Input = Password, Output = strength });
        }

        private static (string strength, double score, string feedback) Calculate(string password)
        {
            double score = 0;
            var feedback = new List<string>();

            if (password.Length < 8) feedback.Add("Password is too short (minimum 8 characters)");
            else score += Math.Min(password.Length / 20.0, 0.3);

            if (Regex.IsMatch(password, "[A-Z]")) score += 0.2; else feedback.Add("Add uppercase letters");
            if (Regex.IsMatch(password, "[a-z]")) score += 0.2; else feedback.Add("Add lowercase letters");
            if (Regex.IsMatch(password, "[0-9]")) score += 0.2; else feedback.Add("Add numbers");
            if (Regex.IsMatch(password, "[^A-Za-z0-9]")) score += 0.2; else feedback.Add("Add special characters");
            score += Math.Min(password.Distinct().Count() / 20.0, 0.1);

            string strength = score >= 0.9 ? "Very Strong"
                : score >= 0.7 ? "Strong"
                : score >= 0.5 ? "Moderate"
                : score >= 0.3 ? "Weak"
                : "Very Weak";
            return (strength, score, string.Join("\n", feedback));
        }
    }
}
