using NmapMaui.ViewModels;

namespace NmapMaui.Views;

public partial class DnsLookupPage : ContentPage
{
    public DnsLookupPage(DnsLookupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
