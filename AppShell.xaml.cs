using NmapMaui.Views;

namespace NmapMaui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(CryptographyToolsPage), typeof(CryptographyToolsPage));
		Routing.RegisterRoute(nameof(NetworkToolsPage), typeof(NetworkToolsPage));
		Routing.RegisterRoute(nameof(PasswordToolsPage), typeof(PasswordToolsPage));
		Routing.RegisterRoute(nameof(DatabaseControlPage), typeof(DatabaseControlPage));
		Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
		Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
		Routing.RegisterRoute("LogoutPage", typeof(LoginPage));
		Routing.RegisterRoute(nameof(ChangePasswordPage), typeof(ChangePasswordPage));
		Routing.RegisterRoute(nameof(EncryptionToolPage), typeof(EncryptionToolPage));
		Routing.RegisterRoute(nameof(GobusterPage), typeof(GobusterPage));
		Routing.RegisterRoute(nameof(NetcatPage), typeof(NetcatPage));
		Routing.RegisterRoute(nameof(LogsPage), typeof(LogsPage));
		Routing.RegisterRoute(nameof(SchedulerPage), typeof(SchedulerPage));
		Routing.RegisterRoute(nameof(ApiSettingsPage), typeof(ApiSettingsPage));
	}

	protected override void OnNavigating(ShellNavigatingEventArgs args)
	{
		base.OnNavigating(args);

		// if the user navigates via a flyout item, close the flyout
		if (args.Source == ShellNavigationSource.ShellItemChanged)
		{
			FlyoutIsPresented = false;
		}
	}
}
