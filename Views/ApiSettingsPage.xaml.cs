using NmapMaui.Services;

namespace NmapMaui.Views;

public partial class ApiSettingsPage : ContentPage
{
    private readonly IApiClient _api;

    public ApiSettingsPage(IApiClient api)
    {
        InitializeComponent();
        _api = api;
        UrlEntry.Text = Preferences.Get("ApiBaseUrl", "http://localhost:5000");
    }

    // ── URL Kayıt & Health ────────────────────────────────────────────────

    private void OnSaveClicked(object sender, EventArgs e)
    {
        Preferences.Set("ApiBaseUrl", UrlEntry.Text);
        StatusLabel.Text      = "Saved. Restart the app to apply.";
        StatusLabel.TextColor = Colors.LightGreen;
    }

    private async void OnCheckHealthClicked(object sender, EventArgs e)
    {
        StatusLabel.Text      = "Checking...";
        StatusLabel.TextColor = Colors.White;
        var result            = await _api.CheckHealthAsync();
        StatusLabel.Text      = "Health: " + result;
        StatusLabel.TextColor = result == "OK" ? Colors.LightGreen : Colors.OrangeRed;
    }

    // ── API Login / Register ──────────────────────────────────────────────

    private async void OnApiLoginClicked(object sender, EventArgs e)
    {
        if (!ValidateApiCredentials()) return;

        StatusLabel.Text      = "Logging in...";
        StatusLabel.TextColor = Colors.White;

        var token = await _api.LoginAsync(ApiUsernameEntry.Text.Trim(), ApiPasswordEntry.Text);
        if (token is not null)
        {
            StatusLabel.Text      = $"✓ Logged in as '{ApiUsernameEntry.Text.Trim()}'. Token saved.";
            StatusLabel.TextColor = Colors.LightGreen;
        }
        else
        {
            StatusLabel.Text      = "✗ Login failed. Check credentials or API URL.";
            StatusLabel.TextColor = Colors.OrangeRed;
        }
    }

    private async void OnApiRegisterClicked(object sender, EventArgs e)
    {
        if (!ValidateApiCredentials()) return;

        StatusLabel.Text      = "Registering...";
        StatusLabel.TextColor = Colors.White;

        var ok = await _api.RegisterAsync(ApiUsernameEntry.Text.Trim(), ApiPasswordEntry.Text);
        if (ok)
        {
            StatusLabel.Text      = "✓ Registration successful. You can now login.";
            StatusLabel.TextColor = Colors.LightGreen;
        }
        else
        {
            StatusLabel.Text      = "✗ Registration failed. Username may already exist.";
            StatusLabel.TextColor = Colors.OrangeRed;
        }
    }

    private bool ValidateApiCredentials()
    {
        if (string.IsNullOrWhiteSpace(ApiUsernameEntry.Text) ||
            string.IsNullOrWhiteSpace(ApiPasswordEntry.Text))
        {
            StatusLabel.Text      = "Enter API username and password.";
            StatusLabel.TextColor = Colors.OrangeRed;
            return false;
        }
        string username = ApiUsernameEntry.Text.Trim();
        string password = ApiPasswordEntry.Text;

        if (username.Length < 3)
        {
            StatusLabel.Text      = "Username must be at least 3 characters.";
            StatusLabel.TextColor = Colors.OrangeRed;
            return false;
        }

        if (password.Length < 8 || !password.Any(char.IsUpper) || !password.Any(char.IsDigit))
        {
            StatusLabel.Text      = "Password must be >= 8 chars with 1 uppercase & 1 digit.";
            StatusLabel.TextColor = Colors.OrangeRed;
            return false;
        }
        return true;
    }
}
