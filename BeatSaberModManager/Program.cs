using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;

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
using BeatSaberModManager.Views;
using BeatSaberModManager.Views.Localization;
using BeatSaberModManager.Views.Pages;
using BeatSaberModManager.Views.Theming;
using BeatSaberModManager.Views.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using ReactiveUI;

using Serilog;


namespace BeatSaberModManager
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            await using ServiceProvider services = CreateServiceCollection(args).BuildServiceProvider();
            return await services.GetRequiredService<Startup>().RunAsync();
        }

        private static IServiceCollection CreateServiceCollection(IReadOnlyList<string> args) =>
            new ServiceCollection()
                .AddSingleton<Startup>()
                .AddSerilog()
                .AddSettings()
                .AddUpdater()
                .AddCoreServices()
                .AddModServices()
                .AddAssetProviders()
                .AddProtocolHandlerRegistrar()
                .AddApplication()
                .AddViewModels()
                .AddViews(args);

        private static IServiceCollection AddSerilog(this IServiceCollection services) =>
            services.AddLogging(static loggerBuilder =>
                loggerBuilder.AddSerilog(
                    new LoggerConfiguration()
                        .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(BeatSaberModManager), "log.txt"))
                        .CreateLogger(), true));

        private static IServiceCollection AddSettings(this IServiceCollection services) =>
            services.AddSingleton<IOptions<AppSettings>, JsonSettingsProvider<AppSettings>>();

        private static IServiceCollection AddCoreServices(this IServiceCollection services) =>
            services.AddSingleton(typeof(Program).Assembly.GetName())
                .AddSingleton<HttpProgressClient>()
                .AddSingleton<IStatusProgress, StatusProgress>()
                .AddSingleton<IInstallDirLocator, BeatSaberInstallDirLocator>()
                .AddSingleton<IInstallDirValidator, BeatSaberInstallDirValidator>()
                .AddSingleton<IGameLauncher, BeatSaberGameLauncher>();

        private static IServiceCollection AddProtocolHandlerRegistrar(this IServiceCollection services) =>
            OperatingSystem.IsWindows() ? services.AddSingleton<IProtocolHandlerRegistrar, WindowsProtocolHandlerRegistrar>()
                : OperatingSystem.IsLinux() ? services.AddSingleton<IProtocolHandlerRegistrar, LinuxProtocolHandlerRegistrar>()
                    : throw new PlatformNotSupportedException();

        private static IServiceCollection AddModServices(this IServiceCollection services) =>
            services.AddSingleton<IHashProvider, MD5HashProvider>()
                .AddSingleton<IDependencyResolver, DependencyResolver>()
                .AddSingleton<IModProvider, BeatModsModProvider>()
                .AddSingleton<IModInstaller, BeatModsModInstaller>()
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
                .AddSingleton<DashboardViewModel>()
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
                    .AddSingleton<Window, AssetInstallWindow>()
                : services.AddSingleton<Window, MainWindow>()
                    .AddSingleton<IViewFor<DashboardViewModel>, DashboardPage>()
                    .AddSingleton<IViewFor<ModsViewModel>, ModsPage>()
                    .AddSingleton<IViewFor<SettingsViewModel>, SettingsPage>();
    }
}