using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using AdskLicensingModifier.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using AdskLicensingModifier.Contracts.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
// ReSharper disable InconsistentNaming

namespace AdskLicensingModifier.ViewModels;

public partial class ModifyLicensingViewModel : ObservableObject
{
    private readonly IGenericMessageDialogService _messageDialogService;
    public Task Initialization { get; }

    private const string LicenseHelperExe =
        @"C:\Program Files (x86)\Common Files\Autodesk Shared\AdskLicensing\Current\helper\AdskLicensingInstHelper.exe";
    [ObservableProperty] public partial string? SearchText { get; set;}
    [ObservableProperty] public partial bool ResultAskRun { get; set; }
    [ObservableProperty] public partial bool WasRunCommandSuccessful { get; set; }
    [ObservableProperty] public partial string Result { get; set; } = "";
    [ObservableProperty] public partial string? ServerNames { get; set; }
    [ObservableProperty] public partial LicenseType SelectedLicenseType { get; set; } = LicenseType.Reset;
    [ObservableProperty] public partial ServerType SelectedServerType { get; set; }
    [ObservableProperty] public partial List<ServerType>? ServerTypes { get; set; }
    [ObservableProperty] public partial List<LicenseType>? LicenseTypes { get; set; }
    [ObservableProperty] public partial bool UiIsEnabled { get; set; }
    [ObservableProperty] public partial bool CommandDialogBarOpen { get; set; }
    [ObservableProperty] public partial bool ServerTypeIsEnabled { get; set; }
    [ObservableProperty] public partial Dictionary<string, string>? AdskProducts { get; set; } = [];
    [ObservableProperty]
    public ObservableCollection<KeyValuePair<string, string>>? FilteredAdskProducts { get; set; } = [];
    public Dictionary<string, string>? FilteredYearAdskProducts { get; set; }
    [ObservableProperty] public partial KeyValuePair<string, string> SelectedProduct { get; set; }
    [ObservableProperty] public partial string SelectedYear { get; set; } = "2026";
    private string _productFeatureCode = "2026.0.0.F";

