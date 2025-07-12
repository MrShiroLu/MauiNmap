using NmapMaui.Services;
using NmapMaui.Models;

namespace NmapMaui.Views;

public partial class PortScannerPage : ContentPage
{
    private readonly INetworkScanner _networkScanner;
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;

    public PortScannerPage(INetworkScanner networkScanner, DatabaseService databaseService, AuthService authService)
    {
        InitializeComponent();
        _networkScanner = networkScanner;
        _databaseService = databaseService;
        _authService = authService;
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        if (string.IsNullOrWhiteSpace(HostEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a host address", "OK");
            return;
        }

        if (!int.TryParse(StartPortEntry.Text, out int startPort) || !int.TryParse(EndPortEntry.Text, out int endPort))
        {
            await DisplayAlert("Error", "Please enter valid port numbers", "OK");
            return;
        }

        if (startPort < 1 || startPort > 65535 || endPort < 1 || endPort > 65535)
        {
            await DisplayAlert("Error", "Port numbers must be between 1 and 65535", "OK");
            return;
        }

        if (startPort > endPort)
        {
            await DisplayAlert("Error", "Start port must be less than or equal to end port", "OK");
            return;
        }

        try
        {
            ScanningIndicator.IsRunning = true;
            ScanningIndicator.IsVisible = true;
            ResultEditor.Text = "Scanning...";

            var result = await _networkScanner.ScanPortRangeAsync(HostEntry.Text, startPort, endPort);
            ResultEditor.Text = result.Result;

            // The NetworkScanner service will handle saving individual open ports.
            // No need to save a summary here unless specified.

            ResultEditor.Text = "Scan finished.";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            ResultEditor.Text = "Scan failed.";
        }
        finally
        {
            ScanningIndicator.IsRunning = false;
            ScanningIndicator.IsVisible = false;
        }
    }
} 