using System.Text.RegularExpressions;
using NmapMaui.Services;
using NmapMaui.Models;

namespace NmapMaui.Views;

public partial class PasswordStrengthPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;

    public PasswordStrengthPage(DatabaseService databaseService, AuthService authService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _authService = authService;
    }

    private async void OnCheckStrengthClicked(object sender, EventArgs e)
    {
        if (_authService.CurrentUser == null)
        {
            await DisplayAlert("Error", "You must be logged in to perform this operation.", "OK");
            return;
        }

        _databaseService.SetCurrentUser(_authService.CurrentUser.Username, _authService.CurrentUser.Id);

        string password = PasswordEditor.Text;
        if (string.IsNullOrWhiteSpace(password))
        {
            DisplayAlert("Error", "Please enter a password to check", "OK");
            return;
        }

        var (strength, score, feedback) = CalculatePasswordStrength(password);
        
        StrengthLabel.Text = $"Password Strength: {strength}";
        StrengthProgressBar.Progress = score;
        FeedbackLabel.Text = feedback;

        // Save password strength result to database
        var passStr = new PassStr
        {
            Input = password,
            Output = strength,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = _authService.CurrentUser?.Username ?? "Unknown"
        };
        await _databaseService.AddItemAsync(passStr);
    }

    private (string strength, double score, string feedback) CalculatePasswordStrength(string password)
    {
        double score = 0;
        var feedback = new List<string>();

        if (password.Length < 8)
        {
            feedback.Add("Password is too short (minimum 8 characters)");
        }
        else
        {
            score += Math.Min(password.Length / 20.0, 0.3); 
        }

        
        if (Regex.IsMatch(password, "[A-Z]"))
        {
            score += 0.2;
        }
        else
        {
            feedback.Add("Add uppercase letters");
        }

        if (Regex.IsMatch(password, "[a-z]"))
        {
            score += 0.2;
        }
        else
        {
            feedback.Add("Add lowercase letters");
        }

        if (Regex.IsMatch(password, "[0-9]"))
        {
            score += 0.2;
        }
        else
        {
            feedback.Add("Add numbers");
        }

        if (Regex.IsMatch(password, "[^A-Za-z0-9]"))
        {
            score += 0.2;
        }
        else
        {
            feedback.Add("Add special characters");
        }

        var uniqueChars = password.Distinct().Count();
        score += Math.Min(uniqueChars / 20.0, 0.1);

        string strength;
        if (score >= 0.9)
        {
            strength = "Very Strong";
        }
        else if (score >= 0.7)
        {
            strength = "Strong";
        }
        else if (score >= 0.5)
        {
            strength = "Moderate";
        }
        else if (score >= 0.3)
        {
            strength = "Weak";
        }
        else
        {
            strength = "Very Weak";
        }

        return (strength, score, string.Join("\n", feedback));
    }
} 