using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NmapMaui.Data;
using NmapMaui.Services;
using NmapMaui.ViewModels;
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
		builder.Services.AddSingleton<IGobusterService, GobusterService>();
		builder.Services.AddSingleton<INetcatService, NetcatService>();
		builder.Services.AddSingleton<IExportService, ExportService>();
		builder.Services.AddSingleton<ILoggingService, LoggingService>();
		builder.Services.AddSingleton<ISchedulerService, SchedulerService>();
		builder.Services.AddHttpClient<IApiClient, ApiClient>();

		// Have I Been Pwned API için named HttpClient (socket exhaustion önleme)
		builder.Services.AddHttpClient("hibp", client =>
		{
			client.DefaultRequestHeaders.Add("User-Agent", "NmapMaui");
			client.BaseAddress = new Uri("https://api.pwnedpasswords.com/");
		});
		builder.Services.AddDbContextFactory<AppDbContext>(opts =>
		{
			var dbPath = System.IO.Path.Combine(FileSystem.AppDataDirectory, "maui_db_ef.db");
			opts.UseSqlite($"Data Source={dbPath}");
		});

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
		builder.Services.AddTransient<GobusterPage>();
		builder.Services.AddTransient<NetcatPage>();
		builder.Services.AddTransient<LogsPage>();
		builder.Services.AddTransient<SchedulerPage>();
		builder.Services.AddTransient<ApiSettingsPage>();

		// ViewModels (MVVM)
		builder.Services.AddTransient<PingToolViewModel>();
		builder.Services.AddTransient<DnsLookupViewModel>();
		builder.Services.AddTransient<HashCalculatorViewModel>();
		builder.Services.AddTransient<Base64ToolViewModel>();
		builder.Services.AddTransient<EncryptionToolViewModel>();
		builder.Services.AddTransient<PasswordGenViewModel>();
		builder.Services.AddTransient<PasswordStrengthViewModel>();
		builder.Services.AddTransient<PasswordLeakCheckViewModel>();
		builder.Services.AddTransient<PortScannerViewModel>();
		builder.Services.AddTransient<GobusterViewModel>();
		builder.Services.AddTransient<NetcatViewModel>();
		builder.Services.AddTransient<PasswordLeakCheckService>();
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
				fonts.AddFont("IBMPlexMono-Regular.ttf", "IBMPlexMonoRegular");
				fonts.AddFont("IBMPlexMono-SemiBold.ttf", "IBMPlexMonoSemiBold");
				fonts.AddFont("IBMPlexMono-Bold.ttf", "IBMPlexMonoBold");
				fonts.AddFont("IBMPlexMono-Italic.ttf", "IBMPlexMonoItalic");
			})
			.Build(); // Build the MauiApp instance here

		Current = app; // Assign the built app to Current
		return app;
	}
}
