namespace NmapMaui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(MauiProgram.Current.Services.GetRequiredService<AppShell>());
	}
}