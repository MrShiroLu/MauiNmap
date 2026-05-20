using NmapMaui.Services;

namespace NmapMaui.Views;

public partial class SchedulerPage : ContentPage
{
    private readonly ISchedulerService _scheduler;

    public SchedulerPage(ISchedulerService scheduler)
    {
        InitializeComponent();
        _scheduler = scheduler;
    }

    private async void OnScheduleClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(HostEntry.Text) ||
            !int.TryParse(StartPortEntry.Text, out var sp) ||
            !int.TryParse(EndPortEntry.Text, out var ep) ||
            string.IsNullOrWhiteSpace(CronEntry.Text))
        {
            await DisplayAlert("Error", "Fill all fields.", "OK");
            return;
        }

        try
        {
            await _scheduler.ScheduleNmapScanAsync(HostEntry.Text, sp, ep, CronEntry.Text);
            var count = await _scheduler.GetActiveJobCountAsync();
            StatusLabel.Text = $"Job scheduled. Active jobs: {count}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Scheduler error", ex.Message, "OK");
        }
    }
}
