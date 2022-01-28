using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
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

        public DashboardViewModel(ModsViewModel modsViewModel, IOptions<AppSettings> appSettings, AssemblyName assemblyName, IInstallDirValidator installDirValidator, IGameVersionProvider gameVersionProvider, IGameLauncher gameLauncher, IStatusProgress statusProgress, PlaylistInstaller playlistInstaller)
        {
            _appSettings = appSettings;
            _playlistInstaller = playlistInstaller;
            AppVersion = assemblyName.Version!.ToString(3);
            ModsViewModel = modsViewModel;
            StatusProgress = (StatusProgress)statusProgress;
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync);
            LaunchGameCommand = ReactiveCommand.Create(() => gameLauncher.LaunchGame(appSettings.Value.InstallDir!, appSettings.Value.PlatformType!), appSettings.Value.WhenAnyValue(static x => x.InstallDir).Select(installDirValidator.ValidateInstallDir));
            appSettings.Value.WhenAnyValue(static x => x.InstallDir)
                .Where(installDirValidator.ValidateInstallDir)
                .SelectMany(gameVersionProvider.DetectGameVersionAsync!)
                .ToProperty(this, nameof(GameVersion), out _gameVersion);
            modsViewModel.WhenAnyValue(static x => x.GridItems)
                .WhereNotNull()
                .SelectMany(x => x.Values)
                .SelectMany(InitializeInstalledModsCount)
                .Subscribe(x => InstalledMods += x is null ? -1 : 1);
        }

        public string AppVersion { get; }

        public ModsViewModel ModsViewModel { get; }

        public StatusProgress StatusProgress { get; }

        public ReactiveCommand<Unit, Unit> UninstallModLoaderCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallAllModsCommand { get; }

        public ReactiveCommand<Unit, Unit> LaunchGameCommand { get; }

        public string? GameVersion => _gameVersion.Value;

        private int _installedMods;
        public int InstalledMods
        {
            get => _installedMods;
            set => this.RaiseAndSetIfChanged(ref _installedMods, value);
        }

        public Task<bool> InstallPlaylistAsync(string path) =>
            _playlistInstaller.InstallPlaylistAsync(_appSettings.Value.InstallDir!, path, StatusProgress);

        private IObservable<IMod?> InitializeInstalledModsCount(ModGridItemViewModel x)
        {
            IObservable<IMod?> whenAnyValue = x.WhenAnyValue(y => y.InstalledMod);
            whenAnyValue.FirstOrDefaultAsync().Subscribe(y => InstalledMods += y is null ? 0 : 1);
            return whenAnyValue.Skip(1);
        }
    }
}