using NmapMaui.Views;
using NmapMaui.Services;
using Microsoft.Extensions.DependencyInjection;

namespace NmapMaui;

public partial class CryptographyToolsPage : ContentPage
{
    private readonly ICryptographyService _cryptoService;   
    private readonly IServiceProvider _serviceProvider;

    public CryptographyToolsPage(ICryptographyService cryptoService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _cryptoService = cryptoService;
        _serviceProvider = serviceProvider;
    }

    private async void OnHashCalculatorClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(_serviceProvider.GetRequiredService<HashCalculatorPage>());
    }

    private async void OnBase64ToolClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(_serviceProvider.GetRequiredService<Base64ToolPage>());
    }

    private async void OnEncryptionToolClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(_serviceProvider.GetRequiredService<EncryptionToolPage>());
    }
}
