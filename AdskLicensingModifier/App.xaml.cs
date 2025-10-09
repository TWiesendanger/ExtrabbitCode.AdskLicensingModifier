using AdskLicensingModifier.Activation;
using AdskLicensingModifier.Contracts.Services;
using AdskLicensingModifier.Core.Contracts.Services;
using AdskLicensingModifier.Core.Services;
using AdskLicensingModifier.Models;
using AdskLicensingModifier.Services;
using AdskLicensingModifier.ViewModels;
using AdskLicensingModifier.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MainViewModel = AdskLicensingModifier.ViewModels.MainViewModel;

namespace AdskLicensingModifier;

public partial class App
{
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((_, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IGenericMessageDialogService, GenericMessageDialogService>();


            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<ModifyLicensingViewModel>();
            services.AddTransient<ModifyLicensingPage>();
            services.AddTransient<ProductKeyViewModel>();
            services.AddTransient<ProductKeyPage>();
            services.AddTransient<ModifyLicensingPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.AddSingleton(sp =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var s = cfg.GetRequiredSection(nameof(LocalSettingsOptions));

                var options = new LocalSettingsOptions
                {
                    ApplicationDataFolder = s["ApplicationDataFolder"],
                    LocalSettingsFile = s["LocalSettingsFile"]
                };

                return options;
            });
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await GetService<IActivationService>().ActivateAsync(args);
    }
}
