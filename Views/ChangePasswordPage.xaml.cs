using NmapMaui.Services;

namespace NmapMaui.Views;

public partial class ChangePasswordPage : ContentPage
{
    private readonly AuthService _authService;

    public string Username          { get; set; } = string.Empty;
    public string CurrentPassword   { get; set; } = string.Empty;
    public string NewPassword       { get; set; } = string.Empty;
    public string ConfirmNewPassword{ get; set; } = string.Empty;

    // AuthService artık DI üzerinden inject ediliyor
    public ChangePasswordPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        BindingContext = this;
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Username)          ||
            string.IsNullOrWhiteSpace(CurrentPassword)   ||
            string.IsNullOrWhiteSpace(NewPassword)       ||
            string.IsNullOrWhiteSpace(ConfirmNewPassword))
        {
            MessageLabel.Text = "Please fill in all fields.";
            return;
        }

        if (NewPassword.Length < 8 || !NewPassword.Any(char.IsUpper) || !NewPassword.Any(char.IsDigit))
        {
            MessageLabel.Text = "New password must be at least 8 characters long, contain at least 1 uppercase letter and 1 digit.";
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
            MessageLabel.Text      = "Password changed successfully.";
            MessageLabel.TextColor = Colors.Green;

            CurrentPassword    = string.Empty;
            NewPassword        = string.Empty;
            ConfirmNewPassword = string.Empty;
            OnPropertyChanged(nameof(CurrentPassword));
            OnPropertyChanged(nameof(NewPassword));
            OnPropertyChanged(nameof(ConfirmNewPassword));
        }
        else
        {
            MessageLabel.Text      = "Password change failed. Username or current password may be incorrect.";
            MessageLabel.TextColor = Colors.Red;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}