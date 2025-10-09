using AdskLicensingModifier.ViewModels;

namespace AdskLicensingModifier.Views;

public sealed partial class ProductKeyPage
{
    public ProductKeyViewModel ViewModel { get; }
    public ProductKeyPage()
    {
        ViewModel = App.GetService<ProductKeyViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}