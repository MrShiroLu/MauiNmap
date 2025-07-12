using Microsoft.Extensions.Logging;
using NmapMaui.Services;
using NmapMaui.Views;

namespace NmapMaui;

public static class MauiProgram
{
	public static MauiApp Current { get; private set; }

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		
		// Register services using the builder
		builder.Services.AddSingleton<INetworkScanner, NetworkScanner>();
		builder.Services.AddSingleton<ICryptographyService, CryptographyService>();
		builder.Services.AddSingleton<DatabaseService>();
		builder.Services.AddSingleton<AuthService>();

		// Register pages for dependency injection using the builder
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<HashCalculatorPage>();
		builder.Services.AddTransient<Base64ToolPage>();
		builder.Services.AddTransient<EncryptionToolPage>();
		builder.Services.AddTransient<PasswordGenPage>();
		builder.Services.AddTransient<PasswordStrengthPage>();
		builder.Services.AddTransient<PasswordLeakCheckPage>();
		builder.Services.AddTransient<PortScannerPage>();
		builder.Services.AddTransient<PingToolPage>();
		builder.Services.AddTransient<DnsLookupPage>();
		builder.Services.AddTransient<CryptographyToolsPage>();
		builder.Services.AddTransient<PasswordToolsPage>();
		builder.Services.AddTransient<DatabaseControlPage>();
		builder.Services.AddTransient<NetworkToolsPage>();
		builder.Services.AddTransient<ChangePasswordPage>();
		builder.Services.AddSingleton<AppShell>();
		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.Build(); // Build the MauiApp instance here

		Current = app; // Assign the built app to Current
		return app;
	}
}
