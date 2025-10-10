using AdskLicensingModifier.ViewModels;

namespace AdskLicensingModifier.Views;

public partial class MainPage
{
    public MainViewModel ViewModel{ get; }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}