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
    /// Handles the applications start, e.g. updating and configuring Avalonia.
    /// </summary>
    public class Startup
    {
        private readonly IResolver _resolver;
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IUpdater _updater;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IInstallDirLocator _installDirLocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        public Startup(IResolver resolver, ISettings<AppSettings> appSettings, IUpdater updater, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _resolver = resolver;
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
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir))
                _appSettings.Value.InstallDir = await _installDirLocator.LocateInstallDirAsync().ConfigureAwait(false);
#if !DEBUG
            if (await _updater.NeedsUpdate().ConfigureAwait(false))
                return await _updater.Update().ConfigureAwait(false);
#endif
            return RunAvaloniaApp();
        }

        private int RunAvaloniaApp() =>
            AppBuilder.Configure(() => _resolver.Resolve<Application>())
                .With(new Win32PlatformOptions { UseWindowsUIComposition = true })
                .With(new X11PlatformOptions { UseEGL = true })
                .With(new FontManagerOptions { DefaultFamilyName = string.IsNullOrEmpty(SKTypeface.Default.FamilyName) ? SKFontManager.Default.GetFamilyName(0) : SKTypeface.Default.FamilyName })
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToTrace()
                .StartWithClassicDesktopLifetime(null!);
    }
}