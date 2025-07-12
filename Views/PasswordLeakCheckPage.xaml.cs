using NmapMaui.Services;

namespace NmapMaui.Views;

public partial class PasswordLeakCheckPage : ContentPage
{
    private readonly PasswordLeakCheckService _passwordLeakCheckService;

    public PasswordLeakCheckPage()
    {
        InitializeComponent();
        _passwordLeakCheckService = new PasswordLeakCheckService();
    }

    private async void OnCheckClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Warning", "Please enter a password.", "OK");
            return;
        }

        try
        {
            LoadingIndicator.IsVisible = LoadingIndicator.IsRunning = true;

            var leakCount = await _passwordLeakCheckService.CheckPasswordLeakAsync(PasswordEntry.Text);

            if (leakCount > 0)
            {
                ResultLabel.Text = $"This password has been found in data breaches {leakCount:N0} times!";
                ResultLabel.TextColor = Colors.Red;
            }
            else
            {
                ResultLabel.Text = "This password has not been found in any data breaches.";
                ResultLabel.TextColor = Colors.Green;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "An error occurred during password check: " + ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = LoadingIndicator.IsRunning = false;
        }
    }
} 