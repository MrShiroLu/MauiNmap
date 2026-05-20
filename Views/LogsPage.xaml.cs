using NmapMaui.Services;

namespace NmapMaui.Views;

public partial class LogsPage : ContentPage
{
    private readonly ILoggingService _logging;

    public LogsPage(ILoggingService logging)
    {
        InitializeComponent();
        _logging = logging;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async void OnRefreshClicked(object sender, EventArgs e) => await LoadAsync();

    private async Task LoadAsync()
    {
        var items = await _logging.GetAllAsync();
        items.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));
        LogList.ItemsSource = items;
    }
}
