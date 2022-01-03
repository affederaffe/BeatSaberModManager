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
    public class ModsViewModel : ViewModelBase
    {
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IModProvider _modProvider;
        private readonly IModInstaller _modInstaller;
        private readonly IModVersionComparer _modVersionComparer;
        private readonly IStatusProgress _progress;
        private readonly Dictionary<IMod, ModGridItemViewModel> _modGridItemPairs;
        private readonly ObservableAsPropertyHelper<bool> _isSuccess;
        private readonly ObservableAsPropertyHelper<bool> _isFailed;
        private readonly ObservableAsPropertyHelper<IEnumerable<ModGridItemViewModel>?> _gridItems;

        public ModsViewModel(ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IDependencyResolver dependencyResolver, IModProvider modProvider, IModInstaller modInstaller, IModVersionComparer modVersionComparer, IStatusProgress progress)
        {
            _appSettings = appSettings;
            _dependencyResolver = dependencyResolver;
            _modProvider = modProvider;
            _modInstaller = modInstaller;
            _modVersionComparer = modVersionComparer;
            _progress = progress;
            _modGridItemPairs = new Dictionary<IMod, ModGridItemViewModel>();
            InitializeCommand = ReactiveCommand.CreateFromTask<string, IEnumerable<ModGridItemViewModel>?>(InitializeDataGridAsync);
            InitializeCommand.Select(x => x is not null)
                .ToProperty(this, nameof(IsSuccess), out _isSuccess);
            InitializeCommand.ToProperty(this, nameof(GridItems), out _gridItems);
            this.WhenAnyValue(x => x.IsSuccess)
                .CombineLatest(InitializeCommand.IsExecuting)
                .Select(x => !x.First && !x.Second)
                .ToProperty(this, nameof(IsFailed), out _isFailed);
            _appSettings.Value.InstallDir.Changed.Where(installDirValidator.ValidateInstallDir).InvokeCommand(InitializeCommand!);
        }

        public ReactiveCommand<string, IEnumerable<ModGridItemViewModel>?> InitializeCommand { get; }

        public bool IsSuccess => _isSuccess.Value;

        public bool IsFailed => _isFailed.Value;

        public IEnumerable<ModGridItemViewModel>? GridItems => _gridItems.Value;

        private ModGridItemViewModel? _selectedGridItem;
        public ModGridItemViewModel? SelectedGridItem
        {
            get => _selectedGridItem;
            set => this.RaiseAndSetIfChanged(ref _selectedGridItem, value);
        }

        private async Task<IEnumerable<ModGridItemViewModel>?> InitializeDataGridAsync(string installDir)
        {
            _modGridItemPairs.Clear();
            await Task.WhenAll(Task.Run(() => _modProvider.LoadAvailableModsForCurrentVersionAsync(installDir)), Task.Run(() => _modProvider.LoadInstalledModsAsync(installDir))).ConfigureAwait(false);
            if (_modProvider.AvailableMods is null) return null;
            foreach (IMod availableMod in _modProvider.AvailableMods)
            {
                IMod? installedMod = _modProvider.InstalledMods?.FirstOrDefault(x => x.Name == availableMod.Name);
                ModGridItemViewModel gridItem = new(_modVersionComparer, availableMod, installedMod);
                _modGridItemPairs.Add(availableMod, gridItem);
                gridItem.IsCheckBoxChecked = gridItem.InstalledMod is not null || _appSettings.Value.SelectedMods.Contains(availableMod.Name);
            }

            foreach (ModGridItemViewModel gridItem in _modGridItemPairs.Values)
                gridItem.WhenAnyValue(x => x.IsCheckBoxChecked).Subscribe(b => OnCheckboxUpdated(b, gridItem));

            return _modGridItemPairs.Values;
        }

        public async Task RefreshModsAsync()
        {
            IEnumerable<IMod> install = _modGridItemPairs.Values.Where(x => x.IsCheckBoxChecked && (!x.IsUpToDate || _appSettings.Value.ForceReinstallMods)).Select(x => x.AvailableMod);
            await InstallMods(_appSettings.Value.InstallDir.Value!, install).ConfigureAwait(false);
            IEnumerable<IMod> uninstall = _modGridItemPairs.Values.Where(x => !x.IsCheckBoxChecked && x.InstalledMod is not null).Select(x => x.AvailableMod);
            await UninstallMods(_appSettings.Value.InstallDir.Value!, uninstall).ConfigureAwait(false);
        }

        public async Task UninstallModLoaderAsync()
        {
            IMod? modLoader = _modGridItemPairs.Values.FirstOrDefault(x => _modProvider.IsModLoader(x.InstalledMod))?.AvailableMod ??
                              _modProvider.InstalledMods?.FirstOrDefault(_modProvider.IsModLoader);
            if (modLoader is null) return;
            await UninstallMods(_appSettings.Value.InstallDir.Value!, new[] { modLoader }).ConfigureAwait(false);
        }

        public async Task UninstallAllModsAsync()
        {
            IEnumerable<IMod> mods = _modGridItemPairs.Values.Where(x => x.InstalledMod is not null).Select(x => x.AvailableMod);
            await UninstallMods(_appSettings.Value.InstallDir.Value!, mods).ConfigureAwait(false);
            _modInstaller.RemoveAllMods(_appSettings.Value.InstallDir.Value!);
        }

        private async Task InstallMods(string installDir, IEnumerable<IMod> mods)
        {
            await foreach (IMod mod in _modInstaller.InstallModsAsync(installDir, mods, _progress).ConfigureAwait(false))
            {
                if (_modGridItemPairs.TryGetValue(mod, out ModGridItemViewModel? gridItem))
                    gridItem.InstalledMod = mod;
            }
        }

        private async Task UninstallMods(string installDir, IEnumerable<IMod> mods)
        {
            await foreach (IMod mod in _modInstaller.UninstallModsAsync(installDir, mods, _progress).ConfigureAwait(false))
            {
                if (_modGridItemPairs.TryGetValue(mod, out ModGridItemViewModel? gridItem))
                    gridItem.InstalledMod = null;
            }
        }

        private void OnCheckboxUpdated(bool checkBoxChecked, ModGridItemViewModel gridItem)
        {
            if (checkBoxChecked)
            {
                _appSettings.Value.SelectedMods.Add(gridItem.AvailableMod.Name);
                IEnumerable<IMod> affectedMods = _dependencyResolver.ResolveDependencies(gridItem.AvailableMod);
                UpdateGridItems(affectedMods);
            }
            else
            {
                _appSettings.Value.SelectedMods.Remove(gridItem.AvailableMod.Name);
                IEnumerable<IMod> affectedMods = _dependencyResolver.UnresolveDependencies(gridItem.AvailableMod);
                UpdateGridItems(affectedMods);
            }
        }

        private void UpdateGridItems(IEnumerable<IMod> mods)
        {
            foreach (IMod mod in mods)
            {
                if (!_modGridItemPairs.TryGetValue(mod, out ModGridItemViewModel? gridItem)) continue;
                bool isDependency = _dependencyResolver.IsDependency(gridItem.AvailableMod);
                gridItem.IsCheckBoxEnabled = !isDependency;
                gridItem.IsCheckBoxChecked = gridItem.IsCheckBoxChecked || isDependency;
            }
        }
    }
}