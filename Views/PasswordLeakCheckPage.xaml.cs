using NmapMaui.ViewModels;

namespace NmapMaui.Views;

public partial class PasswordLeakCheckPage : ContentPage
{
    public PasswordLeakCheckPage(PasswordLeakCheckViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
