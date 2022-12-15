using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="BeatSaberModManager.Views.Pages.ModsPage"/>.
    /// </summary>
    public class ModsViewModel : ViewModelBase
    {
        private readonly ISettings<AppSettings> _appSettings;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IModProvider _modProvider;
        private readonly IModInstaller _modInstaller;
        private readonly IStatusProgress _progress;
        private readonly object _initializeSyncLock;
        private readonly ObservableAsPropertyHelper<Dictionary<IMod, ModGridItemViewModel>?> _gridItems;
        private readonly ObservableAsPropertyHelper<bool> _isExecuting;
        private readonly ObservableAsPropertyHelper<bool> _isSuccess;
        private readonly ObservableAsPropertyHelper<bool> _isEmpty;
        private readonly ObservableAsPropertyHelper<bool> _isFailed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModsViewModel"/> class.
        /// </summary>
        public ModsViewModel(ISettings<AppSettings> appSettings, SettingsViewModel settingsViewModel, IDependencyResolver dependencyResolver, IModProvider modProvider, IModInstaller modInstaller, IStatusProgress progress)
        {
            _appSettings = appSettings;
            _settingsViewModel = settingsViewModel;
            _dependencyResolver = dependencyResolver;
            _modProvider = modProvider;
            _modInstaller = modInstaller;
            _progress = progress;
            _initializeSyncLock = new object();
            InitializeCommand = ReactiveCommand.CreateFromTask<string, Dictionary<IMod, ModGridItemViewModel>?>(GetModGridItemsAsync);
            InitializeCommand.ToProperty(this, nameof(GridItems), out _gridItems);
            InitializeCommand.IsExecuting.ToProperty(this, nameof(IsExecuting), out _isExecuting);
            InitializeCommand.Select(static x => x is null)
                .CombineLatest(InitializeCommand.IsExecuting)
                .Select(static x => x.First && !x.Second)
                .ToProperty(this, nameof(IsFailed), out _isFailed);
            InitializeCommand.Select(static x => x?.Count == 0)
                .CombineLatest(InitializeCommand.IsExecuting)
                .Select(static x => x.First && !x.Second)
                .ToProperty(this, nameof(IsEmpty), out _isEmpty);
            IsSuccessObservable = InitializeCommand.Select(static x => x?.Count > 0)
                .CombineLatest(InitializeCommand.IsExecuting, settingsViewModel.IsInstallDirValidObservable)
                .Select(static x => x.First && !x.Second && x.Third);
            IsSuccessObservable.ToProperty(this, nameof(IsSuccess), out _isSuccess);
        }

        /// <summary>
        /// Initializes the grid items.
        /// </summary>
        public ReactiveCommand<string, Dictionary<IMod, ModGridItemViewModel>?> InitializeCommand { get; }

        /// <summary>
        /// Signals when mod loading completes.
        /// </summary>
        public IObservable<bool> IsSuccessObservable { get; }

        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> of all available <see cref="IMod"/>s and their respective <see cref="ModGridItemViewModel"/>.
        /// </summary>
        public Dictionary<IMod, ModGridItemViewModel>? GridItems => _gridItems.Value;

        /// <summary>
        /// True if the operation is currently executing, false otherwise.
        /// </summary>
        public bool IsExecuting => _isExecuting.Value;

        /// <summary>
        /// True if the operation successfully ran to completion and returned a non-empty result, false otherwise.
        /// </summary>
        public bool IsSuccess => _isSuccess.Value;

        /// <summary>
        /// True if the operation successfully ran to completion and returned an empty result, false otherwise.
        /// </summary>
        public bool IsEmpty => _isEmpty.Value;

        /// <summary>
        /// True if the operation faulted, false otherwise.
        /// </summary>
        public bool IsFailed => _isFailed.Value;

        /// <summary>
        /// The currently selected <see cref="ModGridItemViewModel"/>.
        /// </summary>
        public ModGridItemViewModel? SelectedGridItem
        {
            get => _selectedGridItem;
            set => this.RaiseAndSetIfChanged(ref _selectedGridItem, value);
        }

        private ModGridItemViewModel? _selectedGridItem;

        /// <summary>
        /// The count of all currently installed mods.
        /// </summary>
        public int InstalledModsCount
        {
            get => _installedModsCount;
            private set => this.RaiseAndSetIfChanged(ref _installedModsCount, value);
        }

        private int _installedModsCount;

        /// <summary>
        /// Asynchronously installs selected mods and uninstalls unselected ones.
        /// </summary>
        public async Task RefreshModsAsync()
        {
            IMod[] install = GridItems!.Values.Where(x => x.IsCheckBoxChecked && (!x.IsUpToDate || _appSettings.Value.ForceReinstallMods))
                .Select(static x => x.AvailableMod)
                .ToArray();
            await InstallModsAsync(_settingsViewModel.InstallDir!, install);
            IMod[] uninstall = GridItems.Values.Where(static x => !x.IsCheckBoxChecked && x.InstalledMod is not null)
                .Select(static x => x.AvailableMod)
                .ToArray();
            await UninstallModsAsync(_settingsViewModel.InstallDir!, uninstall);
        }

        /// <summary>
        /// Asynchronously uninstalls the mod loader.
        /// </summary>
        public async Task UninstallModLoaderAsync(string installDir)
        {
            IMod? modLoader = GridItems!.Values.FirstOrDefault(x => _modProvider.IsModLoader(x.InstalledMod))?.AvailableMod;
            if (modLoader is null) return;
            await UninstallModsAsync(installDir, new[] { modLoader });
        }

        /// <summary>
        /// Asynchronously uninstalls all installed mods.
        /// </summary>
        public async Task UninstallAllModsAsync(string installDir)
        {
            IMod[] mods = GridItems!.Values.Where(static x => x.InstalledMod is not null)
                .Select(static x => x.AvailableMod)
                .ToArray();
            await UninstallModsAsync(installDir, mods);
            _modInstaller.RemoveAllModFiles(installDir);
        }

        /// <summary>
        /// Asynchronously gets available and installed <see cref="IMod"/>s and creates <see cref="ModGridItemViewModel"/>s for them.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> of all available <see cref="IMod"/>s and their respective <see cref="ModGridItemViewModel"/>.</returns>
        private Task<Dictionary<IMod, ModGridItemViewModel>?> GetModGridItemsAsync(string installDir)
        {
            lock (_initializeSyncLock)
            {
                if (GridItems is not null)
                    foreach (ModGridItemViewModel modGridItem in GridItems.Values)
                        modGridItem.Dispose();
                return CreateModGridItemsAsync(installDir);
            }
        }

        private async Task<Dictionary<IMod, ModGridItemViewModel>?> CreateModGridItemsAsync(string installDir)
        {
            await _modProvider.LoadAvailableModsForCurrentVersionAsync(installDir);
            if (_modProvider.AvailableMods is null) return null;
            await _modProvider.LoadInstalledModsAsync(installDir);
            if (_modProvider.InstalledMods is null) return null;
            Dictionary<IMod, ModGridItemViewModel> gridItems = _modProvider.AvailableMods.ToDictionary(static x => x, x => new ModGridItemViewModel(x, _modProvider.InstalledMods.FirstOrDefault(y => y.Name == x.Name), _appSettings, _dependencyResolver));
            InstalledModsCount = gridItems.Values.Count(static x => x.InstalledMod is not null);
            foreach (ModGridItemViewModel gridItem in gridItems.Values)
                gridItem.Initialize(gridItems);
            return gridItems;
        }

        /// <summary>
        /// Asynchronously installs a collection of <see cref="IMod"/>s and updates the <see cref="InstalledModsCount"/>.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="mods">The mods to install.</param>
        private async Task InstallModsAsync(string installDir, IReadOnlyList<IMod> mods)
        {
            _progress.Report(0);
            for (int i = 0; i < mods.Count; i++)
            {
                _progress.Report(new ProgressInfo(StatusType.Installing, mods[i].Name));
                bool success = await _modInstaller.InstallModAsync(installDir, mods[i]);
                if (!success) continue;
                _progress.Report(((double)i + 1) / mods.Count);
                if (!GridItems!.TryGetValue(mods[i], out ModGridItemViewModel? gridItem)) continue;
                if (gridItem.InstalledMod is null) InstalledModsCount++;
                gridItem.InstalledMod = mods[i];
            }

            _progress.Report(1);
            _progress.Report(new ProgressInfo(StatusType.Completed, null));
        }

        /// <summary>
        /// Asynchronously uninstalls a collection of <see cref="IMod"/>s and updates the <see cref="InstalledModsCount"/>.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="mods">The mods to uninstall.</param>
        private async Task UninstallModsAsync(string installDir, IReadOnlyList<IMod> mods)
        {
            _progress.Report(0);
            for (int i = 0; i < mods.Count; i++)
            {
                _progress.Report(new ProgressInfo(StatusType.Uninstalling, mods[i].Name));
                await _modInstaller.UninstallModAsync(installDir, mods[i]);
                _progress.Report(((double)i + 1) / mods.Count);
                if (!GridItems!.TryGetValue(mods[i], out ModGridItemViewModel? gridItem)) continue;
                if (gridItem.InstalledMod is null) continue;
                InstalledModsCount--;
                gridItem.InstalledMod = null;
            }

            _progress.Report(1);
            _progress.Report(new ProgressInfo(StatusType.Completed, null));
        }
    }
}
