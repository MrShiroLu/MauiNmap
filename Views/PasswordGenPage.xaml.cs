using System;
using System.Linq;
using System.Text;
using NmapMaui.Services;
using NmapMaui.Models;

namespace NmapMaui.Views;

public partial class PasswordGenPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;

    public PasswordGenPage(DatabaseService databaseService, AuthService authService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _authService = authService;
    }

    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        if (!int.TryParse(LengthEntry.Text, out int length) || length <= 0)
        {
            DisplayAlert("Error", "Please enter a valid password length", "OK");
            return;
        }

        var uppercase = UppercaseCheckBox.IsChecked ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "";
        var lowercase = LowercaseCheckBox.IsChecked ? "abcdefghijklmnopqrstuvwxyz" : "";
        var numbers = NumbersCheckBox.IsChecked ? "0123456789" : "";
        var specialChars = SpecialCharsCheckBox.IsChecked ? "!@#$%^&*()_+-=[]{}|;:,.<>?" : "";

        var chars = uppercase + lowercase + numbers + specialChars;
        if (string.IsNullOrEmpty(chars))
        {
            DisplayAlert("Error", "Please select at least one character type", "OK");
            return;
        }

        var random = new Random();
        var password = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            password.Append(chars[random.Next(chars.Length)]);
        }

        ResultEditor.Text = password.ToString();

        // Save password generation result to database
        var passGen = new PassGen
        {
            Input = length.ToString(),
            Output = password.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = _authService.CurrentUser?.Username ?? "Unknown"
        };
        await _databaseService.AddItemAsync(passGen);
    }
} 