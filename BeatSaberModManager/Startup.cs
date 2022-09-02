using System;
using System.Threading.Tasks;

using Avalonia;
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
        private readonly Lazy<Application> _application;
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IInstallDirLocator _installDirLocator;
#if RELEASE
        private readonly IUpdater _updater;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
#if RELEASE
        public Startup(Lazy<Application> application, ISettings<AppSettings> appSettings, IUpdater updater, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _application = application;
            _appSettings = appSettings;
            _updater = updater;
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
        }
#else
        public Startup(Lazy<Application> application, ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator,
            IInstallDirLocator installDirLocator)
        {
            _application = application;
            _appSettings = appSettings;
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
        }
#endif

        /// <summary>
        /// Asynchronously starts the application.
        /// </summary>
        public async Task<int> RunAsync()
        {
#if RELEASE
            if (await _updater.NeedsUpdateAsync().ConfigureAwait(false))
                return await _updater.UpdateAsync().ConfigureAwait(false);
#endif
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir))
                _appSettings.Value.InstallDir = await _installDirLocator.LocateInstallDirAsync().ConfigureAwait(false);
            return RunAvaloniaApp();
        }

        private int RunAvaloniaApp() =>
            AppBuilder.Configure(() => _application.Value)
                .UsePlatformDetect()
                .UseReactiveUI()
                .StartWithClassicDesktopLifetime(null!);
    }
}
