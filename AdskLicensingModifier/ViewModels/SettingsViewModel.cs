using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Input;
using AdskLicensingModifier.Contracts.Services;
using AdskLicensingModifier.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI;

namespace AdskLicensingModifier.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IGenericMessageDialogService _messageDialogService;
    private ElementTheme _elementTheme;
    [ObservableProperty] private bool uiIsEnabled;
    [ObservableProperty] private bool desktopServiceIsOn;
    private string _versionDescription;
    private const string LicenseHelperExe =
        @"C:\Program Files (x86)\Common Files\Autodesk Shared\AdskLicensing\Current\helper\AdskLicensingInstHelper.exe";

    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, IGenericMessageDialogService messageDialogService)
    {
        var themeSelectorService1 = themeSelectorService;
        _messageDialogService = messageDialogService;
        _elementTheme = themeSelectorService1.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async void (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await themeSelectorService1.SetThemeAsync(param);
                }
            });

        CheckPath();
        CheckLicensingService();
    }

    private void CheckLicensingService()
    {
        try
        {
            var sc = new ServiceController("AdskLicensingService");
            DesktopServiceIsOn = sc.Status == ServiceControllerStatus.Running;
        }
        catch (InvalidOperationException) // license service is not installed
        {
            DesktopServiceIsOn = false;
        }
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    private void CheckPath() => UiIsEnabled = File.Exists(LicenseHelperExe);

    [RelayCommand]
    private async Task PrintListCopy()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText($"\"{LicenseHelperExe}\" list");
        Clipboard.SetContent(dataPackage);

        var dialogSettings = new DialogSettings()
        {
            Title = "Command copied",
            Message = "Print list command was copied. Use it in a terminal window. ",
            Color = Color.FromArgb(255, 160, 209, 77),
            Symbol = ((char)0xE73E).ToString(),
        };
        await _messageDialogService.ShowDialog(dialogSettings);

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task PrintList()
    {
        var process = new Process();

        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = $"/c \"{LicenseHelperExe}\" list";
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;

        process.Start();
        string output;
        using (var reader = process.StandardOutput)
        {
            output = await reader.ReadToEndAsync();
        }
        await File.WriteAllTextAsync(Path.Combine(Path.GetTempPath(), "AdskLicenseOutput.json"), output);

        var dialogSettings = new DialogSettings()
        {
            Title = "Export successful",
            Message = $"Export was successful. Do you want to open the file from {Path.Combine(Path.GetTempPath(), "AdskLicenseOutput.json")} ?",
            Color = Color.FromArgb(255, 160, 209, 77),
            Symbol = ((char)0xE73E).ToString(),
            PrimaryButtonIsEnabled = true,
            PrimaryButtonCommand = OpenLicenseOutputCommand,
            PrimaryButtonText = "Yes",
            SecondaryButtonIsEnabled = true,
            SecondaryButtonText = "No"
        };
        await _messageDialogService.ShowDialog(dialogSettings);

        await Task.CompletedTask;
    }

    [RelayCommand]
    private void OpenLicenseOutput()
    {
        Process.Start("notepad.exe", Path.Combine(Path.GetTempPath(), "AdskLicenseOutput.json"));
    }

    [RelayCommand]
    private async Task OpenLoginStatePath()
    {
        var appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var path = $@"{appDataFolderPath}\Autodesk\Web Services";
        var folder = await StorageFolder.GetFolderFromPathAsync(path);
        if (Directory.Exists(path))
        {
            await Launcher.LaunchFolderAsync(folder);
            return;
        }

        var dialogSettings = new DialogSettings()
        {
            Title = "Path not found",
            Message = "Path was not found and could not be opened.",
            Color = Color.FromArgb(255, 234, 93, 97),
            Symbol = ((char)0xEA39).ToString(),
        };
        await _messageDialogService.ShowDialog(dialogSettings);

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OpenAdskLicensingPath()
    {
        const string path = @"C:\ProgramData\Autodesk\AdskLicensingService";
        if (Directory.Exists(path))
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(path);
            await Launcher.LaunchFolderAsync(folder);

            return;
        }

        var dialogSettings = new DialogSettings()
        {
            Title = "Path not found",
            Message = "Path was not found and could not be opened.",
            Color = Color.FromArgb(255, 234, 93, 97),
            Symbol = ((char)0xEA39).ToString(),
        };
        await _messageDialogService.ShowDialog(dialogSettings);

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OpenAdskIdentityServicePath()
    {
        var appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var path = $@"{appDataFolderPath}\Autodesk\Identity Services";
        var folder = await StorageFolder.GetFolderFromPathAsync(path);

        if (Directory.Exists(path))
        {
            await Launcher.LaunchFolderAsync(folder);

            return;
        }

        var dialogSettings = new DialogSettings()
        {
            Title = "Path not found",
            Message = "Path was not found and could not be opened.",
            Color = Color.FromArgb(255, 234, 93, 97),
            Symbol = ((char)0xEA39).ToString(),
        };
        await _messageDialogService.ShowDialog(dialogSettings);

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async void OpenAdskLicensingInstHelperPath()
    {
        const string path = @"C:\Program Files (x86)\Common Files\Autodesk Shared\AdskLicensing\Current\helper";
        var folder = await StorageFolder.GetFolderFromPathAsync(path);

        if (Directory.Exists(path))
        {
            await Launcher.LaunchFolderAsync(folder);
            return;
        }

        var dialogSettings = new DialogSettings()
        {
            Title = "Path not found",
            Message = "Path was not found and could not be opened.",
            Color = Color.FromArgb(255, 234, 93, 97),
            Symbol = ((char)0xEA39).ToString(),
        };
        await _messageDialogService.ShowDialog(dialogSettings);
    }

    public async void DesktopLicensingServiceToggled(object sender, RoutedEventArgs e)
    {
        var sc = new ServiceController("AdskLicensingService");
        try
        {
            UiIsEnabled = true;
        }
        catch (InvalidOperationException)
        {
            var dialogSettings = new DialogSettings()
            {
                Title = "Service not found",
                Message = "Service AdskLicensingService was not found. You will be only able to generate commands, but not running them. Some Options are deactivated.",
                Color = Color.FromArgb(255, 234, 93, 97),
                Symbol = ((char)0xEA39).ToString(),
            };
            await _messageDialogService.ShowDialog(dialogSettings);
            UiIsEnabled = false;
            return;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }

        var toggleSwitch = (ToggleSwitch)sender;
        switch (toggleSwitch.IsOn)
        {
            case true:
                {
                    if (sc.Status is ServiceControllerStatus.Running or ServiceControllerStatus.StartPending)
                    {
                        return;
                    }

                    sc.Start();
                    break;
                }
            case false:
                {
                    if (sc.Status is ServiceControllerStatus.Stopped or ServiceControllerStatus.StopPending)
                    {
                        return;
                    }

                    sc.Stop();
                    break;
                }
        }
    }


    [RelayCommand]
    public void RefreshDesktopLicensingState()
    {
        CheckLicensingService();
    }
}