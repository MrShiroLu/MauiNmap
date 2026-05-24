using NmapMaui.Services;
using NmapMaui.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace NmapMaui.Views;

public partial class LogsPage : ContentPage
{
    private readonly ILoggingService _logging;
    private List<ActivityLog> _allLogs = new();

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
        _allLogs = await _logging.GetAllAsync();
        _allLogs.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));

        // Populate Pickers if empty
        if (LevelPicker.Items.Count == 0)
        {
            LevelPicker.Items.Add("All Levels");
            LevelPicker.Items.Add("Info");
            LevelPicker.Items.Add("Warning");
            LevelPicker.Items.Add("Error");
            LevelPicker.SelectedIndex = 0;
        }

        if (CategoryPicker.Items.Count == 0)
        {
            CategoryPicker.Items.Add("All Categories");
            CategoryPicker.Items.Add("Auth");
            CategoryPicker.Items.Add("Network");
            CategoryPicker.Items.Add("Crypto");
            CategoryPicker.SelectedIndex = 0;
        }

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allLogs.AsEnumerable();

        // 1. Text Search Filter (Action, Details, CreatedBy)
        var searchText = LogSearchBar.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filtered = filtered.Where(l => 
                (l.Action != null && l.Action.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (l.Category != null && l.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (l.Details != null && l.Details.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (l.CreatedBy != null && l.CreatedBy.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            );
        }

        // 2. Level Filter
        if (LevelPicker.SelectedIndex > 0)
        {
            var selectedLevel = LevelPicker.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedLevel))
            {
                filtered = filtered.Where(l => l.Level != null && l.Level.Equals(selectedLevel, StringComparison.OrdinalIgnoreCase));
            }
        }

        // 3. Category Filter
        if (CategoryPicker.SelectedIndex > 0)
        {
            var selectedCategory = CategoryPicker.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedCategory))
            {
                filtered = filtered.Where(l => l.Category != null && l.Category.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase));
            }
        }

        LogList.ItemsSource = filtered.ToList();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
    private void OnFilterChanged(object sender, EventArgs e) => ApplyFilters();
}
