using AdskLicensingModifier.ViewModels;

namespace AdskLicensingModifier.Views;

public sealed partial class ModifyLicensingPage
{
    public ModifyLicensingViewModel ViewModel{ get; }

    public ModifyLicensingPage()
    {
        ViewModel = App.GetService<ModifyLicensingViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}