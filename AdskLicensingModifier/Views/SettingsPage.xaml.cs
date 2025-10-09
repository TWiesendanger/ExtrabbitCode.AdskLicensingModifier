using AdskLicensingModifier.ViewModels;

namespace AdskLicensingModifier.Views;

public sealed partial class SettingsPage
{
    public SettingsViewModel ViewModel{ get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}