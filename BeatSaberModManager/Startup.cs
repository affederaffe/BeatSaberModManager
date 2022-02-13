using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Media;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;

using DryIoc;

using SkiaSharp;


namespace BeatSaberModManager
{
    /// <summary>
    /// Handles the applications start, e.g. updating, logging or UI.
    /// </summary>
    public class Startup
    {
        private readonly Container _container;
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IUpdater _updater;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IInstallDirLocator _installDirLocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        public Startup(Container container, ISettings<AppSettings> appSettings, IUpdater updater, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _container = container;
            _appSettings = appSettings;
            _updater = updater;
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
        }

        /// <summary>
        /// Asynchronously starts the application.
        /// </summary>
        /// <exception cref="InvalidOperationException">The app was started in the game's installation directory</exception>
        public async Task<int> RunAsync()
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir))
                _appSettings.Value.InstallDir = await _installDirLocator.LocateInstallDirAsync().ConfigureAwait(false);
            return
#if !DEBUG
                await _updater.NeedsUpdate().ConfigureAwait(false)
                    ? await _updater.Update().ConfigureAwait(false)
                    :
#endif
                RunAvaloniaApp();
        }

        private int RunAvaloniaApp() =>
            AppBuilder.Configure(() => _container.Resolve<Application>())
                .With(new Win32PlatformOptions { UseWindowsUIComposition = true })
                .With(new X11PlatformOptions { UseEGL = true })
                .With(new FontManagerOptions { DefaultFamilyName = string.IsNullOrEmpty(SKTypeface.Default.FamilyName) ? SKFontManager.Default.GetFamilyName(0) : SKTypeface.Default.FamilyName })
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToTrace()
                .StartWithClassicDesktopLifetime(null!);
    }
}