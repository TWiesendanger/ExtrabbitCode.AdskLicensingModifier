using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
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
    [ObservableProperty] private string? searchText;
    [ObservableProperty] private bool resultAskRun;
    [ObservableProperty] private bool wasRunCommandSuccessful;
    [ObservableProperty] private string result = "";
    [ObservableProperty] private string? serverNames;
    [ObservableProperty] private LicenseType selectedLicenseType = LicenseType.Reset;
    [ObservableProperty] private ServerType selectedServerType;
    [ObservableProperty] private List<ServerType>? serverTypes;
    [ObservableProperty] private List<LicenseType>? licenseTypes;
    [ObservableProperty] private bool uiIsEnabled;
    [ObservableProperty] private bool commandDialogBarOpen;
    [ObservableProperty] private bool serverTypeIsEnabled;
    [ObservableProperty] private Dictionary<string, string>? adskProducts;
    [ObservableProperty] private Dictionary<string, string>? filteredAdskProducts;
    private Dictionary<string, string>? _filteredYearAdskProducts;
    [ObservableProperty] private KeyValuePair<string, string> selectedProduct;
    [ObservableProperty] private string selectedYear = "2025";
    private string _productFeatureCode = "2025.0.0.F";

    public ModifyLicensingViewModel(IGenericMessageDialogService messageDialogService)
    {
        _messageDialogService = messageDialogService;
        Initialization = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        AdskProducts = await ReadAutodeskProductsAsync();
        FilteredAdskProducts = new Dictionary<string, string>();
        _filteredYearAdskProducts = new Dictionary<string, string>();
        if (AdskProducts != null)
        {
            _filteredYearAdskProducts = string.IsNullOrEmpty(SelectedYear)
                ? AdskProducts
                : AdskProducts.Where(x => x.Key.Contains(SelectedYear, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        FilteredAdskProducts = _filteredYearAdskProducts;

        ServerTypes = new List<ServerType>(Enum.GetValues(typeof(ServerType)).Cast<ServerType>());
        LicenseTypes = new List<LicenseType>(Enum.GetValues(typeof(LicenseType)).Cast<LicenseType>());

        await CheckPathAsync();
        SetFeatureCode(SelectedYear);

    }

    partial void OnSelectedYearChanged(string value)
    {
        SelectedProduct = new KeyValuePair<string, string>();
        if (AdskProducts != null)
        {
            _filteredYearAdskProducts = string.IsNullOrEmpty(value)
                ? AdskProducts
                : AdskProducts.Where(x => x.Key.Contains(value, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        FilteredAdskProducts = _filteredYearAdskProducts;

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

    private async Task<Dictionary<string, string>?> ReadAutodeskProductsAsync()
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

        if (_filteredYearAdskProducts != null)
        {
            FilteredAdskProducts = string.IsNullOrEmpty(value)
                ? _filteredYearAdskProducts
                : _filteredYearAdskProducts.Where(x => x.Key.Contains(value, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
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
