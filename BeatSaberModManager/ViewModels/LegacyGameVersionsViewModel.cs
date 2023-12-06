using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Progress;
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
    public sealed class LegacyGameVersionsViewModel : ViewModelBase
    {
        private readonly IGameInstallLocator _gameInstallLocator;
        private readonly ILegacyGameVersionProvider _legacyGameVersionProvider;
        private readonly ILegacyGameVersionInstaller _legacyGameVersionInstaller;
        private readonly ObservableAsPropertyHelper<bool> _isExecuting;
        private readonly ObservableAsPropertyHelper<bool> _isSuccess;
        private readonly ObservableAsPropertyHelper<IReadOnlyList<IGrouping<int, GameVersionViewModel>>> _legacyGameVersions;

        /// <summary>
        /// TODO
        /// </summary>
        public LegacyGameVersionsViewModel(SettingsViewModel settingsViewModel, IGameInstallLocator gameInstallLocator, ILegacyGameVersionProvider legacyGameVersionProvider, ILegacyGameVersionInstaller legacyGameVersionInstaller, StatusProgress statusProgress)
        {
            ArgumentNullException.ThrowIfNull(settingsViewModel);
            _gameInstallLocator = gameInstallLocator;
            _legacyGameVersionProvider = legacyGameVersionProvider;
            _legacyGameVersionInstaller = legacyGameVersionInstaller;
            StatusProgress = statusProgress;
            InitializeCommand = ReactiveCommand.CreateFromTask(GetLegacyGameVersionsAsync);
            InitializeCommand.IsExecuting.ToProperty(this, nameof(IsExecuting), out _isExecuting);
            InitializeCommand.CombineLatest(InitializeCommand.IsExecuting)
                .Select(static x => x is (not null, false))
                .ToProperty(this, nameof(IsSuccess), out _isSuccess);
            InitializeCommand.WhereNotNull()
                .Select(static gameVersions => gameVersions
                    .OrderByDescending(static x => x.GameVersion.ReleaseDate)
                    .GroupBy(static version => version.GameVersion.ReleaseDate.Year)
                    .ToArray())
                .ToProperty(this, nameof(LegacyGameVersions), out _legacyGameVersions);
            settingsViewModel.IsGameVersionValidObservable.FirstAsync()
                .Where(static isValid => !isValid)
                .CombineLatest(InitializeCommand.WhereNotNull())
                .Select(static x => x.Second.LastOrDefault(static gameVersion => gameVersion.IsInstalled))
                .WhereNotNull()
                .Subscribe(installedVersion => settingsViewModel.GameVersion = installedVersion.GameVersion);
            settingsViewModel.ValidatedGameVersionObservable.FirstAsync()
                .CombineLatest(InitializeCommand.WhereNotNull())
                .Select(static x => x.Second.FirstOrDefault(gameVersion => gameVersion.GameVersion.GameVersion == x.First.GameVersion))
                .Subscribe(gameVersion => SelectedGameVersion = gameVersion);
            IObservable<(GameVersionViewModel? GameVersion, bool IsInstalled)> whenAnySelectedGameVersion = this.WhenAnyValue(static x => x.SelectedGameVersion, static x => x.SelectedGameVersion!.IsInstalled, static (gameVersion, _) => (gameVersion, gameVersion?.IsInstalled ?? false));
            IObservable<bool> canInstallVersion = whenAnySelectedGameVersion.Select(static x => x.GameVersion is not null && !x.IsInstalled);
            InstallCommand = ReactiveCommand.CreateFromTask(InstallSelectedLegacyGameVersionAsync, canInstallVersion);
            IObservable<bool> canUninstallVersion = whenAnySelectedGameVersion.Select(static version => version is { GameVersion.InstallDir: not null });
            UninstallCommand = ReactiveCommand.CreateFromTask(UninstallSelectedLegacyGameVersionAsync, canUninstallVersion);
            IObservable<bool> canViewMoreInfo = whenAnySelectedGameVersion.Select(static x => x.GameVersion?.GameVersion.ReleaseUrl is not null);
            MoreInfoCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(SelectedGameVersion!.GameVersion.ReleaseUrl!), canViewMoreInfo);
            whenAnySelectedGameVersion.Where(static version => version is { GameVersion.InstallDir: not null })
                .Subscribe(x => settingsViewModel.GameVersion = x.GameVersion!.GameVersion);
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
        public GameVersionViewModel? SelectedGameVersion
        {
            get => _selectedGameVersion;
            set
            {
                if (value is not null)
                    this.RaiseAndSetIfChanged(ref _selectedGameVersion, value);
            }
        }

        private GameVersionViewModel? _selectedGameVersion;

        private async Task<IReadOnlyList<GameVersionViewModel>?> GetLegacyGameVersionsAsync()
        {
            IReadOnlyList<IGameVersion>? availableGameVersions = await _legacyGameVersionProvider.GetAvailableGameVersionsAsync().ConfigureAwait(false);
            if (availableGameVersions is null)
                return null;
            IReadOnlyList<IGameVersion>? installedLegacyGameVersions = await _legacyGameVersionProvider.GetInstalledLegacyGameVersionsAsync().ConfigureAwait(false);
            IGameVersion? installedStoreVersion = await _gameInstallLocator.LocateGameInstallAsync().ConfigureAwait(false);
            List<IGameVersion>? allInstalledGameVersions = installedLegacyGameVersions?.ToList();
            if (installedStoreVersion is not null)
                allInstalledGameVersions?.Add(installedStoreVersion);
            if (allInstalledGameVersions is null)
                return availableGameVersions.Select(static gameVersion => new GameVersionViewModel(gameVersion)).ToArray();
            foreach (IGameVersion gameVersion in availableGameVersions)
                gameVersion.InstallDir = allInstalledGameVersions.FirstOrDefault(x => x.GameVersion == gameVersion.GameVersion)?.InstallDir;
            return availableGameVersions.Select(static gameVersion => new GameVersionViewModel(gameVersion)).ToArray();
        }

        private async Task<bool> InstallSelectedLegacyGameVersionAsync()
        {
            GameVersionViewModel selectedGameVersion = SelectedGameVersion!;
            StatusProgress.Report(new ProgressInfo(StatusType.Installing, selectedGameVersion.GameVersion.GameVersion));
            string? installDir = await _legacyGameVersionInstaller.InstallLegacyGameVersionAsync(selectedGameVersion.GameVersion, CancellationToken.None, StatusProgress).ConfigureAwait(false);
            selectedGameVersion.InstallDir = installDir;
            return installDir is not null;
        }

        private async Task<bool> UninstallSelectedLegacyGameVersionAsync()
        {
            GameVersionViewModel selectedGameVersion = SelectedGameVersion!;
            bool success = await _legacyGameVersionInstaller.UninstallLegacyGameVersionAsync(selectedGameVersion.GameVersion).ConfigureAwait(false);
            if (success)
                selectedGameVersion.InstallDir = null;
            return success;
        }
    }
}
