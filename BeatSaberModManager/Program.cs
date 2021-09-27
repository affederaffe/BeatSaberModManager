using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.BeatSaber;
using BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Services.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Services.Implementations.BeatSaber.ModelSaber;
using BeatSaberModManager.Services.Implementations.BeatSaber.Playlists;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Implementations.ProtocolHandlerRegistrars;
using BeatSaberModManager.Services.Implementations.Settings;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Implementations;
using BeatSaberModManager.Views.Implementations.Localisation;
using BeatSaberModManager.Views.Implementations.Pages;
using BeatSaberModManager.Views.Implementations.Theming;
using BeatSaberModManager.Views.Implementations.Windows;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;


namespace BeatSaberModManager
{
    public static class Program
    {
        public static void Main(string[] args) =>
            CreateServiceCollection(args).BuildServiceProvider().RunAvaloniaApp();

        private static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>().UsePlatformDetect().UseReactiveUI();

        private static IServiceCollection CreateServiceCollection(IReadOnlyList<string> args) =>
            new ServiceCollection()
                .AddCoreServices()
                .AddApplication()
                .AddAssetProviders()
                .AddWindows(args);

        private static IServiceCollection AddWindows(this IServiceCollection services, IReadOnlyList<string> args) =>
            args.Count is 2 && args[0] == "--install"
                ? services.AddSingleton<AssetInstallWindowViewModel>()
                    .AddSingleton<Window, AssetInstallWindow>(provider => new AssetInstallWindow(provider.GetRequiredService<AssetInstallWindowViewModel>(), args[1]))
                : services.AddProtocolHandlerRegistrar()
                    .AddModServices()
                    .AddViewModels()
                    .AddViews();

        private static IServiceCollection AddCoreServices(this IServiceCollection services) =>
            services.AddSingleton<HttpProgressClient>()
                .AddSingleton<IStatusProgress, StatusProgress>()
                .AddSingleton<IInstallDirLocator, BeatSaberInstallDirLocator>()
                .AddSingleton<IInstallDirValidator, BeatSaberInstallDirValidator>()
                .AddSingleton<ISettings<AppSettings>, AppSettingsProvider>();

        private static IServiceCollection AddProtocolHandlerRegistrar(this IServiceCollection services) =>
            OperatingSystem.IsWindows() ? services.AddSingleton<IProtocolHandlerRegistrar, WindowsProtocolHandlerRegistrar>()
                : OperatingSystem.IsLinux() ? services.AddSingleton<IProtocolHandlerRegistrar, LinuxProtocolHandlerRegistrar>()
                    : throw new PlatformNotSupportedException();

        private static IServiceCollection AddModServices(this IServiceCollection services) =>
            services.AddSingleton<IHashProvider, MD5HashProvider>()
                .AddSingleton<IModProvider, BeatModsModProvider>()
                .AddSingleton<IModInstaller, BeatModsModInstaller>()
                .AddSingleton<IModVersionComparer, SystemVersionComparer>()
                .AddSingleton<IGameVersionProvider, BeatSaberGameVersionProvider>();

        private static IServiceCollection AddAssetProviders(this IServiceCollection services) =>
            services.AddSingleton<BeatSaverMapInstaller>()
                .AddSingleton<IAssetProvider, BeatSaverAssetProvider>()
                .AddSingleton<ModelSaberModelInstaller>()
                .AddSingleton<IAssetProvider, ModelSaberAssetProvider>()
                .AddSingleton<PlaylistInstaller>()
                .AddSingleton<IAssetProvider, PlaylistAssetProvider>();

        private static IServiceCollection AddViewModels(this IServiceCollection services) =>
            services.AddSingleton<MainWindowViewModel>()
                .AddSingleton<ModsViewModel>()
                .AddSingleton<OptionsViewModel>();

        private static IServiceCollection AddViews(this IServiceCollection services) =>
            services.AddSingleton<Window, MainWindow>()
                .AddSingleton<IPage, IntroPage>()
                .AddSingleton<IPage, ModsPage>()
                .AddSingleton<IPage, OptionsPage>();

        private static IServiceCollection AddApplication(this IServiceCollection services) =>
            services.AddSingleton(BuildAvaloniaApp())
                .AddSingleton<Application, App>()
                .AddSingleton<IClassicDesktopStyleApplicationLifetime, ClassicDesktopStyleApplicationLifetime>()
                .AddSingleton<ILocalisationManager, LocalisationManager>()
                .AddSingleton<IThemeManager, ThemeManager>();

        private static void RunAvaloniaApp(this ServiceProvider services)
        {
            AppBuilder builder = services.GetRequiredService<AppBuilder>();
            builder.RuntimePlatformServicesInitializer();
            builder.WindowingSubsystemInitializer();
            builder.RenderingSubsystemInitializer();
            builder.AfterPlatformServicesSetupCallback(builder);
            Application app = services.GetRequiredService<Application>();
            ClassicDesktopStyleApplicationLifetime lifetime = (ClassicDesktopStyleApplicationLifetime)services.GetRequiredService<IClassicDesktopStyleApplicationLifetime>();
            app.ApplicationLifetime = lifetime;
            app.RegisterServices();
            app.Initialize();
            builder.AfterSetupCallback(builder);
            app.OnFrameworkInitializationCompleted();
            lifetime.MainWindow = services.GetRequiredService<Window>();
            lifetime.Start(null);
            services.Dispose();
        }
    }
}