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
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly PlaylistInstaller _playlistInstaller;
        private readonly ObservableAsPropertyHelper<string?> _gameVersion;

        public DashboardViewModel(ModsViewModel modsViewModel, IOptions<AppSettings> appSettings, IInstallDirValidator installDirValidator, IGameVersionProvider gameVersionProvider, IGameLauncher gameLauncher, IStatusProgress statusProgress, PlaylistInstaller playlistInstaller)
        {
            _appSettings = appSettings;
            _playlistInstaller = playlistInstaller;
            AppVersion = ThisAssembly.Info.Version;
            ModsViewModel = modsViewModel;
            StatusProgress = (StatusProgress)statusProgress;
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync);
            LaunchGameCommand = ReactiveCommand.Create(() => gameLauncher.LaunchGame(appSettings.Value.InstallDir!, appSettings.Value.PlatformType!), appSettings.Value.WhenAnyValue(static x => x.InstallDir).Select(installDirValidator.ValidateInstallDir));
            appSettings.Value.WhenAnyValue(static x => x.InstallDir)
                .Where(installDirValidator.ValidateInstallDir)
                .SelectMany(gameVersionProvider.DetectGameVersionAsync!)
                .ToProperty(this, nameof(GameVersion), out _gameVersion);
        }

        public string AppVersion { get; }

        public ModsViewModel ModsViewModel { get; }

        public StatusProgress StatusProgress { get; }

        public ReactiveCommand<Unit, Unit> UninstallModLoaderCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallAllModsCommand { get; }

        public ReactiveCommand<Unit, Unit> LaunchGameCommand { get; }

        public string? GameVersion => _gameVersion.Value;

        public Task<bool> InstallPlaylistAsync(string path) =>
            _playlistInstaller.InstallPlaylistAsync(_appSettings.Value.InstallDir!, path, StatusProgress);
    }
}