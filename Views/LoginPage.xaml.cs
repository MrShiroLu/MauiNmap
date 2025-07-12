using NmapMaui.Services;
using NmapMaui.Models;

namespace NmapMaui.Views;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;
    public string Username { get; set; }
    public string Password { get; set; }

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Shell.Current.CurrentState.Location.OriginalString.Contains("LogoutPage"))
        {
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

        if (Username.Length < 8 || Password.Length < 8)
        {
            MessageLabel.Text = "Username and password must be at least 8 characters long.";
            return;
        }

        var user = await _authService.LoginUser(Username, Password);
        if (user != null)
        {
            
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;

            foreach (var item in Shell.Current.Items.OfType<FlyoutItem>())
            {
                item.IsVisible = true;
            }

            await Shell.Current.GoToAsync("///MainPage");
        }
        else
        {
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

        if (Username.Length < 8 || Password.Length < 8)
        {
            MessageLabel.Text = "Username and password must be at least 8 characters long.";
            return;
        }

        var success = await _authService.RegisterUser(Username, Password);
        if (success)
        {
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