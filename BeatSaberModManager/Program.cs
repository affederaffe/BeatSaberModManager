using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.BeatSaber;
using BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Services.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Services.Implementations.BeatSaber.ModelSaber;
using BeatSaberModManager.Services.Implementations.BeatSaber.Playlists;
using BeatSaberModManager.Services.Implementations.DependencyManagement;
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

using ReactiveUI;


namespace BeatSaberModManager
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            using ServiceProvider services = CreateServiceCollection(args).BuildServiceProvider();
            return AppBuilder.Configure(services.GetRequiredService<Application>).UsePlatformDetect().UseReactiveUI().StartWithClassicDesktopLifetime(args);
        }

        private static IServiceCollection CreateServiceCollection(IReadOnlyList<string> args) =>
            new ServiceCollection()
                .AddCoreServices()
                .AddApplication()
                .AddAssetProviders()
                .AddWindows(args);

        private static IServiceCollection AddWindows(this IServiceCollection services, IReadOnlyList<string> args) =>
            args.Count is 2 && args[0] == "--install"
                ? services.AddSingleton(new Uri(args[1]))
                    .AddSingleton<AssetInstallWindowViewModel>()
                    .AddSingleton<Window, AssetInstallWindow>()
                : services.AddProtocolHandlerRegistrar()
                    .AddModServices()
                    .AddViewModels()
                    .AddViews();

        private static IServiceCollection AddCoreServices(this IServiceCollection services) =>
            services.AddSingleton<HttpProgressClient>()
                .AddSingleton<IStatusProgress, StatusProgress>()
                .AddSingleton<IInstallDirLocator, BeatSaberInstallDirLocator>()
                .AddSingleton<IInstallDirValidator, BeatSaberInstallDirValidator>()
                .AddSingleton<ISettings<AppSettings>, JsonSettingsProvider<AppSettings>>();

        private static IServiceCollection AddProtocolHandlerRegistrar(this IServiceCollection services) =>
            OperatingSystem.IsWindows() ? services.AddSingleton<IProtocolHandlerRegistrar, WindowsProtocolHandlerRegistrar>()
                : OperatingSystem.IsLinux() ? services.AddSingleton<IProtocolHandlerRegistrar, LinuxProtocolHandlerRegistrar>()
                    : throw new PlatformNotSupportedException();

        private static IServiceCollection AddModServices(this IServiceCollection services) =>
            services.AddSingleton<IHashProvider, MD5HashProvider>()
                .AddSingleton<IDependencyResolver, DependencyResolver>()
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
                .AddSingleton<SettingsViewModel>();

        private static IServiceCollection AddViews(this IServiceCollection services) =>
            services.AddSingleton<Window, MainWindow>()
                .AddSingleton<IViewFor<ModsViewModel>, ModsPage>()
                .AddSingleton<IViewFor<SettingsViewModel>, SettingsPage>();

        private static IServiceCollection AddApplication(this IServiceCollection services) =>
            services.AddSingleton<Application, App>()
                .AddSingleton<ILocalisationManager, LocalisationManager>()
                .AddSingleton<IThemeManager, ThemeManager>();
    }
}