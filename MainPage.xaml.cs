using NmapMaui.Views;
using NmapMaui.Services;
using Microsoft.Extensions.DependencyInjection;

namespace NmapMaui;

public partial class MainPage : ContentPage
{
	private readonly INetworkScanner _networkScanner;
	private readonly ICryptographyService _cryptoService;
	private readonly IServiceProvider _serviceProvider;

	public MainPage(INetworkScanner networkScanner, ICryptographyService cryptoService, IServiceProvider serviceProvider)
	{
		InitializeComponent();
		_networkScanner = networkScanner;
		_cryptoService = cryptoService;
		_serviceProvider = serviceProvider;
	}

	private async void OnNetworkToolsClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(NetworkToolsPage));
	}

	private async void OnCryptotoolsClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(CryptographyToolsPage));
	}

	private async void OnPasswordToolsClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(PasswordToolsPage));
	}

	private async void OnDatabaseControlClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(DatabaseControlPage));
	}

	private async void OnChangePasswordPageClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(ChangePasswordPage));
	}

	private async void OnLoginPageClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(LoginPage));
	}
}
