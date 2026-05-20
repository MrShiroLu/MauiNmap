using NmapMaui.ViewModels;

namespace NmapMaui.Views;

public partial class PortScannerPage : ContentPage
{
    public PortScannerPage(PortScannerViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
