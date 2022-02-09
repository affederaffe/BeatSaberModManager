using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Services.Implementations.BeatSaber.Playlists;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;

using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="BeatSaberModManager.Views.Pages.DashboardPage"/>.
    /// </summary>
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly PlaylistInstaller _playlistInstaller;
        private readonly ObservableAsPropertyHelper<string?> _gameVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
        /// </summary>
        public DashboardViewModel(ModsViewModel modsViewModel, IOptions<AppSettings> appSettings, IGameVersionProvider gameVersionProvider, IGameLauncher gameLauncher, IAppDataPathProvider appDataPathProvider, IStatusProgress statusProgress, PlaylistInstaller playlistInstaller)
        {
            _appSettings = appSettings;
            _playlistInstaller = playlistInstaller;
            AppVersion = ThisAssembly.Info.Version;
            ModsViewModel = modsViewModel;
            StatusProgress = (StatusProgress)statusProgress;
            modsViewModel.ValidatedInstallDirObservable.SelectMany(gameVersionProvider.DetectGameVersionAsync).ToProperty(this, nameof(GameVersion), out _gameVersion);
            OpenAppDataCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(appDataPathProvider.GetAppDataPath(appSettings.Value.InstallDir!)), modsViewModel.ValidatedInstallDirObservable.Select(appDataPathProvider.GetAppDataPath).Select(Directory.Exists));
            OpenLogsCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(Path.Combine(appSettings.Value.InstallDir!, "Logs")), modsViewModel.IsInstallDirValidObservable);
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
        /// <returns>true if the operation succeeds, false otherwise.</returns>
        public Task<bool> InstallPlaylistAsync(string path) =>
            _playlistInstaller.InstallPlaylistAsync(_appSettings.Value.InstallDir!, path, StatusProgress);
    }
}