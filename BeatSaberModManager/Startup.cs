using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Media;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Services.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SkiaSharp;


namespace BeatSaberModManager
{
    /// <summary>
    /// Handles the applications start, e.g. updating, logging or UI.
    /// </summary>
    public class Startup
    {
        private readonly IServiceProvider _services;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ILogger<Startup> _logger;
        private readonly IUpdater _updater;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IInstallDirLocator _installDirLocator;
        private readonly Action<ILogger, Exception> _crash;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        public Startup(IServiceProvider services, IOptions<AppSettings> appSettings, ILogger<Startup> logger, IUpdater updater, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _services = services;
            _appSettings = appSettings;
            _logger = logger;
            _updater = updater;
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
            _crash = LoggerMessage.Define(LogLevel.Critical, new EventId(-1, "Crash"), string.Empty);
        }

        /// <summary>
        /// Asynchronously starts the application.
        /// </summary>
        /// <returns>1 when the application gracefully exits<br/>
        /// -1 when an <see cref="Exception"/> occurs</returns>
        /// <exception cref="InvalidOperationException">The app was started in the game's installation directory</exception>
        public async Task<int> RunAsync()
        {
#if DEBUG
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir)) _appSettings.Value.InstallDir = await _installDirLocator.LocateInstallDirAsync().ConfigureAwait(false);
            return Environment.CurrentDirectory == _appSettings.Value.InstallDir
                    ? throw new InvalidOperationException("Application cannot be executed in the game's installation directory")
                    : RunAvaloniaApp();
#else
            try
            {
                if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir)) _appSettings.Value.InstallDir = await _installDirLocator.LocateInstallDirAsync().ConfigureAwait(false);
                return Environment.CurrentDirectory == _appSettings.Value.InstallDir
                    ? throw new InvalidOperationException("Application cannot be executed in the game's installation directory")
                    : await _updater.NeedsUpdate().ConfigureAwait(false) ? await _updater.Update().ConfigureAwait(false) : RunAvaloniaApp();
            }
            catch (Exception e)
            {
                _crash(_logger, e);
                return -1;
            }
#endif
        }

        private int RunAvaloniaApp() =>
            AppBuilder.Configure(_services.GetRequiredService<Application>)
                .With(new Win32PlatformOptions { UseWindowsUIComposition = true })
                .With(new X11PlatformOptions { UseEGL = true })
                .With(new FontManagerOptions { DefaultFamilyName = string.IsNullOrEmpty(SKTypeface.Default.FamilyName) ? SKFontManager.Default.GetFamilyName(0) : SKTypeface.Default.FamilyName })
                .UsePlatformDetect()
                .UseReactiveUI()
                .StartWithClassicDesktopLifetime(null!);
    }
}