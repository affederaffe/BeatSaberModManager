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
    public class ModsViewModel : ReactiveObject
    {
        private readonly AppSettings _appSettings;
        private readonly IModProvider _modProvider;
        private readonly IModInstaller _modInstaller;
        private readonly IModVersionComparer _modVersionComparer;
        private readonly ObservableAsPropertyHelper<bool> _noModsTextVisible;

        public ModsViewModel(ISettings<AppSettings> appSettings, IModProvider modProvider, IModInstaller modInstaller, IModVersionComparer modVersionComparer)
        {
            _appSettings = appSettings.Value;
            _modProvider = modProvider;
            _modInstaller = modInstaller;
            _modVersionComparer = modVersionComparer;
            this.WhenAnyValue(x => x.AreModsLoading, x => x.AreModsAvailable)
                .Select(tuple => !tuple.Item1 && !tuple.Item2)
                .ToProperty(this, nameof(NoModsTextVisible), out _noModsTextVisible);
        }

        public Dictionary<IMod, ModGridItemViewModel> GridItems { get; } = new();

        public bool NoModsTextVisible => _noModsTextVisible.Value;

        private bool _areModsLoading;
        public bool AreModsLoading
        {
            get => _areModsLoading;
            set => this.RaiseAndSetIfChanged(ref _areModsLoading, value);
        }

        private bool _areModsAvailable;
        public bool AreModsAvailable
        {
            get => _areModsAvailable;
            set => this.RaiseAndSetIfChanged(ref _areModsAvailable, value);
        }

        private ModGridItemViewModel? _selectedGridItem;
        public ModGridItemViewModel? SelectedGridItem
        {
            get => _selectedGridItem;
            set => this.RaiseAndSetIfChanged(ref _selectedGridItem, value);
        }

        public async Task RefreshDataGridAsync()
        {
            AreModsLoading = true;
            await Task.WhenAll(Task.Run(_modProvider.LoadAvailableModsForCurrentVersionAsync), Task.Run(_modProvider.LoadInstalledModsAsync));
            AreModsLoading = false;
            AreModsAvailable = _modProvider.AvailableMods?.Length > 0;
            if (_modProvider.AvailableMods is null) return;
            foreach (IMod availableMod in _modProvider.AvailableMods)
            {
                IMod? installedMod = _modProvider.InstalledMods?.FirstOrDefault(x => x.Name == availableMod.Name);
                ModGridItemViewModel gridItem = new(_modVersionComparer, availableMod, installedMod);
                GridItems.Add(availableMod, gridItem);
                gridItem.IsCheckBoxChecked = gridItem.InstalledMod is not null || _appSettings.SelectedMods.Contains(availableMod.Name);
            }

            foreach (ModGridItemViewModel gridItem in GridItems.Values)
                gridItem.WhenAnyValue(x => x.IsCheckBoxChecked).Subscribe(_ => OnCheckboxUpdated(gridItem));
        }

        public async Task RefreshModsAsync()
        {
            IEnumerable<IMod> mods = GridItems.Values.Where(x => x.IsCheckBoxChecked && !x.IsUpToDate).Select(x => x.AvailableMod);
            await foreach (IMod mod in _modInstaller.InstallModsAsync(mods))
                GridItems[mod].InstalledMod = mod;

            mods = GridItems.Values.Where(x => !x.IsCheckBoxChecked && x.InstalledMod is not null).Select(x => x.AvailableMod);
            await foreach (IMod mod in _modInstaller.UninstallModsAsync(mods))
                GridItems[mod].InstalledMod = null;
        }

        public async Task UninstallModLoaderAsync()
        {
            IMod? modLoader = GridItems.Values.FirstOrDefault(x => x.InstalledMod?.Name.ToLowerInvariant() == _modProvider.ModLoaderName)?.InstalledMod;
            if (modLoader is null) return;
            await foreach (IMod mod in _modInstaller.UninstallModsAsync(new []{ modLoader }))
                GridItems[mod].InstalledMod = null;
        }

        public async Task UninstallAllModsAsync()
        {
            IEnumerable<IMod> mods = GridItems.Values.Where(x => x.InstalledMod is not null).Select(x => x.AvailableMod);
            await foreach (IMod mod in _modInstaller.UninstallModsAsync(mods))
                GridItems[mod].InstalledMod = null;
            _modInstaller.RemoveAllMods();
        }

        private void OnCheckboxUpdated(ModGridItemViewModel gridItem)
        {
            if (gridItem.IsCheckBoxChecked)
            {
                _appSettings.SelectedMods.Add(gridItem.AvailableMod.Name);
                foreach (IMod mod in _modProvider.ResolveDependencies(gridItem.AvailableMod))
                    UpdateGridItem(GridItems[mod]);
            }
            else
            {
                _appSettings.SelectedMods.Remove(gridItem.AvailableMod.Name);
                foreach (IMod mod in _modProvider.UnresolveDependencies(gridItem.AvailableMod))
                    UpdateGridItem(GridItems[mod]);
            }
        }

        private void UpdateGridItem(ModGridItemViewModel gridItem)
        {
            bool isDependency = _modProvider.Dependencies.TryGetValue(gridItem.AvailableMod, out HashSet<IMod>? dependents) && dependents.Count != 0;
            gridItem.IsCheckBoxEnabled = !isDependency;
            gridItem.IsCheckBoxChecked = gridItem.IsCheckBoxChecked || isDependency;
        }
    }
}