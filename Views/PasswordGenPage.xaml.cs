using NmapMaui.ViewModels;

namespace NmapMaui.Views;

public partial class PasswordGenPage : ContentPage
{
    public PasswordGenPage(PasswordGenViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
