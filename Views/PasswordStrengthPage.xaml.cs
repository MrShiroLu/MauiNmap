using NmapMaui.ViewModels;

namespace NmapMaui.Views;

public partial class PasswordStrengthPage : ContentPage
{
    public PasswordStrengthPage(PasswordStrengthViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
