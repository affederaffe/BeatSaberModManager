using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Media;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager
{
    /// <summary>
    /// Handles the applications start, e.g. updating and configuring Avalonia.
    /// </summary>
    public class Startup
    {
        private readonly string[] _args;
        private readonly Lazy<Application> _application;
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IInstallDirLocator _installDirLocator;
        private readonly IUpdater _updater;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        public Startup(string[] args, Lazy<Application> application, ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator, IUpdater updater)
        {
            _args = args;
            _application = application;
            _appSettings = appSettings;
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
            _updater = updater;
        }

        /// <summary>
        /// Asynchronously starts the application.
        /// </summary>
        public async Task<int> RunAsync()
        {
            if (await _updater.NeedsUpdateAsync().ConfigureAwait(false))
                return await _updater.UpdateAsync().ConfigureAwait(false);
            await _appSettings.LoadAsync().ConfigureAwait(false);
            if (_args is ["--path", { } installDir])
                _appSettings.Value.InstallDir = installDir;
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir))
                _appSettings.Value.InstallDir = await _installDirLocator.LocateInstallDirAsync().ConfigureAwait(false);
            return RunAvaloniaApp();
        }

        private int RunAvaloniaApp() => BuildAvaloniaApp().StartWithClassicDesktopLifetime(null!);

        private AppBuilder BuildAvaloniaApp()
        {
            AppBuilder appBuilder = AppBuilder.Configure(() => _application.Value)
                .UsePlatformDetect()
                .UseReactiveUI()
                .WithInterFont();
            if (OperatingSystem.IsLinux())
                appBuilder = appBuilder.With(new FontManagerOptions { DefaultFamilyName = "Noto Sans" });
            return appBuilder;
        }
    }
}
