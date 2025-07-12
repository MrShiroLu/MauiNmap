using NmapMaui.Views;
using NmapMaui.Services;
using NmapMaui.Models;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection; // Added for reflection
using Microsoft.Maui.Graphics;

namespace NmapMaui;

public partial class DatabaseControlPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthService _authService;
    private Dictionary<string, Type>? _modelTypes;
    private CollectionView? _currentCollectionView; // To hold the dynamically created CollectionView
    private Type? _selectedModelType; // To hold the currently selected model type
    private Dictionary<string, View>? _inputFields;

    public DatabaseControlPage(DatabaseService databaseService, IServiceProvider serviceProvider, AuthService authService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _serviceProvider = serviceProvider;
        _authService = authService;
        
        // Set current user for database operations
        if (_authService.CurrentUser != null)
        {
            _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);
        }
        
        InitializeModelTypes();
        PopulateTablePicker();
    }

    private void InitializeModelTypes()
    {
        _modelTypes = new Dictionary<string, Type>
        {
            { "Base64", typeof(Base64) },
            { "Dns", typeof(Dns) },
            { "Encryption", typeof(Encryption) },
            { "Hash", typeof(Hash) },
            { "Nmap", typeof(Nmap) },
            { "PassGen", typeof(PassGen) },
            { "PassStr", typeof(PassStr) },
            { "Ping", typeof(Ping) }
        };
    }

    private void PopulateTablePicker()
    {
        if (_modelTypes != null)
        {
            foreach (var modelName in _modelTypes.Keys)
            {
                _tablePicker.Items.Add(modelName);
            }
        }
    }

    private async void OnTablePickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        var selectedTable = _tablePicker.SelectedItem as string;
        if (string.IsNullOrEmpty(selectedTable) || _modelTypes == null || !_modelTypes.TryGetValue(selectedTable, out var modelType))
        {
            _tableContentView.Content = null;
            return;
        }

        _selectedModelType = modelType;

        var mainContainer = new VerticalStackLayout();
        
        var crudView = CreateCrudViewForModel(modelType);
        mainContainer.Children.Add(crudView);
        
        _currentCollectionView = new CollectionView
        {
            ItemTemplate = CreateDataTemplateForModel(modelType),
            SelectionMode = SelectionMode.Single,
            BackgroundColor = Color.FromArgb("#1E1E1E"),
            Margin = new Thickness(0, 20, 0, 0)
        };
        _currentCollectionView.SelectionChanged += OnCollectionViewSelectionChanged;
        mainContainer.Children.Add(_currentCollectionView);

        _tableContentView.Content = mainContainer;
        
        await LoadDataForModel(modelType);
    }

    private View CreateCrudViewForModel(Type modelType)
    {
        _inputFields = new Dictionary<string, View>();
        var properties = modelType.GetProperties().Where(p => p.Name != "Id" && p.Name != "CreatedBy" && p.Name != "CreatedAt" && p.Name != "UpdatedAt").ToList();

        var grid = new Grid
        {
            Padding = new Thickness(0, 10),
            BackgroundColor = Color.FromArgb("#2D2D2D"),
            Margin = new Thickness(10)
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        for (int i = 0; i < properties.Count; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        int row = 0;
        foreach (var prop in properties)
        {
            var label = new Label 
            { 
                Text = prop.Name, 
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = Color.FromArgb("#FFFFFF"),
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(10, 5, 10, 5)
            };
            grid.Children.Add(label);
            Grid.SetRow(label, row);
            Grid.SetColumn(label, 0);

            if (prop.PropertyType == typeof(DateTime))
            {
                var datePicker = new DatePicker 
                { 
                    AutomationId = $"{prop.Name}DatePicker",
                    TextColor = Color.FromArgb("#FFFFFF"),
                    BackgroundColor = Color.FromArgb("#3A3A3A")
                };
                var timePicker = new TimePicker 
                { 
                    AutomationId = $"{prop.Name}TimePicker",
                    TextColor = Color.FromArgb("#FFFFFF"),
                    BackgroundColor = Color.FromArgb("#3A3A3A")
                };
                var stackLayout = new HorizontalStackLayout { Spacing = 5 };
                stackLayout.Children.Add(datePicker);
                stackLayout.Children.Add(timePicker);

                _inputFields[prop.Name] = stackLayout;
                grid.Children.Add(stackLayout);
                Grid.SetRow(stackLayout, row);
                Grid.SetColumn(stackLayout, 1);
            }
            else
            {
                var entry = new Entry 
                { 
                    Placeholder = prop.Name, 
                    AutomationId = prop.Name,
                    TextColor = Color.FromArgb("#FFFFFF"),
                    PlaceholderColor = Color.FromArgb("#B0BEC5"),
                    BackgroundColor = Color.FromArgb("#3A3A3A"),
                    Margin = new Thickness(10, 5, 10, 5)
                };
                _inputFields[prop.Name] = entry;
                grid.Children.Add(entry);
                Grid.SetRow(entry, row);
                Grid.SetColumn(entry, 1);
            }
            row++;
        }

        var addButton = new Button 
        { 
            Text = $"Add Selected {modelType.Name}",
            BackgroundColor = Color.FromArgb("#00BFAE"),
            TextColor = Color.FromArgb("#FFFFFF"),
            CornerRadius = 10,
            Margin = new Thickness(5)
        };
        addButton.Clicked += (sender, e) => OnAddItemClicked(sender, e, modelType);

        var updateButton = new Button 
        { 
            Text = $"Update Selected {modelType.Name}",
            BackgroundColor = Color.FromArgb("#5B66F0"),
            TextColor = Color.FromArgb("#FFFFFF"),
            CornerRadius = 10,
            Margin = new Thickness(5)
        };
        updateButton.Clicked += (sender, e) => OnUpdateItemClicked(sender, e, modelType);

        var deleteButton = new Button 
        { 
            Text = $"Delete Selected {modelType.Name}",
            BackgroundColor = Color.FromArgb("#DC3545"),
            TextColor = Color.FromArgb("#FFFFFF"),
            CornerRadius = 10,
            Margin = new Thickness(5)
        };
        deleteButton.Clicked += (sender, e) => OnDeleteItemClicked(sender, e, modelType);

        var buttonLayout = new HorizontalStackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10),
            Children = { addButton, updateButton, deleteButton }
        };

        var mainLayout = new VerticalStackLayout();
        mainLayout.Children.Add(grid);
        mainLayout.Children.Add(buttonLayout);

        return mainLayout;
    }

    private DataTemplate CreateDataTemplateForModel(Type modelType)
    {
        var dataTemplate = new DataTemplate(() =>
        {
            var border = new Border
            {
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                BackgroundColor = Color.FromArgb("#2D2D2D"),
                Stroke = Color.FromArgb("#3A3A3A"),
                StrokeThickness = 1
            };

            var grid = new Grid
            {
                ColumnSpacing = 10
            };

            var properties = modelType.GetProperties().ToList();
            foreach (var prop in properties)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            int col = 0;
            foreach (var prop in properties)
            {
                var label = new Label
                {
                    TextColor = Color.FromArgb("#FFFFFF"),
                    FontSize = 12
                };
                
                if (prop.Name == "CreatedAt" || prop.Name == "UpdatedAt")
                {
                    label.SetBinding(Label.TextProperty, new Binding(prop.Name, stringFormat: "{0:yyyy-MM-dd HH:mm}"));
                    label.TextColor = Color.FromArgb("#AAAAAA");
                }
                else if (prop.Name == "Id")
                {
                    label.SetBinding(Label.TextProperty, new Binding(prop.Name));
                    label.FontAttributes = FontAttributes.Bold;
                    label.TextColor = Color.FromArgb("#00BFAE");
                }
                else
                {
                    label.SetBinding(Label.TextProperty, new Binding(prop.Name));
                }
                
                grid.Children.Add(label);
                Grid.SetColumn(label, col++);
            }

            border.Content = grid;
            return border;
        });
        return dataTemplate;
    }

    private async Task LoadDataForModel(Type modelType)
    {
        var method = typeof(DatabaseService).GetMethod("GetItemsAsync");
        if (method == null) return;

        var genericMethod = method.MakeGenericMethod(modelType);
        var task = (Task?)genericMethod.Invoke(_databaseService, null);
        if (task == null) return;
        await task;

        var resultProperty = task.GetType().GetProperty("Result");
        var items = resultProperty?.GetValue(task);

        if (_currentCollectionView != null)
        {
            _currentCollectionView.ItemsSource = items as System.Collections.IEnumerable;
        }
    }

    private async Task OnAddItemClicked(object? sender, EventArgs e, Type modelType)
    {
        if (_inputFields == null) return;

        // Check if user is logged in
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        // Ensure current user is set in database service
        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        var newItem = Activator.CreateInstance(modelType) as BaseModel;
        if (newItem == null) return;

        foreach (var prop in modelType.GetProperties())
        {
            if (_inputFields.TryGetValue(prop.Name, out var view))
            {
                try
                {
                    if (view is Entry entry)
                    {
                        if (string.IsNullOrEmpty(entry.Text) && prop.PropertyType == typeof(string))
                        {
                            await DisplayAlert("Error", $"{prop.Name} cannot be empty.", "OK");
                            return;
                        }
                        var value = Convert.ChangeType(entry.Text, prop.PropertyType);
                        prop.SetValue(newItem, value);
                    }
                    else if (view is HorizontalStackLayout stackLayout)
                    {
                        var datePicker = stackLayout.Children.OfType<DatePicker>().FirstOrDefault();
                        var timePicker = stackLayout.Children.OfType<TimePicker>().FirstOrDefault();
                        if (datePicker != null && timePicker != null)
                        {
                            DateTime combinedDateTime = datePicker.Date.Add(timePicker.Time);
                            prop.SetValue(newItem, combinedDateTime);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Input Error", $"Invalid value for {prop.Name}: {ex.Message}", "OK");
                    return;
                }
            }
        }

        newItem.CreatedBy = _authService.CurrentUser.Username;
        newItem.CreatedAt = DateTime.UtcNow;
        newItem.UpdatedAt = DateTime.UtcNow;

        try
        {
            var method = typeof(DatabaseService).GetMethod("AddItemAsync");
            if (method == null) return;
            var genericMethod = method.MakeGenericMethod(modelType);
            var invokeTask = (Task?)genericMethod.Invoke(_databaseService, new object[] { newItem });
            if (invokeTask != null)
            {
                await invokeTask;
            }

            await DisplayAlert("Success", $"{modelType.Name} added successfully!", "OK");
            await LoadDataForModel(modelType); // Refresh data
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to add item: {ex.Message}", "OK");
        }
    }

    private async Task OnDeleteItemClicked(object? sender, EventArgs e, Type modelType)
    {
        if (_currentCollectionView?.SelectedItem is not BaseModel selectedItem)
        {
            await DisplayAlert("Error", "No item selected for deletion.", "OK");
            return;
        }

        // Check if user is logged in
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        // Ensure current user is set in database service
        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        try
        {
            var method = typeof(DatabaseService).GetMethod("DeleteItemAsync");
            if (method == null) return;
            var genericMethod = method.MakeGenericMethod(modelType);
            var invokeTask = (Task?)genericMethod.Invoke(_databaseService, new object[] { selectedItem.Id });
            if (invokeTask != null)
            {
                await invokeTask;
            }

            await DisplayAlert("Success", $"{modelType.Name} deleted successfully!", "OK");
            await LoadDataForModel(modelType); // Refresh data
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete item: {ex.Message}", "OK");
        }
    }

    private async Task OnUpdateItemClicked(object? sender, EventArgs e, Type modelType)
    {
        if (_currentCollectionView?.SelectedItem is not BaseModel selectedItem || _inputFields == null)
        {
            await DisplayAlert("Error", "No item selected for update.", "OK");
            return;
        }

        // Check if user is logged in
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        // Ensure current user is set in database service
        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        foreach (var prop in modelType.GetProperties())
        {
            if (_inputFields.TryGetValue(prop.Name, out var view))
            {
                try
                {
                    if (view is Entry entry)
                    {
                        if (string.IsNullOrEmpty(entry.Text) && prop.PropertyType == typeof(string))
                        {
                            await DisplayAlert("Error", $"{prop.Name} cannot be empty.", "OK");
                            return;
                        }
                        var value = Convert.ChangeType(entry.Text, prop.PropertyType);
                        prop.SetValue(selectedItem, value);
                    }
                    else if (view is HorizontalStackLayout stackLayout)
                    {
                        var datePicker = stackLayout.Children.OfType<DatePicker>().FirstOrDefault();
                        var timePicker = stackLayout.Children.OfType<TimePicker>().FirstOrDefault();
                        if (datePicker != null && timePicker != null)
                        {
                            DateTime combinedDateTime = datePicker.Date.Add(timePicker.Time);
                            prop.SetValue(selectedItem, combinedDateTime);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Input Error", $"Invalid value for {prop.Name}: {ex.Message}", "OK");
                    return;
                }
            }
        }

        selectedItem.UpdatedAt = DateTime.UtcNow;

        try
        {
            var method = typeof(DatabaseService).GetMethod("UpdateItemAsync");
            if (method == null) return;
            var genericMethod = method.MakeGenericMethod(modelType);
            var invokeTask = (Task?)genericMethod.Invoke(_databaseService, new object[] { selectedItem });
            if (invokeTask != null)
            {
                await invokeTask;
            }

            await DisplayAlert("Success", $"{modelType.Name} updated successfully!", "OK");
            await LoadDataForModel(modelType); // Refresh data
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to update item: {ex.Message}", "OK");
        }
    }

    private void OnCollectionViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not BaseModel selectedItem || _inputFields == null || _selectedModelType == null)
        {
            return;
        }

        foreach (var prop in _selectedModelType.GetProperties())
        {
            if (_inputFields.TryGetValue(prop.Name, out var view))
            {
                var value = prop.GetValue(selectedItem);
                if (view is Entry entry)
                {
                    entry.Text = value?.ToString();
                }
                else if (view is HorizontalStackLayout stackLayout && value is DateTime dt)
                {
                    var datePicker = stackLayout.Children.OfType<DatePicker>().FirstOrDefault();
                    var timePicker = stackLayout.Children.OfType<TimePicker>().FirstOrDefault();
                    if (datePicker != null) datePicker.Date = dt.Date;
                    if (timePicker != null) timePicker.Time = dt.TimeOfDay;
                }
            }
        }
    }
}