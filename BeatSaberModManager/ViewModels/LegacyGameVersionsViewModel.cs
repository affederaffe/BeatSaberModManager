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
        private readonly IInstallDirValidator _installDirValidator;
        private readonly ILegacyGameVersionProvider _legacyGameVersionProvider;
        private readonly ILegacyGameVersionInstaller _legacyGameVersionInstaller;
        private readonly ObservableAsPropertyHelper<bool> _isExecuting;
        private readonly ObservableAsPropertyHelper<bool> _isSuccess;
        private readonly ObservableAsPropertyHelper<bool> _canInstall;
        private readonly ObservableAsPropertyHelper<bool> _canUninstall;
        private readonly ObservableAsPropertyHelper<LegacyGameVersionsTab[]> _legacyGameVersions;

        /// <summary>
        /// TODO
        /// </summary>
        public LegacyGameVersionsViewModel(SettingsViewModel settingsViewModel, SteamAuthenticationViewModel steamAuthenticationViewModel, IGameInstallLocator gameInstallLocator, IInstallDirValidator installDirValidator, ILegacyGameVersionProvider legacyGameVersionProvider, ILegacyGameVersionInstaller legacyGameVersionInstaller, StatusProgress statusProgress)
        {
            ArgumentNullException.ThrowIfNull(settingsViewModel);
            _gameInstallLocator = gameInstallLocator;
            _installDirValidator = installDirValidator;
            _legacyGameVersionProvider = legacyGameVersionProvider;
            _legacyGameVersionInstaller = legacyGameVersionInstaller;
            SteamAuthenticationViewModel = steamAuthenticationViewModel;
            StatusProgress = statusProgress;
            InitializeCommand = ReactiveCommand.CreateFromTask(GetLegacyGameVersionsAsync);
            InitializeCommand.IsExecuting.ToProperty(this, nameof(IsExecuting), out _isExecuting);
            InitializeCommand.CombineLatest(InitializeCommand.IsExecuting)
                .Select(static x => x is (not null, false))
                .ToProperty(this, nameof(IsSuccess), out _isSuccess);
            IObservable<LegacyGameVersionsTab[]> legacyGameVersionTabs = InitializeCommand.WhereNotNull()
                .Select(gameVersions => gameVersions
                    .OrderByDescending(static version => version.GameVersion.ReleaseDate)
                    .GroupBy(static version => version.GameVersion.ReleaseDate.Year)
                    .Select(group => new LegacyGameVersionsTab(group, this))
                    .ToArray());
            legacyGameVersionTabs.ToProperty(this, nameof(LegacyGameVersions), out _legacyGameVersions);
            settingsViewModel.ValidatedGameVersionObservable
                .FirstAsync()
                .CombineLatest(legacyGameVersionTabs)
                .Subscribe(x => MarkStoreVersionAsInstalled(x.First, x.Second));
            IObservable<(GameVersionViewModel? GameVersion, bool IsInstalled)> whenAnySelectedGameVersion = this.WhenAnyValue(static x => x.SelectedGameVersion, static x => x.SelectedGameVersion!.IsInstalled, static (gameVersion, _) => (gameVersion, gameVersion?.IsInstalled ?? false));
            IObservable<bool> canInstallVersion = whenAnySelectedGameVersion.Select(static x => x.GameVersion is not null && !x.IsInstalled);
            canInstallVersion.ToProperty(this, nameof(CanInstall), out _canInstall);
            InstallCommand = ReactiveCommand.CreateFromTask(InstallSelectedLegacyGameVersionAsync, canInstallVersion);
            IObservable<bool> canUninstallVersion = whenAnySelectedGameVersion.Select(static version => version is { GameVersion.InstallDir: not null });
            canUninstallVersion.ToProperty(this, nameof(CanUninstall), out _canUninstall);
            UninstallCommand = ReactiveCommand.CreateFromTask(UninstallSelectedLegacyGameVersionAsync, canUninstallVersion);
            IObservable<bool> canViewMoreInfo = whenAnySelectedGameVersion.Select(static x => x.GameVersion?.GameVersion.ReleaseUrl is not null);
            MoreInfoCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(SelectedGameVersion!.GameVersion.ReleaseUrl!), canViewMoreInfo);
            whenAnySelectedGameVersion.Where(static version => version is { GameVersion.InstallDir: not null })
                .Subscribe(x => settingsViewModel.GameVersion = x.GameVersion!.GameVersion);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public SteamAuthenticationViewModel SteamAuthenticationViewModel { get; }

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
        /// 
        /// </summary>
        public bool CanInstall => _canInstall.Value;

        /// <summary>
        /// 
        /// </summary>
        public bool CanUninstall => _canUninstall.Value;

        /// <summary>
        /// TODO
        /// </summary>
        public IReadOnlyList<LegacyGameVersionsTab> LegacyGameVersions => _legacyGameVersions.Value;

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
            IReadOnlyList<IGameVersion>? installedLegacyGameVersions = await _legacyGameVersionProvider.GetInstalledLegacyGameVersionsAsync().ConfigureAwait(false);
            IGameVersion? installedStoreVersion = await _gameInstallLocator.LocateGameInstallAsync().ConfigureAwait(false);
            List<IGameVersion> allInstalledGameVersions = installedLegacyGameVersions?.ToList() ?? [];
            if (installedStoreVersion is not null)
                allInstalledGameVersions.Insert(0, installedStoreVersion);

            if (availableGameVersions is not null && allInstalledGameVersions.Count == 0)
                return availableGameVersions.Select(gameVersion => new GameVersionViewModel(gameVersion, _installDirValidator)).ToArray();

            if (availableGameVersions is null)
                return allInstalledGameVersions.Select(gameVersion => new GameVersionViewModel(gameVersion, _installDirValidator)).ToArray();

            foreach (IGameVersion gameVersion in availableGameVersions)
                gameVersion.InstallDir = allInstalledGameVersions.FirstOrDefault(x => x.GameVersion == gameVersion.GameVersion)?.InstallDir;

            return availableGameVersions.Select(gameVersion => new GameVersionViewModel(gameVersion, _installDirValidator)).ToArray();
        }

        private async Task<bool> InstallSelectedLegacyGameVersionAsync()
        {
            GameVersionViewModel selectedGameVersion = SelectedGameVersion!;
            StatusProgress.Report(new ProgressInfo(StatusType.Installing, selectedGameVersion.GameVersion.GameVersion));
            using CancellationTokenSource cts = new();
            using IDisposable subscription = SteamAuthenticationViewModel.CancelCommand.Subscribe(_ => cts.Cancel());
            string? installDir = await _legacyGameVersionInstaller.InstallLegacyGameVersionAsync(selectedGameVersion.GameVersion, cts.Token, StatusProgress).ConfigureAwait(false);
            if (installDir is null)
            {
                StatusProgress.Report(new ProgressInfo(StatusType.Failed, null));
                return false;
            }

            selectedGameVersion.InstallDir = installDir;
            StatusProgress.Report(new ProgressInfo(StatusType.Completed, null));
            return true;
        }

        private async Task<bool> UninstallSelectedLegacyGameVersionAsync()
        {
            GameVersionViewModel selectedGameVersion = SelectedGameVersion!;
            bool success = await _legacyGameVersionInstaller.UninstallLegacyGameVersionAsync(selectedGameVersion.GameVersion).ConfigureAwait(false);
            if (success)
                selectedGameVersion.InstallDir = null;
            return success;
        }

        private void MarkStoreVersionAsInstalled(IGameVersion storeVersion, IEnumerable<LegacyGameVersionsTab> tabs)
        {
            foreach (LegacyGameVersionsTab tab in tabs)
            {
                GameVersionViewModel? storeInstalledVersion = tab.Versions.FirstOrDefault(version => version.GameVersion.GameVersion == storeVersion.GameVersion);
                if (storeInstalledVersion is null)
                    continue;

                storeInstalledVersion.InstallDir = storeVersion.InstallDir;
                storeInstalledVersion.GameVersion.InstallDir = storeVersion.InstallDir;
                SelectedGameVersion = storeInstalledVersion;
                break;
            }
        }
    }
}
