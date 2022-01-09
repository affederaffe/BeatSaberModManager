using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Settings;
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
using BeatSaberModManager.Services.Implementations.Updater;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Implementations;
using BeatSaberModManager.Views.Implementations.Localization;
using BeatSaberModManager.Views.Implementations.Pages;
using BeatSaberModManager.Views.Implementations.Theming;
using BeatSaberModManager.Views.Implementations.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using ReactiveUI;

using SkiaSharp;


namespace BeatSaberModManager
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            await host.StartAsync().ConfigureAwait(false);
            int exitCode = await host.RunAvaloniaApp().ConfigureAwait(false);
            await host.StopAsync().ConfigureAwait(false);
            return exitCode;
        }

        private static async Task<int> RunAvaloniaApp(this IHost host)
        {
            IUpdater updater = host.Services.GetRequiredService<IUpdater>();
            if (await updater.NeedsUpdate().ConfigureAwait(false)) return await updater.Update().ConfigureAwait(false);
            X11PlatformOptions x11Options = new() { UseEGL = true };
            FontManagerOptions fontManagerOptions = new() { DefaultFamilyName = string.IsNullOrEmpty(SKTypeface.Default.FamilyName) ? SKFontManager.Default.GetFamilyName(0) : SKTypeface.Default.FamilyName };
            return AppBuilder.Configure(host.Services.GetRequiredService<Application>).With(fontManagerOptions).With(x11Options).UsePlatformDetect().UseReactiveUI().StartWithClassicDesktopLifetime(null!);
        }

        private static IHostBuilder CreateHostBuilder(IReadOnlyList<string> args) =>
            Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                    services.AddHostedService<Startup>()
                        .AddSettings()
                        .AddCoreServices()
                        .AddModServices()
                        .AddUpdater()
                        .AddAssetProviders()
                        .AddProtocolHandlerRegistrar()
                        .AddApplication()
                        .AddViewModels()
                        .AddViews(args));

        private static IServiceCollection AddSettings(this IServiceCollection services) =>
            services.AddSingleton<IOptions<AppSettings>, JsonSettingsProvider<AppSettings>>();

        private static IServiceCollection AddCoreServices(this IServiceCollection services) =>
            services.AddSingleton(typeof(Program).Assembly.GetName().Version!)
                .AddSingleton<HttpProgressClient>()
                .AddSingleton<IStatusProgress, StatusProgress>()
                .AddSingleton<IInstallDirLocator, BeatSaberInstallDirLocator>()
                .AddSingleton<IInstallDirValidator, BeatSaberInstallDirValidator>();

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

        private static IServiceCollection AddUpdater(this IServiceCollection services) =>
            services.AddSingleton<IUpdater, GitHubUpdater>();

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
                .AddSingleton<SettingsViewModel>()
                .AddSingleton<AssetInstallWindowViewModel>();

        private static IServiceCollection AddApplication(this IServiceCollection services) =>
            services.AddSingleton<Application, App>()
                .AddSingleton<LocalizationManager>()
                .AddSingleton<ThemeManager>();

        private static IServiceCollection AddViews(this IServiceCollection services, IReadOnlyList<string> args) =>
            args.Count is 2 && args[0] == "--install"
                ? services.AddSingleton(new Uri(args[1]))
                    .AddSingleton<AssetInstallWindowViewModel>()
                    .AddSingleton<Window, AssetInstallWindow>()
                : services.AddSingleton<Window, MainWindow>()
                    .AddSingleton<IViewFor<ModsViewModel>, ModsPage>()
                    .AddSingleton<IViewFor<SettingsViewModel>, SettingsPage>();
    }
}