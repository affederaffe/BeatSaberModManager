using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;

using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Alias;
using DynamicData.Binding;
using DynamicData.Kernel;

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
        private readonly SourceCache<ModGridItemViewModel, IMod> _gridItemsSourceCache;
        private readonly ReadOnlyObservableCollection<ModGridItemViewModel> _gridItemsList;
        private readonly ObservableAsPropertyHelper<bool> _isExecuting;
        private readonly ObservableAsPropertyHelper<bool> _isSuccess;
        private readonly ObservableAsPropertyHelper<bool> _isEmpty;
        private readonly ObservableAsPropertyHelper<bool> _isFailed;
        private readonly ObservableAsPropertyHelper<int> _installedModsCount;

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
            _gridItemsSourceCache = new SourceCache<ModGridItemViewModel, IMod>(static x => x.AvailableMod);
            InitializeCommand = ReactiveCommand.CreateFromTask<string, bool>(FetchModsAsync);
            InitializeCommand.IsExecuting.ToProperty(this, nameof(IsExecuting), out _isExecuting);
            InitializeCommand.CombineLatest(InitializeCommand.IsExecuting)
                .Select(static x => !x.First && !x.Second)
                .ToProperty(this, nameof(IsFailed), out _isFailed);
            InitializeCommand.CombineLatest(InitializeCommand.IsExecuting)
                .Select(x => x.First && !x.Second && _gridItemsSourceCache.Count == 0)
                .ToProperty(this, nameof(IsEmpty), out _isEmpty);
            IsSuccessObservable = InitializeCommand.CombineLatest(InitializeCommand.IsExecuting, settingsViewModel.IsInstallDirValidObservable)
                .Select(x => x.First && !x.Second && x.Third && _gridItemsSourceCache.Count != 0);
            IsSuccessObservable.ToProperty(this, nameof(IsSuccess), out _isSuccess);
            IObservable<IChangeSet<ModGridItemViewModel, IMod>> connection = _gridItemsSourceCache.Connect();
            IObservable<PropertyValue<ModGridItemViewModel, bool>> whenAnyCheckBoxChecked = connection.WhenPropertyChanged(static x => x.IsCheckBoxChecked);
            whenAnyCheckBoxChecked.Subscribe(UpdateSelectedModsList);
            whenAnyCheckBoxChecked.SelectMany(x => x.Value ? _dependencyResolver.ResolveDependencies(x.Sender.AvailableMod) : _dependencyResolver.UnresolveDependencies(x.Sender.AvailableMod))
                .Select(_gridItemsSourceCache.Lookup)
                .Where(static x => x.HasValue)
                .Select(static x => x.Value)
                .Subscribe(UpdateGridItemState);
            connection.Bind(out _gridItemsList)
                .DisposeMany()
                .Subscribe();
            connection.AutoRefresh(static x => x.InstalledMod)
                .Where(static x => x.InstalledMod is not null)
                .Count()
                .ToProperty(this, nameof(InstalledModsCount), out _installedModsCount);
        }

        /// <summary>
        /// An <see cref="ReadOnlyObservableCollection{T}"/> of <see cref="ModGridItemViewModel"/> for display in a DataGrid.
        /// </summary>
        public ReadOnlyObservableCollection<ModGridItemViewModel> GridItems => _gridItemsList;

        /// <summary>
        /// Initializes the grid items.
        /// </summary>
        public ReactiveCommand<string, bool> InitializeCommand { get; }

        /// <summary>
        /// Signals when mod loading completes.
        /// </summary>
        public IObservable<bool> IsSuccessObservable { get; }

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
        /// The count of all currently installed mods.
        /// </summary>
        public int InstalledModsCount => _installedModsCount.Value;

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
        /// Asynchronously installs selected mods and uninstalls unselected ones.
        /// </summary>
        public async Task UpdateModsAsync()
        {
            IMod[] install = _gridItemsSourceCache.Items.Where(x => x.IsCheckBoxChecked && (!x.IsUpToDate || _appSettings.Value.ForceReinstallMods))
                .Select(static x => x.AvailableMod)
                .ToArray();
            await InstallModsAsync(_settingsViewModel.InstallDir!, install);
            IMod[] uninstall = _gridItemsSourceCache.Items.Where(static x => !x.IsCheckBoxChecked && x.InstalledMod is not null)
                .Select(static x => x.AvailableMod)
                .ToArray();
            await UninstallModsAsync(_settingsViewModel.InstallDir!, uninstall);
        }

        /// <summary>
        /// Asynchronously uninstalls the mod loader.
        /// </summary>
        public async Task UninstallModLoaderAsync(string installDir)
        {
            IMod? modLoader = _gridItemsSourceCache.Items.FirstOrDefault(x => _modProvider.IsModLoader(x.InstalledMod))?.AvailableMod;
            if (modLoader is null) return;
            await UninstallModsAsync(installDir, new[] { modLoader });
        }

        /// <summary>
        /// Asynchronously uninstalls all installed mods.
        /// </summary>
        public async Task UninstallAllModsAsync(string installDir)
        {
            IMod[] mods = _gridItemsSourceCache.Items.Where(static x => x.InstalledMod is not null)
                .Select(static x => x.AvailableMod)
                .ToArray();
            await UninstallModsAsync(installDir, mods);
            _modInstaller.RemoveAllModFiles(installDir);
        }

        private async Task<bool> FetchModsAsync(string installDir)
        {
            await _modProvider.LoadAvailableModsForCurrentVersionAsync(installDir);
            if (_modProvider.AvailableMods is null) return false;
            await _modProvider.LoadInstalledModsAsync(installDir);
            if (_modProvider.InstalledMods is null) return false;
            _gridItemsSourceCache.Edit(innerCache =>
            {
                innerCache.Clear();
                innerCache.AddOrUpdate(_modProvider.AvailableMods.Select(x => new ModGridItemViewModel(x, _modProvider.InstalledMods.FirstOrDefault(y => y.Name == x.Name), _appSettings)));
            });

            return true;
        }

        /// <summary>
        /// Asynchronously installs a collection of <see cref="IMod"/>s/>.
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
                Optional<ModGridItemViewModel> gridItem = _gridItemsSourceCache.Lookup(mods[i]);
                if (!gridItem.HasValue) continue;
                gridItem.Value.InstalledMod = mods[i];
            }

            _progress.Report(1);
            _progress.Report(new ProgressInfo(StatusType.Completed, null));
        }

        /// <summary>
        /// Asynchronously uninstalls a collection of <see cref="IMod"/>s/>.
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
                Optional<ModGridItemViewModel> gridItem = _gridItemsSourceCache.Lookup(mods[i]);
                if (!gridItem.HasValue) continue;
                gridItem.Value.InstalledMod = null;
            }

            _progress.Report(1);
            _progress.Report(new ProgressInfo(StatusType.Completed, null));
        }

        private void UpdateGridItemState(ModGridItemViewModel gridItem)
        {
            bool isDependency = _dependencyResolver.IsDependency(gridItem.AvailableMod);
            gridItem.IsCheckBoxEnabled = !isDependency;
            gridItem.IsCheckBoxChecked = isDependency || (gridItem.IsCheckBoxChecked && gridItem.InstalledMod is not null);
        }

        private void UpdateSelectedModsList(PropertyValue<ModGridItemViewModel, bool> value)
        {
            if (value.Value)
                _appSettings.Value.SelectedMods.Add(value.Sender.AvailableMod.Name);
            else
                _appSettings.Value.SelectedMods.Remove(value.Sender.AvailableMod.Name);
        }
    }
}