    public ModifyLicensingViewModel(IGenericMessageDialogService messageDialogService)
    {
        _messageDialogService = messageDialogService;
        Initialization = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        AdskProducts = await ReadAutodeskProductsAsync();
        FilteredAdskProducts ??= new ObservableCollection<KeyValuePair<string, string>>();

        if (AdskProducts != null)
        {
            FilteredYearAdskProducts = string.IsNullOrEmpty(SelectedYear)
                ? AdskProducts
                : AdskProducts.Where(x => x.Key.Contains(SelectedYear, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        UpdateFilteredProductsCollection();

        ServerTypes = [..Enum.GetValues(typeof(ServerType)).Cast<ServerType>()];
        LicenseTypes = [..Enum.GetValues(typeof(LicenseType)).Cast<LicenseType>()];

        await CheckPathAsync();
        SetFeatureCode(SelectedYear);
    }

    private void UpdateFilteredProductsCollection()
    {
        FilteredAdskProducts?.Clear();

        if (FilteredYearAdskProducts != null)
        {
            foreach (var item in FilteredYearAdskProducts)
            {
                FilteredAdskProducts?.Add(item);
            }
        }
    }

    partial void OnSelectedYearChanged(string value)
    {
        SelectedProduct = new KeyValuePair<string, string>();
        if (AdskProducts != null)
        {
            FilteredYearAdskProducts = string.IsNullOrEmpty(value)
                ? AdskProducts
                : AdskProducts.Where(x => x.Key.Contains(value, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        UpdateFilteredProductsCollection();

        SetFeatureCode(value);
        SearchText = "";
    }

    private void SetFeatureCode(string value) =>
        _productFeatureCode = value switch
        {
            "2020" => "2020.0.0.F",
            "2021" => "2021.0.0.F",
            "2022" => "2022.0.0.F",
            "2023" => "2023.0.0.F",
            "2024" => "2024.0.0.F",
            "2025" => "2025.0.0.F",
            "2026" => "2026.0.0.F",
            _ => ""
        };

    private static async Task<Dictionary<string, string>?> ReadAutodeskProductsAsync()
    {
        var uri = new Uri("ms-appx:///Assets/resources/AutodeskProducts.txt");
        var productKeys = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
        var productKeyList = await Windows.Storage.FileIO.ReadLinesAsync(productKeys);

        return productKeyList
            .Select(line => line.Split(';'))
            .ToDictionary(parts => parts[0], parts => parts[1]);
    }

    private async Task CheckPathAsync()
    {

        if (File.Exists(LicenseHelperExe) == false)
        {
            var dialogSettings = new DialogSettings()
            {
                Title = "AdskLicensingInstHelper not found",
                Message = "AdskLicensingInstHelper was not found. This exe is needed to be able to change any license settings for autodesk products. Make sure you have the autodesk licensing service installed. ",
                Color = ResourceHelper.GetColor("Error"),
                Symbol = ResourceHelper.GetString("ErrorSymbol")
            };
            UiIsEnabled = false;
            await _messageDialogService.ShowDialog(dialogSettings);
            return;
        }
        UiIsEnabled = true;
    }

    partial void OnSearchTextChanged(string? value)
    {
        SelectedProduct = new KeyValuePair<string, string>();

        if (FilteredYearAdskProducts != null)
        {
            var filtered = string.IsNullOrEmpty(value)
                ? FilteredYearAdskProducts
                : FilteredYearAdskProducts.Where(x => x.Key.Contains(value, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);

            FilteredAdskProducts?.Clear();
            foreach (var item in filtered)
            {
                FilteredAdskProducts?.Add(item);
            }
        }
    }

    partial void OnSelectedLicenseTypeChanged(LicenseType value)
    {
        Result = "";
        ServerNames = "";
        switch (value)
        {
            case LicenseType.Network:
                SelectedServerType = SelectedServerType = ServerType.Single;
                ServerTypeIsEnabled = true;
                break;
            default:
                SelectedServerType = SelectedServerType;
                ServerTypeIsEnabled = false;
                break;
        }
    }

    [RelayCommand]
    public async Task Generate()
    {
        if (string.IsNullOrWhiteSpace(SelectedProduct.Key))
        {
            var dialogSettings = new DialogSettings()
            {
                Title = "No product selected",
                Message = "No product was selected. Make sure you have selected one product. ",
                Color = ResourceHelper.GetColor("Error"),
                Symbol = ResourceHelper.GetString("ErrorSymbol")
            };
            await _messageDialogService.ShowDialog(dialogSettings);
            return;
        }

        var cmdCommand = SetCmdCommand();
        Result = cmdCommand;

        var dataPackage = new DataPackage();
        dataPackage.SetText(cmdCommand);
        Clipboard.SetContent(dataPackage);
    }

    private string SetCmdCommand()
    {
        var cmdCommand = SelectedLicenseType switch
        {
            LicenseType.Network =>
                $@"""{LicenseHelperExe}"" change --prod_key {SelectedProduct.Value} --prod_ver {_productFeatureCode} --lic_method {SelectedLicenseType.ToString().ToUpper()} --lic_server_type {SelectedServerType.ToString().ToUpper()} --lic_servers {ServerNames}",
            LicenseType.Standalone or LicenseType.User =>
                $@"""{LicenseHelperExe}"" change --prod_key {SelectedProduct.Value} --prod_ver {_productFeatureCode} --lic_method {SelectedLicenseType.ToString().ToUpper()}",
            LicenseType.Reset =>
                $@"""{LicenseHelperExe}"" change --prod_key {SelectedProduct.Value} --prod_ver {_productFeatureCode} --lic_method """" --lic_server_type """" --lic_servers """"",
            _ => ""
        };
        return cmdCommand;
    }

    [RelayCommand]
    public async Task Run()
    {
        if (string.IsNullOrWhiteSpace(SelectedProduct.Key))
        {
            var dialogSettings = new DialogSettings()
            {
                Title = "No product selected",
                Message = "No product was selected. Make sure you have selected one product. ",
                Color = ResourceHelper.GetColor("Error"),
                Symbol = ResourceHelper.GetString("ErrorSymbol")
            };
            await _messageDialogService.ShowDialog(dialogSettings);
            return;
        }

        var dialogSettingsConfirmation = new DialogSettings()
        {
            Title = "Are you sure?",
            Message = "Are you sure that you want to run the command locally? ",
            PrimaryButtonText = "Yes",
            PrimaryButtonCommand = SetAskRunResultCommand,
            SecondaryButtonText = "No",
            Color = ResourceHelper.GetColor("Information"),
            Symbol = ResourceHelper.GetString("QuestionMarkSymbol")
        };
        await _messageDialogService.ShowDialog(dialogSettingsConfirmation);

        if (ResultAskRun)
        {
            await RunCmd();
        }
    }

    [RelayCommand]
    public Task SetAskRunResult() => Task.FromResult(ResultAskRun = true);

    [RelayCommand]
    public async Task RunCmd()
    {
        if (Result.Length == 0)
        {
            Result = SetCmdCommand();
        }

        // Stop adSSO.exe
        var adsso = Process.GetProcessesByName("AdSSO").FirstOrDefault();
        adsso?.Kill();

        // not possible because of sandbox environment
        // not sure if needed
        if (SelectedLicenseType == LicenseType.Network)
        {
            var keyName = @"Software";
            using var key = Registry.CurrentUser.OpenSubKey(keyName, true);
            key?.GetSubKeyNames();
            try
            {
                key?.DeleteSubKeyTree("FLEXlm License Manager");
            }
            catch (Exception)
            {
                // key does not exist / at least not in the virtualization
            }
        }

        var process = new Process();

        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = $@"/c ""{Result}""";
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.Start();

        if (SelectedLicenseType == LicenseType.Reset)
        {
            await DeleteLoginStateAsync();
            await DeleteIdServiceDbAsync();
        }
        else
        {
            WasRunCommandSuccessful = true;
        }

        if (WasRunCommandSuccessful)
        {
            CommandDialogBarOpen = true;
        }

        ResultAskRun = false;
        WasRunCommandSuccessful = false;
        await Task.CompletedTask;
    }

    private async Task DeleteIdServiceDbAsync()
    {
        var appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var idServiceDbFile = $@"{appDataFolderPath}\Autodesk\Identity Services\idservices.db";

        if (File.Exists(idServiceDbFile))
        {
            try
            {
                // process has to stop to allow renaming db
                var processes = Process.GetProcessesByName("AdskIdentityManager");
                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                }

                var newFileName = $"idservices.db_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}";
                var newFilePath = Path.Combine(Path.GetDirectoryName(idServiceDbFile) ?? string.Empty, newFileName);

                File.Move(idServiceDbFile, newFilePath);
                WasRunCommandSuccessful = true;
            }
            catch (IOException)
            {
                WasRunCommandSuccessful = false;
                var dialogSettingsConfirmation = new DialogSettings()
                {
                    Title = "File in use or access denied",
                    Message =
                        "idservices.db could not be renamed because the file is either in use or you don't have access to it. This can lead to resetting not working. ",
                    PrimaryButtonText = "OK",
                    Color = ResourceHelper.GetColor("Error"),
                    Symbol = ResourceHelper.GetString("ErrorSymbol")
                };
                await _messageDialogService.ShowDialog(dialogSettingsConfirmation);
            }
            catch (Exception ex)
            {
                WasRunCommandSuccessful = false;
                var dialogSettingsConfirmation = new DialogSettings()
                {
                    Title = "Error",
                    Message = $"There was an error while running a command. The error was: {ex}.",
                    PrimaryButtonText = "OK",
                    Color = ResourceHelper.GetColor("Error"),
                    Symbol = ResourceHelper.GetString("ErrorSymbol")
                };
                await _messageDialogService.ShowDialog(dialogSettingsConfirmation);
            }
        }
    }

    private async Task DeleteLoginStateAsync()
    {
        var appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var loginStateFile = $@"{appDataFolderPath}\Autodesk\Web Services\LoginState.xml";
        if (File.Exists(loginStateFile))
        {
            try
            {
                var newFileName = $"LoginState.xml_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}";
                var newFilePath = Path.Combine(Path.GetDirectoryName(loginStateFile) ?? string.Empty, newFileName);
                File.Move(loginStateFile, newFilePath);
                WasRunCommandSuccessful = true;
            }
            catch (IOException)
            {
                WasRunCommandSuccessful = false;
                var dialogSettingsConfirmation = new DialogSettings()
                {
                    Title = "File in use or access denied",
                    Message =
                        "LoginState.xml could not be renamed because the file is either in use or you don't have access to it. This can lead to resetting not working. ",
                    PrimaryButtonText = "OK",
                    Color = ResourceHelper.GetColor("Error"),
                    Symbol = ResourceHelper.GetString("ErrorSymbol")
                };
                await _messageDialogService.ShowDialog(dialogSettingsConfirmation);
            }
            catch (Exception ex)
            {
                WasRunCommandSuccessful = false;
                var dialogSettingsConfirmation = new DialogSettings()
                {
                    Title = "Error",
                    Message = $"There was an error while running a command. The error was: {ex}.",
                    PrimaryButtonText = "OK",
                    Color = ResourceHelper.GetColor("Error"),
                    Symbol = ResourceHelper.GetString("ErrorSymbol")
                };
                await _messageDialogService.ShowDialog(dialogSettingsConfirmation);
            }
        }
    }

    public void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var productListView = (ListView)sender;
        if (productListView.SelectedItem is null)
        {
            return;
        }
        SelectedProduct = (KeyValuePair<string, string>)productListView.SelectedItem;
    }
}
