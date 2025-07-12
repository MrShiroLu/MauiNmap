using NmapMaui.Services;
using NmapMaui.Models;

namespace NmapMaui.Views;

public partial class DnsLookupPage : ContentPage
{
    private readonly INetworkScanner _networkScanner;
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;

    public DnsLookupPage(INetworkScanner networkScanner, DatabaseService databaseService, AuthService authService)
    {
        InitializeComponent();
        _networkScanner = networkScanner;
        _databaseService = databaseService;
        _authService = authService;
    }

    private async void OnLookupClicked(object sender, EventArgs e)
    {
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        var hostname = HostnameEntry.Text;
        if (string.IsNullOrWhiteSpace(hostname))
        {
            await DisplayAlert("Error", "Please enter a hostname.", "OK");
            return;
        }

        try
        {
            ResultEditor.Text = "Looking up DNS...";
            var result = await _networkScanner.ResolveDnsAsync(hostname);
            ResultEditor.Text = result.Result;

            // Save DNS lookup result to database
            var dns = new Dns
            {
                Hostname = hostname,
                IpAddress = result.Result,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _authService.CurrentUser?.Username ?? "Unknown"
            };
            await _databaseService.AddItemAsync(dns);
            await DisplayAlert("Success", "DNS lookup saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            ResultEditor.Text = $"Error: {ex.Message}";
            await DisplayAlert("Error", $"Failed to lookup DNS: {ex.Message}", "OK");
        }
    }

    public class ScanResult
    {
        public string IpAddress { get; set; }
        public bool IsOpen { get; set; }
    }
} 