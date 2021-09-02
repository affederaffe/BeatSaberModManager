using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Localisation;
using BeatSaberModManager.Models.Implementations.BeatSaber;
using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Models.Implementations.BeatSaber.ModelSaber;
using BeatSaberModManager.Models.Implementations.BeatSaber.Playlist;
using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Models.Implementations.ProtocolHandlerRegistrars;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Theming;
using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views;

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
            host.Services.StartAvaloniaApp();
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

        private static void StartAvaloniaApp(this IServiceProvider services)
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
            services.AddCoreUI();
            services.AddCoreServices();
            services.AddAssetProviders();
            if (args.Length is 2 && args[0] == "--install")
            {
                services.AddSingleton(new Uri(args[1]));
                services.AddSingleton<AssetInstallWindowViewModel>();
                services.AddSingleton<Window, AssetInstallWindow>();
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

        private static void AddCoreUI(this IServiceCollection services)
        {
            services.AddSingleton<Application, App>();
            services.AddSingleton<LocalisationManager>();
            services.AddSingleton<ThemeManager>();
        }
    }
}