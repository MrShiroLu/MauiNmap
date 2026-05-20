using NmapMaui.ViewModels;

namespace NmapMaui.Views;

public partial class NetcatPage : ContentPage
{
    public NetcatPage(NetcatViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
