using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// TODO
    /// </summary>
    public class LegacyGameVersionsViewModel : ViewModelBase
    {
        private readonly ILegacyGameVersionProvider _legacyGameVersionProvider;
        private readonly ILegacyGameVersionInstaller _legacyGameVersionInstaller;
        private readonly ObservableAsPropertyHelper<bool> _isExecuting;
        private readonly ObservableAsPropertyHelper<bool> _isSuccess;
        private readonly ObservableAsPropertyHelper<IReadOnlyList<IGrouping<int, GameVersionViewModel>>> _legacyGameVersions;

        /// <summary>
        /// TODO
        /// </summary>
        public LegacyGameVersionsViewModel(SettingsViewModel settingsViewModel, DashboardViewModel dashboardViewModel, ILegacyGameVersionProvider legacyGameVersionProvider, ILegacyGameVersionInstaller legacyGameVersionInstaller, StatusProgress statusProgress)
        {
            _legacyGameVersionProvider = legacyGameVersionProvider;
            _legacyGameVersionInstaller = legacyGameVersionInstaller;
            StatusProgress = statusProgress;
            InitializeCommand = ReactiveCommand.CreateFromTask(GetLegacyGameVersionsAsync);
            InitializeCommand.IsExecuting.ToProperty(this, nameof(IsExecuting), out _isExecuting);
            InitializeCommand.CombineLatest(InitializeCommand.IsExecuting)
                .Select(static x => x is (not null, false))
                .ToProperty(this, nameof(IsSuccess), out _isSuccess);
            IObservable<GameVersionViewModel[]> legacyGameVersions = InitializeCommand.WhereNotNull()
                .Select(static x => x.ToArray());
            legacyGameVersions.CombineLatest(dashboardViewModel.WhenAnyValue(static x => x.GameVersion))
                .FirstAsync()
                .Select(static x => x.First.FirstOrDefault(gameVersion => gameVersion.LegacyGameVersion.GameVersion == x.Second))
                .WhereNotNull()
                .Do(currentGameVersion => currentGameVersion.InstallDir = settingsViewModel.InstallDir)
                .Subscribe(currentGameVersion => SelectedLegacyGameVersion = currentGameVersion);
            legacyGameVersions
                .Select(static gameVersions => gameVersions
                    .OrderByDescending(static x => x.LegacyGameVersion.ReleaseDate)
                    .GroupBy(static version => version.LegacyGameVersion.ReleaseDate.Year)
                    .ToArray())
                .ToProperty(this, nameof(LegacyGameVersions), out _legacyGameVersions);
            IObservable<GameVersionViewModel?> whenAnySelectedLegacyGameVersion = this.WhenAnyValue(static x => x.SelectedLegacyGameVersion);
            IObservable<bool> canInstallVersion = whenAnySelectedLegacyGameVersion.Select(static version => version is not null && !version.IsInstalled);
            InstallCommand = ReactiveCommand.CreateFromTask(InstallSelectedLegacyGameVersionAsync, canInstallVersion);
            IObservable<bool> canUninstallVersion = whenAnySelectedLegacyGameVersion.Select(static version => version is not null && version.IsInstalled);
            UninstallCommand = ReactiveCommand.CreateFromTask(UninstallSelectedLegacyGameVersionAsync, canUninstallVersion);
            IObservable<bool> canViewMoreInfo = whenAnySelectedLegacyGameVersion.Select(static version => version?.LegacyGameVersion.ReleaseUrl is not null);
            MoreInfoCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(SelectedLegacyGameVersion!.LegacyGameVersion.ReleaseUrl!), canViewMoreInfo);
            whenAnySelectedLegacyGameVersion.Where(static version => version is not null && version.IsInstalled)
                .Subscribe(version => settingsViewModel.InstallDir = version!.InstallDir);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ReactiveCommand<Unit, IReadOnlyList<GameVersionViewModel>?> InitializeCommand { get; }

        /// <summary>
        /// Installs the selected game version.
        /// </summary>
        public ReactiveCommand<Unit, bool> InstallCommand { get; }

        /// <summary>
        /// Uninstalls the selected game version.
        /// </summary>
        public ReactiveCommand<Unit, bool> UninstallCommand { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public ReactiveCommand<Unit, bool> MoreInfoCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public StatusProgress StatusProgress { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsExecuting => _isExecuting.Value;

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsSuccess => _isSuccess.Value;

        /// <summary>
        /// TODO
        /// </summary>
        public IReadOnlyList<IGrouping<int, GameVersionViewModel>> LegacyGameVersions => _legacyGameVersions.Value;

        /// <summary>
        /// TODO
        /// </summary>
        public GameVersionViewModel? SelectedLegacyGameVersion
        {
            get => _selectedLegacyGameVersion;
            set
            {
                if (value is not null)
                    this.RaiseAndSetIfChanged(ref _selectedLegacyGameVersion, value);
            }
        }

        private GameVersionViewModel? _selectedLegacyGameVersion;

        private async Task<IReadOnlyList<GameVersionViewModel>?> GetLegacyGameVersionsAsync()
        {
            IReadOnlyList<ILegacyGameVersion>? availableGameVersions = await _legacyGameVersionProvider.GetAvailableGameVersionsAsync().ConfigureAwait(true);
            if (availableGameVersions is null)
                return null;
            GameVersionViewModel[] gameVersionViewModels = availableGameVersions.Select(static x => new GameVersionViewModel(x)).ToArray();
            IReadOnlyList<(ILegacyGameVersion GameVersion, string InstallDir)>? installedGameVersions = await _legacyGameVersionProvider.GetInstalledLegacyGameVersionsAsync().ConfigureAwait(true);
            if (installedGameVersions is null)
                return gameVersionViewModels;
            foreach (GameVersionViewModel gameVersionViewModel in gameVersionViewModels)
                gameVersionViewModel.InstallDir = installedGameVersions.FirstOrDefault(x => x.GameVersion == gameVersionViewModel.LegacyGameVersion).InstallDir;
            return gameVersionViewModels;
        }

        private async Task<bool> InstallSelectedLegacyGameVersionAsync()
        {
            GameVersionViewModel selectedGameVersion = SelectedLegacyGameVersion!;
            string? installDir = await _legacyGameVersionInstaller.InstallLegacyGameVersionAsync(selectedGameVersion.LegacyGameVersion, CancellationToken.None, StatusProgress).ConfigureAwait(true);
            selectedGameVersion.InstallDir = installDir;
            return installDir is not null;
        }

        private async Task<bool> UninstallSelectedLegacyGameVersionAsync()
        {
            GameVersionViewModel selectedGameVersion = SelectedLegacyGameVersion!;
            bool success = await _legacyGameVersionInstaller.UninstallLegacyGameVersionAsync(selectedGameVersion.LegacyGameVersion).ConfigureAwait(true);
            if (success)
                selectedGameVersion.InstallDir = null;
            return success;
        }
    }
}
