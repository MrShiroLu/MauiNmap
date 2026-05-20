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

    private void OnSaveClicked(object sender, EventArgs e)
    {
        Preferences.Set("ApiBaseUrl", UrlEntry.Text);
        StatusLabel.Text = "Saved. Restart the app to apply.";
    }

    private async void OnCheckHealthClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "Checking...";
        StatusLabel.Text = "Health: " + await _api.CheckHealthAsync();
    }
}
