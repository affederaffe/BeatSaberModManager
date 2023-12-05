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
    public class Startup(string[] args, Lazy<Application> application, ISettings<AppSettings> appSettings, IUpdater updater)
    {
        /// <summary>
        /// Asynchronously starts the application.
        /// </summary>
        public async Task<int> RunAsync()
        {
            if (await updater.NeedsUpdateAsync().ConfigureAwait(false))
                return await updater.UpdateAsync().ConfigureAwait(false);
            await appSettings.LoadAsync().ConfigureAwait(false);
            if (args is ["--path", { } installDir])
                appSettings.Value.InstallDir = installDir;
            return RunAvaloniaApp();
        }

        private int RunAvaloniaApp() => BuildAvaloniaApp().StartWithClassicDesktopLifetime(null!);

        private AppBuilder BuildAvaloniaApp()
        {
            AppBuilder appBuilder = AppBuilder.Configure(() => application.Value)
                .UsePlatformDetect()
                .UseReactiveUI()
                .WithInterFont();
            if (OperatingSystem.IsLinux())
                appBuilder = appBuilder.With(new FontManagerOptions { DefaultFamilyName = "Noto Sans" });
            return appBuilder;
        }
    }
}
