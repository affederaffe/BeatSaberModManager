using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Services.Implementations.BeatSaber.Playlists;
using BeatSaberModManager.Services.Implementations.Observables;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="BeatSaberModManager.Views.Pages.DashboardPage"/>.
    /// </summary>
    public sealed class DashboardViewModel : ViewModelBase, IDisposable
    {
        private readonly SettingsViewModel _settingsViewModel;
        private readonly PlaylistInstaller _playlistInstaller;
        private readonly ObservableAsPropertyHelper<string?> _gameVersion;
        private readonly DirectoryExistsObservable _appDataDirExistsObservable;
        private readonly DirectoryExistsObservable _logsDirExistsObservable;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
        /// </summary>
        public DashboardViewModel(ModsViewModel modsViewModel, SettingsViewModel settingsViewModel, IGameLauncher gameLauncher, IGamePathsProvider gamePathsProvider, StatusProgress statusProgress, PlaylistInstaller playlistInstaller)
        {
            ArgumentNullException.ThrowIfNull(modsViewModel);
            ArgumentNullException.ThrowIfNull(settingsViewModel);
            ArgumentNullException.ThrowIfNull(gamePathsProvider);
            _settingsViewModel = settingsViewModel;
            ModsViewModel = modsViewModel;
            StatusProgress = statusProgress;
            _playlistInstaller = playlistInstaller;
            PickPlaylistInteraction = new Interaction<Unit, string?>();
            settingsViewModel.ValidatedGameVersionObservable.Select(static gameVersion => gameVersion.GameVersion)
                .ToProperty(this, nameof(GameVersion), out _gameVersion);
            _appDataDirExistsObservable = new DirectoryExistsObservable();
            settingsViewModel.ValidatedGameVersionObservable.Select(gamePathsProvider.GetAppDataPath).Subscribe(x => _appDataDirExistsObservable.Path = x);
            OpenAppDataCommand = ReactiveCommand.Create<Unit, bool>(_ => PlatformUtils.TryOpenUri(new Uri(_appDataDirExistsObservable.Path!)), _appDataDirExistsObservable.ObserveOn(RxApp.MainThreadScheduler));
            _logsDirExistsObservable = new DirectoryExistsObservable();
            settingsViewModel.ValidatedGameVersionObservable.Select(gamePathsProvider.GetLogsPath).Subscribe(x => _logsDirExistsObservable.Path = x);
            OpenLogsCommand = ReactiveCommand.Create<Unit, bool>(_ => PlatformUtils.TryOpenUri(new Uri(_logsDirExistsObservable.Path!)), _logsDirExistsObservable.ObserveOn(RxApp.MainThreadScheduler));
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(() => modsViewModel.UninstallModLoaderAsync(settingsViewModel.GameVersion!, statusProgress), modsViewModel.IsSuccessObservable);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(() => modsViewModel.UninstallAllModsAsync(settingsViewModel.GameVersion!, statusProgress), modsViewModel.IsSuccessObservable);
            LaunchGameCommand = ReactiveCommand.Create(() => gameLauncher.LaunchGame(settingsViewModel.GameVersion!), settingsViewModel.IsGameVersionValidObservable);
            InstallPlaylistCommand = ReactiveCommand.CreateFromObservable(() => PickPlaylistInteraction.Handle(Unit.Default)
                .WhereNotNull()
                .SelectMany(InstallPlaylistAsync), settingsViewModel.IsGameVersionValidObservable);
        }

        private async Task<bool> InstallPlaylistAsync(string x)
        {
            bool result = await _playlistInstaller.InstallPlaylistAsync(_settingsViewModel.GameVersion!.InstallDir!, x, StatusProgress).ConfigureAwait(false);
            StatusProgress.Report(new ProgressInfo(result ? StatusType.Completed : StatusType.Failed, null));
            return result;
        }

        /// <summary>
        /// The version of the application.
        /// </summary>
        public static string AppVersion => ThisAssembly.Info.Version;

        /// <summary>
        /// The ModsViewModel.
        /// </summary>
        public ModsViewModel ModsViewModel { get; }

        /// <summary>
        /// TODO
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
        /// Install a see cref="BeatSaberModManager.Models.Implementations.BeatSaber.Playlists.Playlist"/>.
        /// </summary>
        public ReactiveCommand<Unit, bool> InstallPlaylistCommand { get; }

        /// <summary>
        /// Asks the user to select a playlist file to install.
        /// </summary>
        public Interaction<Unit, string?> PickPlaylistInteraction { get; }

        /// <summary>
        /// The version of the game.
        /// </summary>
        public string? GameVersion => _gameVersion.Value;

        /// <inheritdoc />
        public void Dispose()
        {
            _appDataDirExistsObservable.Dispose();
            _logsDirExistsObservable.Dispose();
            OpenAppDataCommand.Dispose();
            OpenLogsCommand.Dispose();
            UninstallModLoaderCommand.Dispose();
            UninstallAllModsCommand.Dispose();
            LaunchGameCommand.Dispose();
            InstallPlaylistCommand.Dispose();
        }
    }
}
