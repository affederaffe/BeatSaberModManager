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
    public class Startup
    {
        private readonly IServiceProvider _services;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ILogger<Startup> _logger;
        private readonly IUpdater _updater;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IInstallDirLocator _installDirLocator;
        private readonly Action<ILogger, Exception> _crash;

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

        public async Task<int> RunAsync()
        {
#if DEBUG
            if (_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir) && _appSettings.Value.PlatformType is not null) return RunAvaloniaApp();
            (string? installDir, string? platform) = await _installDirLocator.LocateInstallDirAsync().ConfigureAwait(false);
            _appSettings.Value.PlatformType = platform;
            _appSettings.Value.InstallDir = installDir;
            return RunAvaloniaApp();
#else
            try
            {
                if (await _updater.NeedsUpdate().ConfigureAwait(false)) return await _updater.Update().ConfigureAwait(false);
                if (_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir) && _appSettings.Value.PlatformType is not null) return RunAvaloniaApp();
                (string? installDir, string? platform) = await _installDirLocator.LocateInstallDirAsync().ConfigureAwait(false);
                _appSettings.Value.PlatformType = platform;
                _appSettings.Value.InstallDir = installDir;
                return RunAvaloniaApp();
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
                .With(new Win32PlatformOptions { AllowEglInitialization = true, UseWindowsUIComposition = true })
                .With(new X11PlatformOptions { UseEGL = true })
                .With(new FontManagerOptions { DefaultFamilyName = string.IsNullOrEmpty(SKTypeface.Default.FamilyName) ? SKFontManager.Default.GetFamilyName(0) : SKTypeface.Default.FamilyName })
                .UsePlatformDetect()
                .UseReactiveUI()
                .StartWithClassicDesktopLifetime(null!);
    }
}