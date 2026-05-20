using NmapMaui.ViewModels;

namespace NmapMaui.Views;

public partial class HashCalculatorPage : ContentPage
{
    public HashCalculatorPage(HashCalculatorViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
