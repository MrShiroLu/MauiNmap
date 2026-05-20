using NmapMaui.ViewModels;

namespace NmapMaui.Views;

public partial class PingToolPage : ContentPage
{
    public PingToolPage(PingToolViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
