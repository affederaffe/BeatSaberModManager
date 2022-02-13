using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;

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

using DryIoc;

using ReactiveUI;

using Serilog;


namespace BeatSaberModManager
{
    /// <summary>
    /// Main application class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        public static async Task<int> Main(string[] args)
        {
            using Container container = CreateContainer(args);
            return await container.Resolve<Startup>().RunAsync();
        }

        private static Container CreateContainer(IReadOnlyList<string> args)
        {
            Container container = new(Rules.Default.With(FactoryMethod.ConstructorWithResolvableArguments).WithDefaultReuse(Reuse.Singleton));
            container.Register<Startup>();
            container.RegisterSerilog();
            container.RegisterSettings();
            container.RegisterHttpClient();
            container.RegisterUpdater();
            container.RegisterGameServices();
            container.RegisterModServices();
            container.RegisterAssetProviders();
            container.RegisterProtocolHandlerRegistrar();
            container.RegisterApplication();
            container.RegisterViewModels();
            container.RegisterViews(args);
            return container;
        }

        private static void RegisterSerilog(this IRegistrator container)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ThisAssembly.Info.Product, "log.txt"), rollingInterval: RollingInterval.Minute).CreateLogger();
            container.Register(Made.Of(static () => Log.Logger), setup: Setup.With(condition: static r => r.Parent.ImplementationType is null));
            container.Register(Made.Of(static () => Log.ForContext(Arg.Index<Type>(0)), static r => r.Parent.ImplementationType), setup: Setup.With(condition: static r => r.Parent.ImplementationType is not null));
        }

        private static void RegisterSettings(this IRegistrator container)
        {
            container.RegisterDelegate<ISettings<AppSettings>>(() => new JsonSettingsProvider<AppSettings>(SettingsJsonSerializerContext.Default.AppSettings));
        }

        private static void RegisterHttpClient(this IRegistrator container)
        {
            container.Register<HttpProgressClient>();
        }

        private static void RegisterGameServices(this IRegistrator container)
        {
            container.Register<IGameVersionProvider, BeatSaberGameVersionProvider>();
            container.Register<IGameLauncher, BeatSaberGameLauncher>();
            container.Register<IInstallDirLocator, BeatSaberInstallDirLocator>();
            container.Register<IInstallDirValidator, BeatSaberInstallDirValidator>();
            container.Register<IAppDataPathProvider, BeatSaberAppDataPathProvider>();
        }

        private static void RegisterProtocolHandlerRegistrar(this IRegistrator container)
        {
            if (OperatingSystem.IsWindows())
                container.Register<IProtocolHandlerRegistrar, WindowsProtocolHandlerRegistrar>();
            else if (OperatingSystem.IsLinux())
                container.Register<IProtocolHandlerRegistrar, LinuxProtocolHandlerRegistrar>();
            else
                throw new PlatformNotSupportedException();
        }

        private static void RegisterModServices(this IRegistrator container)
        {
            container.Register<IHashProvider, MD5HashProvider>();
            container.Register<IDependencyResolver, SimpleDependencyResolver>();
            container.Register<IModProvider, BeatModsModProvider>();
            container.Register<IModInstaller, BeatModsModInstaller>();
        }

        private static void RegisterUpdater(this IRegistrator container)
        {
            container.Register<IUpdater, GitHubUpdater>();
        }

        private static void RegisterAssetProviders(this IRegistrator container)
        {
            container.Register<BeatSaverMapInstaller>();
            container.Register<IAssetProvider, BeatSaverAssetProvider>();
            container.Register<ModelSaberModelInstaller>();
            container.Register<IAssetProvider, ModelSaberAssetProvider>();
            container.Register<PlaylistInstaller>();
            container.Register<IAssetProvider, PlaylistAssetProvider>();
        }

        private static void RegisterViewModels(this IRegistrator container)
        {
            container.Register<MainWindowViewModel>();
            container.Register<DashboardViewModel>();
            container.Register<ModsViewModel>();
            container.Register<SettingsViewModel>();
            container.Register<AssetInstallWindowViewModel>();
        }

        private static void RegisterApplication(this IRegistrator container)
        {
            container.Register<Application, App>();
            container.Register<IStatusProgress, StatusProgress>();
            container.Register<LocalizationManager>();
            container.Register<ThemeManager>();
        }

        private static void RegisterViews(this IRegistrator container, IReadOnlyList<string> args)
        {
            if (args.Count is 2 && args[0] == "--install")
            {
                container.Use(new Uri(args[1]));
                container.Register<Window, AssetInstallWindow>();
            }
            else
            {
                container.Register<Window, MainWindow>();
                container.Register<IViewFor<DashboardViewModel>, DashboardPage>();
                container.Register<IViewFor<ModsViewModel>, ModsPage>();
                container.Register<IViewFor<SettingsViewModel>, SettingsPage>();
            }
        }
    }
}