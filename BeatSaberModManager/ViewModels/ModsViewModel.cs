using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

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
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IModProvider _modProvider;
        private readonly IModInstaller _modInstaller;
        private readonly IStatusProgress _progress;
        private readonly ObservableAsPropertyHelper<Dictionary<IMod, ModGridItemViewModel>?> _gridItems;
        private readonly ObservableAsPropertyHelper<bool> _isExecuting;
        private readonly ObservableAsPropertyHelper<bool> _isSuccess;
        private readonly ObservableAsPropertyHelper<bool> _isFailed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModsViewModel"/> class.
        /// </summary>
        public ModsViewModel(ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IDependencyResolver dependencyResolver, IModProvider modProvider, IModInstaller modInstaller, IStatusProgress progress)
        {
            _appSettings = appSettings;
            _dependencyResolver = dependencyResolver;
            _modProvider = modProvider;
            _modInstaller = modInstaller;
            _progress = progress;
            ReactiveCommand<string, Dictionary<IMod, ModGridItemViewModel>?> initializeCommand = ReactiveCommand.CreateFromTask<string, Dictionary<IMod, ModGridItemViewModel>?>(GetModGridItemsAsync);
            initializeCommand.ToProperty(this, nameof(GridItems), out _gridItems);
            initializeCommand.IsExecuting.ToProperty(this, nameof(IsExecuting), out _isExecuting);
            IsSuccessObservable = initializeCommand.Select(static x => x is not null)
                .CombineLatest(initializeCommand.IsExecuting)
                .Select(static x => x.First && !x.Second);
            IsSuccessObservable.ToProperty(this, nameof(IsSuccess), out _isSuccess);
            IsSuccessObservable.CombineLatest(initializeCommand.IsExecuting)
                .Select(static x => !x.First && !x.Second)
                .ToProperty(this, nameof(IsFailed), out _isFailed);
            IObservable<string?> whenAnyInstallDir = appSettings.Value.WhenAnyValue(static x => x.InstallDir);
            IsInstallDirValidObservable = whenAnyInstallDir.Select(installDirValidator.ValidateInstallDir);
            ValidatedInstallDirObservable = whenAnyInstallDir.Where(installDirValidator.ValidateInstallDir)!;
            ValidatedInstallDirObservable.InvokeCommand(initializeCommand);
        }

        /// <summary>
        /// Signals if the game's installation directory is valid.
        /// </summary>
        public IObservable<bool> IsInstallDirValidObservable { get; }

        /// <summary>
        /// Signals when the game's installation directory becomes valid.
        /// </summary>
        public IObservable<string> ValidatedInstallDirObservable { get; }

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
        /// True if the operation successfully ran to completion, false otherwise.
        /// </summary>
        public bool IsSuccess => _isSuccess.Value;

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
        /// Activates or deactivates the search.
        /// </summary>
        public bool IsSearchEnabled
        {
            get => _isSearchEnabled;
            set => this.RaiseAndSetIfChanged(ref _isSearchEnabled, value);
        }

        private bool _isSearchEnabled;

        /// <summary>
        /// The entered search query.
        /// </summary>
        public string? SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        private string? _searchQuery;

        /// <summary>
        /// The count of all currently installed mods.
        /// </summary>
        public int InstalledModsCount
        {
            get => _installedModsCount;
            set => this.RaiseAndSetIfChanged(ref _installedModsCount, value);
        }

        private int _installedModsCount;

        /// <summary>
        /// Asynchronously gets available and installed <see cref="IMod"/>s and creates <see cref="ModGridItemViewModel"/>s for them.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> of all available <see cref="IMod"/>s and their respective <see cref="ModGridItemViewModel"/>.</returns>
        private async Task<Dictionary<IMod, ModGridItemViewModel>?> GetModGridItemsAsync(string installDir)
        {
            await Task.WhenAll(_modProvider.LoadAvailableModsForCurrentVersionAsync(installDir), _modProvider.LoadInstalledModsAsync(installDir)).ConfigureAwait(false);
            if (_modProvider.AvailableMods is null || _modProvider.InstalledMods is null) return null;
            InstalledModsCount = 0;
            Dictionary<IMod, ModGridItemViewModel> gridItems = _modProvider.AvailableMods.ToDictionary(static x => x, x => new ModGridItemViewModel(x, _modProvider.InstalledMods.FirstOrDefault(y => y.Name == x.Name), _appSettings, _dependencyResolver));
            foreach (ModGridItemViewModel gridItem in gridItems.Values)
            {
                if (gridItem.InstalledMod is not null) InstalledModsCount++;
                gridItem.Initialize(gridItems);
            }

            return gridItems;
        }

        /// <summary>
        /// Asynchronously installs selected mods and uninstalls unselected ones.
        /// </summary>
        public async Task RefreshModsAsync()
        {
            IEnumerable<IMod> install = GridItems!.Values.Where(x => x.IsCheckBoxChecked && (!x.IsUpToDate || _appSettings.Value.ForceReinstallMods)).Select(static x => x.AvailableMod);
            await InstallMods(_appSettings.Value.InstallDir!, install).ConfigureAwait(false);
            IEnumerable<IMod> uninstall = GridItems.Values.Where(static x => !x.IsCheckBoxChecked && x.InstalledMod is not null).Select(static x => x.AvailableMod);
            await UninstallMods(_appSettings.Value.InstallDir!, uninstall).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously uninstalls the mod loader.
        /// </summary>
        public async Task UninstallModLoaderAsync()
        {
            IMod? modLoader = GridItems!.Values.FirstOrDefault(x => _modProvider.IsModLoader(x.InstalledMod))?.AvailableMod;
            if (modLoader is null) return;
            await UninstallMods(_appSettings.Value.InstallDir!, new[] { modLoader }).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously uninstalls all installed mods.
        /// </summary>
        public async Task UninstallAllModsAsync()
        {
            IEnumerable<IMod> mods = GridItems!.Values.Where(static x => x.InstalledMod is not null).Select(static x => x.AvailableMod);
            await UninstallMods(_appSettings.Value.InstallDir!, mods).ConfigureAwait(false);
            _modInstaller.RemoveAllMods(_appSettings.Value.InstallDir!);
        }

        /// <summary>
        /// Asynchronously installs a collection of <see cref="IMod"/>s and updates the <see cref="InstalledModsCount"/>.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="mods">The mods to install.</param>
        private async Task InstallMods(string installDir, IEnumerable<IMod> mods)
        {
            await foreach (IMod mod in _modInstaller.InstallModsAsync(installDir, mods, _progress).ConfigureAwait(false))
            {
                if (!GridItems!.TryGetValue(mod, out ModGridItemViewModel? gridItem)) continue;
                if (gridItem.InstalledMod is null) InstalledModsCount++;
                gridItem.InstalledMod = mod;
            }
        }

        /// <summary>
        /// Asynchronously uninstalls a collection of <see cref="IMod"/>s and updates the <see cref="InstalledModsCount"/>.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="mods">The mods to uninstall.</param>
        private async Task UninstallMods(string installDir, IEnumerable<IMod> mods)
        {
            await foreach (IMod mod in _modInstaller.UninstallModsAsync(installDir, mods, _progress).ConfigureAwait(false))
            {
                if (!GridItems!.TryGetValue(mod, out ModGridItemViewModel? gridItem)) continue;
                if (gridItem.InstalledMod is not null) InstalledModsCount--;
                gridItem.InstalledMod = null;
            }
        }
    }
}