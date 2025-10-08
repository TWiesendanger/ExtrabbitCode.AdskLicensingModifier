using AdskLicensingModifier.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AdskLicensingModifier.ViewModels;

public partial class MainViewModel(INavigationService navigationService) : ObservableObject
{
    [RelayCommand]
    public async Task OpenDocumentation()
    {
        var uri = new Uri("https://github.com/TWiesendanger/AdskLicensingModifierWinUi3");
        await Launcher.LaunchUriAsync(uri);
    }

    [RelayCommand]
    public void MoveToSettings()
    {
        navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
    }
}