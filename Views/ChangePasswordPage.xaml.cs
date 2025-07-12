using NmapMaui.Services;
using NmapMaui.Models;

namespace NmapMaui.Views;

public partial class ChangePasswordPage : ContentPage
{
    private readonly AuthService _authService;
    public string Username { get; set; }
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmNewPassword { get; set; }

    public ChangePasswordPage()
    {
        InitializeComponent();
        _authService = new AuthService();
        BindingContext = this;
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmNewPassword))
        {
            MessageLabel.Text = "Please fill in all fields.";
            return;
        }

        if (NewPassword.Length < 8)
        {
            MessageLabel.Text = "New password must be at least 8 characters long.";
            return;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            MessageLabel.Text = "New passwords do not match.";
            return;
        }

        var success = await _authService.ChangePassword(Username, CurrentPassword, NewPassword);

        if (success)
        {
            MessageLabel.Text = "Password changed successfully.";
            MessageLabel.TextColor = Colors.Green;

            // Clear input fields after password change
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
            OnPropertyChanged(nameof(CurrentPassword));
            OnPropertyChanged(nameof(NewPassword));
            OnPropertyChanged(nameof(ConfirmNewPassword));

            // Optional: Redirect to login page after successful change
            // await Shell.Current.GoToAsync("//LoginPage");
        }
        else
        {
            MessageLabel.Text = "Password change failed. Username or current password may be incorrect.";
            MessageLabel.TextColor = Colors.Red;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(".."); // Bir önceki sayfaya geri dön
    }
} 