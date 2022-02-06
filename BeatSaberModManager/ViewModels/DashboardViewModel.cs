using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Services.Implementations.BeatSaber.Playlists;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;

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
        private readonly ObservableAsPropertyHelper<bool> _isInstallDirValid;
        private readonly ObservableAsPropertyHelper<string?> _gameVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
        /// </summary>
        public DashboardViewModel(ModsViewModel modsViewModel, IOptions<AppSettings> appSettings, IInstallDirValidator installDirValidator, IGameVersionProvider gameVersionProvider, IGameLauncher gameLauncher, IStatusProgress statusProgress, PlaylistInstaller playlistInstaller)
        {
            _appSettings = appSettings;
            _playlistInstaller = playlistInstaller;
            AppVersion = ThisAssembly.Info.Version;
            ModsViewModel = modsViewModel;
            StatusProgress = (StatusProgress)statusProgress;
            appSettings.Value.WhenAnyValue(static x => x.InstallDir)
                .Select(installDirValidator.ValidateInstallDir)
                .ToProperty(this, nameof(IsInstallDirValid), out _isInstallDirValid);
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync, modsViewModel.WhenAnyValue(static x => x.IsSuccess));
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync, modsViewModel.WhenAnyValue(static x => x.IsSuccess));
            LaunchGameCommand = ReactiveCommand.Create(() => gameLauncher.LaunchGame(appSettings.Value.InstallDir!), this.WhenAnyValue(static x => x.IsInstallDirValid));
            appSettings.Value.WhenAnyValue(static x => x.InstallDir)
                .Where(installDirValidator.ValidateInstallDir)
                .SelectMany(gameVersionProvider.DetectGameVersionAsync!)
                .ToProperty(this, nameof(GameVersion), out _gameVersion);
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
        /// True when the game's installation is valid, false otherwise.
        /// </summary>
        public bool IsInstallDirValid => _isInstallDirValid.Value;

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