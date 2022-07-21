using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.BeatSaber.Playlists;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="BeatSaberModManager.Views.Pages.DashboardPage"/>.
    /// </summary>
    public class DashboardViewModel : ViewModelBase
    {
        private readonly ISettings<AppSettings> _appSettings;
        private readonly PlaylistInstaller _playlistInstaller;
        private readonly ObservableAsPropertyHelper<string?> _gameVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
        /// </summary>
        public DashboardViewModel(ModsViewModel modsViewModel, ISettings<AppSettings> appSettings, IGameVersionProvider gameVersionProvider, IGameLauncher gameLauncher, IAppDataPathProvider appDataPathProvider, IStatusProgress statusProgress, PlaylistInstaller playlistInstaller)
        {
            _appSettings = appSettings;
            _playlistInstaller = playlistInstaller;
            AppVersion = ThisAssembly.Info.Version;
            ModsViewModel = modsViewModel;
            StatusProgress = (StatusProgress)statusProgress;
            modsViewModel.ValidatedInstallDirObservable.SelectMany(gameVersionProvider.DetectGameVersionAsync).ToProperty(this, nameof(GameVersion), out _gameVersion);
            IObservable<string> validatedAppDataObservable = modsViewModel.ValidatedInstallDirObservable.Select(appDataPathProvider.GetAppDataPath);
            OpenAppDataCommand = ReactiveCommand.CreateFromObservable<Unit, bool>(_ => validatedAppDataObservable.Select(static x => PlatformUtils.TryOpenUri(x)), validatedAppDataObservable.Select(Directory.Exists));
            IObservable<string> validatedLogsDirObservable = modsViewModel.ValidatedInstallDirObservable.Select(static x => Path.Join(x, "Logs"));
            OpenLogsCommand = ReactiveCommand.CreateFromObservable<Unit, bool>(_ => validatedLogsDirObservable.Select(static x => PlatformUtils.TryOpenUri(x)), validatedLogsDirObservable.Select(Directory.Exists));
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync, modsViewModel.IsSuccessObservable);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync, modsViewModel.IsSuccessObservable);
            LaunchGameCommand = ReactiveCommand.Create(() => gameLauncher.LaunchGame(appSettings.Value.InstallDir!), modsViewModel.IsInstallDirValidObservable);
        }

        /// <summary>
        /// The version of the application.
        /// </summary>
        public string AppVersion { get; }

        /// <summary>
        /// Exposed for the view.
        /// </summary>
        public ModsViewModel ModsViewModel { get; }

        /// <summary>
        /// Exposed for the view.
        /// </summary>
        public StatusProgress StatusProgress { get; }

        /// <summary>
        /// Opens the game's AppData directory in the file explorer.
        /// </summary>
        public ReactiveCommand<Unit, bool> OpenAppDataCommand { get; }

        /// <summary>
        /// Opens the game's Logs directory in the file explorer.
        /// </summary>
        public ReactiveCommand<Unit, bool> OpenLogsCommand { get; }

        /// <summary>
        /// Uninstalls the mod loader.
        /// </summary>
        public ReactiveCommand<Unit, Unit> UninstallModLoaderCommand { get; }

        /// <summary>
        /// Uninstalls all installed mods.
        /// </summary>
        public ReactiveCommand<Unit, Unit> UninstallAllModsCommand { get; }

        /// <summary>
        /// Launches the game.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LaunchGameCommand { get; }

        /// <summary>
        /// The version of the game.
        /// </summary>
        public string? GameVersion => _gameVersion.Value;

        /// <summary>
        /// Asynchronously installs a <see cref="BeatSaberModManager.Models.Implementations.BeatSaber.Playlists.Playlist"/>.
        /// </summary>
        /// <param name="path">The path of the <see cref="BeatSaberModManager.Models.Implementations.BeatSaber.Playlists.Playlist"/>'s file.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        public Task<bool> InstallPlaylistAsync(string path) => _playlistInstaller.InstallPlaylistAsync(_appSettings.Value.InstallDir!, path, StatusProgress);
    }
}
