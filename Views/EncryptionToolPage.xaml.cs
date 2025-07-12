using NmapMaui.Services;
using NmapMaui.Models;

namespace NmapMaui.Views;

public partial class EncryptionToolPage : ContentPage
{
    private readonly ICryptographyService _cryptoService;
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;

    public EncryptionToolPage(ICryptographyService cryptoService, DatabaseService databaseService, AuthService authService)
    {
        InitializeComponent();
        _cryptoService = cryptoService;
        _databaseService = databaseService;
        _authService = authService;
    }

    private async void OnEncryptClicked(object sender, EventArgs e)
    {
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        if (string.IsNullOrWhiteSpace(InputEditor.Text))
        {
            await DisplayAlert("Error", "Please enter text to encrypt", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(KeyEntry.Text))
        {
            await DisplayAlert("Error", "Please enter an encryption key", "OK");
            return;
        }

        try
        {
            ProcessingIndicator.IsRunning = true;
            ProcessingIndicator.IsVisible = true;

            var result = await _cryptoService.EncryptAsync(InputEditor.Text, KeyEntry.Text);
            ResultEditor.Text = result;

            // Save encryption result to database
            var encryption = new Encryption
            {
                Input = InputEditor.Text,
                Output = result,
                IsEncrypt = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _authService.CurrentUser?.Username ?? "Unknown"
            };
            await _databaseService.AddItemAsync(encryption);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Encryption failed: {ex.Message}", "OK");
        }
        finally
        {
            ProcessingIndicator.IsRunning = false;
            ProcessingIndicator.IsVisible = false;
        }
    }

    private async void OnDecryptClicked(object sender, EventArgs e)
    {
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        if (string.IsNullOrWhiteSpace(InputEditor.Text))
        {
            await DisplayAlert("Error", "Please enter text to decrypt", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(KeyEntry.Text))
        {
            await DisplayAlert("Error", "Please enter an encryption key", "OK");
            return;
        }

        try
        {
            ProcessingIndicator.IsRunning = true;
            ProcessingIndicator.IsVisible = true;

            var result = await _cryptoService.DecryptAsync(InputEditor.Text, KeyEntry.Text);
            ResultEditor.Text = result;

            // Save decryption result to database
            var encryption = new Encryption
            {
                Input = InputEditor.Text,
                Output = result,
                IsEncrypt = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _authService.CurrentUser?.Username ?? "Unknown"
            };
            await _databaseService.AddItemAsync(encryption);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Decryption failed: {ex.Message}", "OK");
        }
        finally
        {
            ProcessingIndicator.IsRunning = false;
            ProcessingIndicator.IsVisible = false;
        }
    }
} 