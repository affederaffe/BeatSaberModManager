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
        private readonly string[] _args;
        private readonly Lazy<Application> _application;
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IInstallDirLocator _installDirLocator;
        private readonly IUpdater _updater;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>

        public Startup(string[] args, Lazy<Application> application, ISettings<AppSettings> appSettings, IUpdater updater, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _args = args;
            _application = application;
            _appSettings = appSettings;
            _updater = updater;
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
        }

        /// <summary>
        /// Asynchronously starts the application.
        /// </summary>
        public async Task<int> RunAsync()
        {
            if (await _updater.NeedsUpdateAsync().ConfigureAwait(false))
                return await _updater.UpdateAsync().ConfigureAwait(false);
            await _appSettings.LoadAsync();
            if (_args.Length == 2 && _args[0] == "--path")
                _appSettings.Value.InstallDir = _args[1];
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
