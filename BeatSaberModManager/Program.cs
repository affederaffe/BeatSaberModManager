using System;
using System.Globalization;
using System.IO;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

using BeatSaberModManager.Models.Implementations.Json;
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
using BeatSaberModManager.Services.Implementations.Updater;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views;
using BeatSaberModManager.Views.Localization;
using BeatSaberModManager.Views.Pages;
using BeatSaberModManager.Views.Theming;
using BeatSaberModManager.Views.Windows;

using Serilog;

using StrongInject;
using StrongInject.Modules;


namespace BeatSaberModManager
{
    /// <summary>
    /// Main application class.
    /// </summary>
    public static partial class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        public static async Task<int> Main(string[] args)
        {
            await using Container container = new(args);
            try
            {
                return await container.RunAsync<Startup, int>(static x => x.RunAsync());
            }
            catch (Exception e)
            {
                await using AsyncOwned<ILogger> logger = await container.ResolveAsync<ILogger>();
                logger.Value.Fatal(e, "Application crashed");
                if (IsProduction) return -1;
                throw;
            }
        }

        [Register<Startup>(Scope.SingleInstance)]
        [RegisterModule(typeof(CollectionsModule))]
        [RegisterModule(typeof(LazyModule))]
        [RegisterModule(typeof(SerilogModule))]
        [RegisterModule(typeof(SettingsModule))]
        [RegisterModule(typeof(HttpModule))]
        [RegisterModule(typeof(UpdaterModule))]
        [RegisterModule(typeof(ProtocolHandlerRegistrarModule))]
        [RegisterModule(typeof(GameServicesModule))]
        [RegisterModule(typeof(ModServicesModule))]
        [RegisterModule(typeof(AssetProvidersModule))]
        [RegisterModule(typeof(ViewModelModule))]
        [RegisterModule(typeof(ApplicationModule))]
        [RegisterModule(typeof(ViewsModule))]
#pragma warning disable SI1103
        internal partial class Container : IAsyncContainer<Startup>, IAsyncContainer<ILogger>
        {
            public Container(string[] args)
            {
                Args = args;
            }

            [Instance]
            private string[] Args { get; }

        }
#pragma warning restore SI1103

        internal class SerilogModule
        {
            [Factory(Scope.SingleInstance)]
            public static ILogger CreateLogger() => new LoggerConfiguration()
                .WriteTo.File(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Product, "Logs", "Log.txt"),
                    rollingInterval: RollingInterval.Day,
                    formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();
        }

        [Register(typeof(JsonSettingsProvider<AppSettings>), Scope.SingleInstance, typeof(ISettings<AppSettings>))]
        internal class SettingsModule
        {
            [Instance]
            public static readonly JsonTypeInfo<AppSettings> AppSettingsJsonTypeInfo = SettingsJsonSerializerContext.Default.AppSettings;
        }

        [Register<HttpProgressClient>(Scope.SingleInstance)]
        internal class HttpModule { }

        [Register<GitHubUpdater, IUpdater>(Scope.SingleInstance)]
        internal class UpdaterModule { }

        internal class ProtocolHandlerRegistrarModule
        {
            [Factory(Scope.SingleInstance)]
            public static IProtocolHandlerRegistrar CreateProtocolHandlerRegistrar() =>
                OperatingSystem.IsWindows() ? new WindowsProtocolHandlerRegistrar() :
                    OperatingSystem.IsLinux() ? new LinuxProtocolHandlerRegistrar() :
                        throw new PlatformNotSupportedException();
        }

        [Register<BeatSaberGameVersionProvider, IGameVersionProvider>(Scope.SingleInstance)]
        [Register<BeatSaberGamePathsProvider, IGamePathsProvider>(Scope.SingleInstance)]
        [Register<BeatSaberGameLauncher, IGameLauncher>(Scope.SingleInstance)]
        [Register<BeatSaberInstallDirLocator, IInstallDirLocator>(Scope.SingleInstance)]
        [Register<BeatSaberInstallDirValidator, IInstallDirValidator>(Scope.SingleInstance)]
        internal class GameServicesModule { }

        [Register<MD5HashProvider, IHashProvider>(Scope.SingleInstance)]
        [Register<SimpleDependencyResolver, IDependencyResolver>(Scope.SingleInstance)]
        [Register<BeatModsModProvider, IModProvider>(Scope.SingleInstance)]
        [Register<BeatModsModInstaller, IModInstaller>(Scope.SingleInstance)]
        internal class ModServicesModule { }

        [Register<BeatSaverMapInstaller>(Scope.SingleInstance)]
        [Register<BeatSaverAssetProvider, IAssetProvider>(Scope.SingleInstance)]
        [Register<ModelSaberModelInstaller>(Scope.SingleInstance)]
        [Register<ModelSaberAssetProvider, IAssetProvider>(Scope.SingleInstance)]
        [Register<PlaylistInstaller>(Scope.SingleInstance)]
        [Register<PlaylistAssetProvider, IAssetProvider>(Scope.SingleInstance)]
        internal class AssetProvidersModule { }

        [Register<MainWindowViewModel>(Scope.SingleInstance)]
        [Register<DashboardViewModel>(Scope.SingleInstance)]
        [Register<ModsViewModel>(Scope.SingleInstance)]
        [Register<SettingsViewModel>(Scope.SingleInstance)]
        [Register<AssetInstallWindowViewModel>(Scope.SingleInstance)]
        internal class ViewModelModule { }

        [Register<App, Application>(Scope.SingleInstance)]
        [Register<LocalizationManager>(Scope.SingleInstance)]
        [Register<ThemeManager>(Scope.SingleInstance)]
        [Register(typeof(StatusProgress), Scope.SingleInstance, typeof(IStatusProgress), typeof(StatusProgress))]
        internal class ApplicationModule { }

        [Register<MainWindow>(Scope.SingleInstance)]
        [Register<AssetInstallWindow>(Scope.SingleInstance)]
        [Register<DashboardPage>(Scope.SingleInstance)]
        [Register<ModsPage>(Scope.SingleInstance)]
        [Register<SettingsPage>(Scope.SingleInstance)]
        internal class ViewsModule
        {
            [Factory(Scope.SingleInstance)]
            public static Uri? CreateInstallRequestUri(string[] args) => args is ["--install", { } uri] ? new Uri(uri) : null;

            [Factory(Scope.SingleInstance)]
            public static Window CreateMainWindow(Uri? installRequestUri, Lazy<MainWindow> mainWindow, Lazy<AssetInstallWindow> assetInstallWindow) => installRequestUri is null ? mainWindow.Value : assetInstallWindow.Value;

            [Factory(Scope.SingleInstance, typeof(IDataTemplate))]
            public static FuncDataTemplate CreateDashboardPageDataTemplate(Lazy<DashboardPage> view) => new(static t => t is DashboardViewModel, (_, _) => view.Value, true);

            [Factory(Scope.SingleInstance, typeof(IDataTemplate))]
            public static FuncDataTemplate CreateModsPageDataTemplate(Lazy<ModsPage> view) => new(static t => t is ModsViewModel, (_, _) => view.Value, true);

            [Factory(Scope.SingleInstance, typeof(IDataTemplate))]
            public static FuncDataTemplate CreateDashboardPageDataTemplate(Lazy<SettingsPage> view) => new(static t => t is SettingsViewModel, (_, _) => view.Value, true);
        }

        internal const string Version = "0.0.5";

        internal const string Product = nameof(BeatSaberModManager);

        internal static bool IsProduction =>
#if DEBUG
            false;
#else
            true;
#endif
    }
}
