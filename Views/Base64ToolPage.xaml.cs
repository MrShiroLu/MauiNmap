using NmapMaui.Services;
using NmapMaui.Models;

namespace NmapMaui.Views;

public partial class Base64ToolPage : ContentPage
{
    private readonly ICryptographyService _cryptoService;
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;

    public Base64ToolPage(ICryptographyService cryptoService, DatabaseService databaseService, AuthService authService)
    {
        InitializeComponent();
        _cryptoService = cryptoService;
        _databaseService = databaseService;
        _authService = authService;
    }

    private async void OnEncodeClicked(object sender, EventArgs e)
    {
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        if (string.IsNullOrWhiteSpace(InputEditor.Text))
        {
            await DisplayAlert("Error", "Please enter text to encode", "OK");
            return;
        }

        try
        {
            ProcessingIndicator.IsRunning = true;
            ProcessingIndicator.IsVisible = true;
            ResultEditor.Text = "Encoding...";

            var result = await _cryptoService.EncodeBase64Async(InputEditor.Text);
            ResultEditor.Text = result;

            // Save Base64 encode result to database
            var base64 = new Base64
            {
                Input = InputEditor.Text,
                Output = result,
                IsEncode = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _authService.CurrentUser?.Username ?? "Unknown"
            };
            await _databaseService.AddItemAsync(base64);
            await DisplayAlert("Success", "Base64 operation saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            ResultEditor.Text = "Encoding failed.";
        }
        finally
        {
            ProcessingIndicator.IsRunning = false;
            ProcessingIndicator.IsVisible = false;
        }
    }

    private async void OnDecodeClicked(object sender, EventArgs e)
    {
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        if (string.IsNullOrWhiteSpace(InputEditor.Text))
        {
            await DisplayAlert("Error", "Please enter text to decode", "OK");
            return;
        }

        try
        {
            ProcessingIndicator.IsRunning = true;
            ProcessingIndicator.IsVisible = true;
            ResultEditor.Text = "Decoding...";

            var result = await _cryptoService.DecodeBase64Async(InputEditor.Text);
            ResultEditor.Text = result;

            // Save Base64 decode result to database
            var base64 = new Base64
            {
                Input = InputEditor.Text,
                Output = result,
                IsEncode = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _authService.CurrentUser?.Username ?? "Unknown"
            };
            await _databaseService.AddItemAsync(base64);
            await DisplayAlert("Success", "Base64 operation saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            ResultEditor.Text = "Decoding failed.";
        }
        finally
        {
            ProcessingIndicator.IsRunning = false;
            ProcessingIndicator.IsVisible = false;
        }
    }
} 