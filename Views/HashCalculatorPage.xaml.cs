using NmapMaui.Services;
using NmapMaui.Models;

namespace NmapMaui.Views;

public partial class HashCalculatorPage : ContentPage
{
    private readonly ICryptographyService _cryptoService;
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;

    public HashCalculatorPage(ICryptographyService cryptoService, DatabaseService databaseService, AuthService authService)
    {
        InitializeComponent();
        _cryptoService = cryptoService;
        _databaseService = databaseService;
        _authService = authService;
    }

    private async void OnCalculateClicked(object sender, EventArgs e)
    {
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        if (string.IsNullOrWhiteSpace(InputEditor.Text))
        {
            await DisplayAlert("Error", "Please enter text to hash", "OK");
            return;
        }

        if (AlgorithmPicker.SelectedItem == null)
        {
            await DisplayAlert("Error", "Please select a hash algorithm", "OK");
            return;
        }

        try
        {
            CalculatingIndicator.IsRunning = true;
            CalculatingIndicator.IsVisible = true;
            ResultEditor.Text = "Calculating hash...";

            var selectedAlgorithm = AlgorithmPicker.SelectedItem.ToString()!;
            var result = await _cryptoService.ComputeHashAsync(InputEditor.Text, selectedAlgorithm);
            ResultEditor.Text = result;

            // Save hash result to database
            var hash = new Hash
            {
                Input = InputEditor.Text,
                Algorithm = selectedAlgorithm,
                Output = result,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _authService.CurrentUser?.Username ?? "Unknown"
            };

            await _databaseService.AddItemAsync(hash);
            await DisplayAlert("Success", "Hash saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            ResultEditor.Text = "Hash calculation failed.";
        }
        finally
        {
            CalculatingIndicator.IsRunning = false;
            CalculatingIndicator.IsVisible = false;
        }
    }

    private async void OnVerifyClicked(object sender, EventArgs e)
    {
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        if (string.IsNullOrWhiteSpace(InputEditor.Text))
        {
            await DisplayAlert("Error", "Please enter text to verify", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(HashToVerifyEditor.Text))
        {
            await DisplayAlert("Error", "Please enter hash to verify against", "OK");
            return;
        }

        if (AlgorithmPicker.SelectedItem == null)
        {
            await DisplayAlert("Error", "Please select a hash algorithm", "OK");
            return;
        }

        try
        {
            VerifyingIndicator.IsRunning = true;
            VerifyingIndicator.IsVisible = true;
            VerificationResultEditor.Text = "Verifying hash...";

            var selectedAlgorithm = AlgorithmPicker.SelectedItem.ToString()!;
            var computedHash = await _cryptoService.ComputeHashAsync(InputEditor.Text, selectedAlgorithm);
            var providedHash = HashToVerifyEditor.Text.Trim().ToLower();

            var isMatch = string.Equals(computedHash, providedHash, StringComparison.OrdinalIgnoreCase);

            if (isMatch)
            {
                VerificationResultEditor.Text = $"✅ HASH VERIFIED!\n\n" +
                                              $"Text: {InputEditor.Text}\n" +
                                              $"Algorithm: {selectedAlgorithm}\n" +
                                              $"Computed Hash: {computedHash}\n" +
                                              $"Provided Hash: {providedHash}\n\n" +
                                              $"The hashes match! The text is verified.";

                // Save verification result to database
                var hash = new Hash
                {
                    Input = InputEditor.Text,
                    Algorithm = selectedAlgorithm,
                    Output = $"VERIFIED: {computedHash}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = _authService.CurrentUser?.Username ?? "Unknown"
                };

                await _databaseService.AddItemAsync(hash);
                await DisplayAlert("Success", "Hash verification successful and saved!", "OK");
            }
            else
            {
                VerificationResultEditor.Text = $"❌ HASH VERIFICATION FAILED!\n\n" +
                                              $"Text: {InputEditor.Text}\n" +
                                              $"Algorithm: {selectedAlgorithm}\n" +
                                              $"Computed Hash: {computedHash}\n" +
                                              $"Provided Hash: {providedHash}\n\n" +
                                              $"The hashes do not match. The text is not verified.";

                await DisplayAlert("Verification Failed", "The provided hash does not match the computed hash for the given text.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred during verification: {ex.Message}", "OK");
            VerificationResultEditor.Text = "Hash verification failed.";
        }
        finally
        {
            VerifyingIndicator.IsRunning = false;
            VerifyingIndicator.IsVisible = false;
        }
    }
} 