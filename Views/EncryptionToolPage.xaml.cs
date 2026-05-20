using NmapMaui.ViewModels;

namespace NmapMaui.Views;

public partial class EncryptionToolPage : ContentPage
{
    public EncryptionToolPage(EncryptionToolViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
