using NmapMaui.Services;

namespace NmapMaui.Views;

public partial class PingToolPage : ContentPage
{
    private readonly INetworkScanner _networkScanner;

    public PingToolPage(INetworkScanner networkScanner)
    {
        InitializeComponent();
        _networkScanner = networkScanner;
    }

    private async void OnPingClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(HostEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a host address", "OK");
            return;
        }

        try
        {
            PingingIndicator.IsRunning = true;
            PingingIndicator.IsVisible = true;
            ResultEditor.Text = "Pinging...";

            var result = await _networkScanner.PingHostAsync(HostEntry.Text);
            ResultEditor.Text = result.Result;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            ResultEditor.Text = "Ping failed.";
        }
        finally
        {
            PingingIndicator.IsRunning = false;
            PingingIndicator.IsVisible = false;
        }
    }
} 