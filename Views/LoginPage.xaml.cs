using NmapMaui.Services;
using NmapMaui.Models;

namespace NmapMaui.Views;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly ILoggingService _logging;
    public string Username { get; set; }
    public string Password { get; set; }

    public LoginPage(AuthService authService, ILoggingService logging)
    {
        InitializeComponent();
        _authService = authService;
        _logging = logging;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Shell.Current.CurrentState.Location.OriginalString.Contains("LogoutPage"))
        {
            await _logging.LogAsync("Logout", "Auth", _authService.CurrentUser?.Username ?? "?");
            _authService.Logout();

            
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

            foreach (var item in Shell.Current.Items.OfType<FlyoutItem>())
            {
                item.IsVisible = false;
            }

            
            Username = string.Empty;
            Password = string.Empty;
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));
            MessageLabel.Text = "Successfully logged out.";
            MessageLabel.TextColor = Colors.Green;
            
            await Shell.Current.GoToAsync("///LoginPage", true);
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            MessageLabel.Text = "Please enter username and password.";
            return;
        }

        if (Username.Length < 3 || Password.Length < 8)
        {
            MessageLabel.Text = "Username (min 3 chars) or password (min 8 chars) is invalid.";
            return;
        }

        var user = await _authService.LoginUser(Username, Password);
        if (user != null)
        {
            await _logging.LogAsync("Login", "Auth", Username);
            
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;

            foreach (var item in Shell.Current.Items.OfType<FlyoutItem>())
            {
                item.IsVisible = true;
            }

            await Shell.Current.GoToAsync("///MainPage");
        }
        else
        {
            await _logging.LogAsync("LoginFailed", "Auth", Username, "Warning");
            MessageLabel.Text = "Username or password is incorrect.";
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            MessageLabel.Text = "Please enter username and password.";
            return;
        }

        if (Username.Length < 3)
        {
            MessageLabel.Text = "Username must be at least 3 characters long.";
            return;
        }

        if (Password.Length < 8 || !Password.Any(char.IsUpper) || !Password.Any(char.IsDigit))
        {
            MessageLabel.Text = "Password must be >= 8 characters and contain at least 1 uppercase letter and 1 digit.";
            return;
        }

        var success = await _authService.RegisterUser(Username, Password);
        if (success)
        {
            await _logging.LogAsync("Register", "Auth", Username);
            MessageLabel.Text = "Registration successful! You can now login.";
            MessageLabel.TextColor = Colors.Green;
        }
        else
        {
            MessageLabel.Text = "Registration failed. Username is already in use.";
            MessageLabel.TextColor = Colors.Red;
        }
    }
} 