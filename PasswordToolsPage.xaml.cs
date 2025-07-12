using NmapMaui.Views;
using NmapMaui.Services;
using Microsoft.Extensions.DependencyInjection;

namespace NmapMaui;

public partial class PasswordToolsPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public PasswordToolsPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }
    private async void OnPasswordGenPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(_serviceProvider.GetRequiredService<PasswordGenPage>());
    }

    private async void OnPasswordStrengthPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(_serviceProvider.GetRequiredService<PasswordStrengthPage>());
    }

    

    private async void OnPasswordLeakCheckPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(_serviceProvider.GetRequiredService<PasswordLeakCheckPage>());
    }


}