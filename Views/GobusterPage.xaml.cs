using NmapMaui.ViewModels;

namespace NmapMaui.Views;

public partial class GobusterPage : ContentPage
{
    public GobusterPage(GobusterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
