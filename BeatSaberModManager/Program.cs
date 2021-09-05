using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Services.Implementations.BeatSaber;
using BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Services.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Services.Implementations.BeatSaber.ModelSaber;
using BeatSaberModManager.Services.Implementations.BeatSaber.Playlist;
using BeatSaberModManager.Services.Implementations.ProtocolHandlerRegistrars;
using BeatSaberModManager.Services.Implementations.Settings;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Services.Progress;
using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Implementations.Localisation;
using BeatSaberModManager.Views.Implementations.Pages;
using BeatSaberModManager.Views.Implementations.Theming;
using BeatSaberModManager.Views.Implementations.Windows;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;


namespace BeatSaberModManager
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            BuildAvaloniaApp();
            using IHost host = CreateHostBuilder(args).Build();
            await host.StartAsync();
            RunAvaloniaApp(host.Services);
            await host.StopAsync();
        }

        private static void BuildAvaloniaApp()
        {
            AppBuilder builder = AppBuilder.Configure<App>().UsePlatformDetect().UseReactiveUI();
            builder.RuntimePlatformServicesInitializer();
            builder.WindowingSubsystemInitializer();
            builder.RenderingSubsystemInitializer();
            builder.AfterPlatformServicesSetupCallback(builder);
        }

        private static void RunAvaloniaApp(IServiceProvider services)
        {
            Application app = services.GetRequiredService<Application>();
            AvaloniaLocator.CurrentMutable.BindToSelf(app);
            ClassicDesktopStyleApplicationLifetime lifetime = new();
            app.ApplicationLifetime = lifetime;
            app.RegisterServices();
            app.Initialize();
            app.OnFrameworkInitializationCompleted();
            lifetime.MainWindow = services.GetRequiredService<Window>();
            lifetime.Start(Array.Empty<string>());
        }

        private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).ConfigureServices(services =>
        {
            services.AddCoreServices();
            services.AddApplication();
            services.AddAssetProviders();
            if (args.Length is 2 && args[0] == "--install")
            {
                services.AddSingleton<AssetInstallWindowViewModel>();
                services.AddSingleton<Window, AssetInstallWindow>(provider => new AssetInstallWindow(provider.GetRequiredService<AssetInstallWindowViewModel>(), new Uri(args[1])));
            }
            else
            {
                services.AddProtocolHandlerRegistrar();
                services.AddModServices();
                services.AddViewModels();
                services.AddViews();
            }
        });

        private static void AddCoreServices(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<IStatusProgress, StatusProgress>();
            services.AddSingleton<IInstallDirLocator, BeatSaberInstallDirLocator>();
            services.AddSingleton<IInstallDirValidator, BeatSaberInstallDirValidator>();
            services.AddSingleton<IOptions<SettingsStore>, SettingsManager>();
        }

        private static void AddProtocolHandlerRegistrar(this IServiceCollection services)
        {
            if (OperatingSystem.IsWindows()) services.AddSingleton<IProtocolHandlerRegistrar, WindowsProtocolHandlerRegistrar>();
            else if (OperatingSystem.IsLinux()) services.AddSingleton<IProtocolHandlerRegistrar, LinuxProtocolHandlerRegistrar>();
        }

        private static void AddModServices(this IServiceCollection services)
        {
            services.AddSingleton<IHashProvider, MD5HashProvider>();
            services.AddSingleton<IModProvider, BeatModsModProvider>();
            services.AddSingleton<IModInstaller, BeatModsModInstaller>();
            services.AddSingleton<IModVersionComparer, SystemVersionComparer>();
            services.AddSingleton<IGameVersionProvider, BeatSaberGameVersionProvider>();
        }

        private static void AddAssetProviders(this IServiceCollection services)
        {
            services.AddSingleton<BeatSaverMapInstaller>();
            services.AddSingleton<IAssetProvider, BeatSaverAssetProvider>();
            services.AddSingleton<ModelSaberModelInstaller>();
            services.AddSingleton<IAssetProvider, ModelSaberAssetProvider>();
            services.AddSingleton<PlaylistInstaller>();
            services.AddSingleton<IAssetProvider, PlaylistAssetProvider>();
        }

        private static void AddViewModels(this IServiceCollection services)
        {
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<IntroView>();
            services.AddSingleton<ModsViewModel>();
            services.AddSingleton<OptionsViewModel>();
        }

        private static void AddViews(this IServiceCollection services)
        {
            services.AddSingleton<Window, MainWindow>();
            services.AddSingleton<ModsView>();
            services.AddSingleton<OptionsView>();
        }

        private static void AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<Application, App>();
            services.AddSingleton<ILocalisationManager, LocalisationManager>();
            services.AddSingleton<IThemeManager, ThemeManager>();
        }
    }
}