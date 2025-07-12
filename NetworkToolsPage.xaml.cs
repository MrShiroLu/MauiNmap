using NmapMaui.Views;
using NmapMaui.Services;
using Microsoft.Extensions.DependencyInjection;

namespace NmapMaui;

public partial class NetworkToolsPage : ContentPage
{
    private readonly INetworkScanner _networkScanner;
    private readonly IServiceProvider _serviceProvider;

    public NetworkToolsPage(INetworkScanner networkScanner, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _networkScanner = networkScanner;
        _serviceProvider = serviceProvider;
    }

    private async void OnPortScannerClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(_serviceProvider.GetRequiredService<PortScannerPage>());
    }

    private async void OnPingToolClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(_serviceProvider.GetRequiredService<PingToolPage>());
    }

    private async void OnDnsLookupClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(_serviceProvider.GetRequiredService<DnsLookupPage>());
    }
}