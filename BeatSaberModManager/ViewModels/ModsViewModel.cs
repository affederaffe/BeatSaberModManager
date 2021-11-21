using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class ModsViewModel : ViewModelBase
    {
        private readonly AppSettings _appSettings;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IModProvider _modProvider;
        private readonly IModInstaller _modInstaller;
        private readonly IModVersionComparer _modVersionComparer;

        public ModsViewModel(ISettings<AppSettings> appSettings, IDependencyResolver dependencyResolver, IModProvider modProvider, IModInstaller modInstaller, IModVersionComparer modVersionComparer)
        {
            _appSettings = appSettings.Value;
            _dependencyResolver = dependencyResolver;
            _modProvider = modProvider;
            _modInstaller = modInstaller;
            _modVersionComparer = modVersionComparer;
        }

        public Dictionary<IMod, ModGridItemViewModel> GridItems { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        private bool _isSuccess;
        public bool IsSuccess
        {
            get => _isSuccess;
            set => this.RaiseAndSetIfChanged(ref _isSuccess, value);
        }

        private bool _isFailed;
        public bool IsFailed
        {
            get => _isFailed;
            set => this.RaiseAndSetIfChanged(ref _isFailed, value);
        }

        private ModGridItemViewModel? _selectedGridItem;
        public ModGridItemViewModel? SelectedGridItem
        {
            get => _selectedGridItem;
            set => this.RaiseAndSetIfChanged(ref _selectedGridItem, value);
        }

        public async Task InitializeDataGridAsync()
        {
            IsLoading = true;
            await Task.WhenAll(Task.Run(_modProvider.LoadAvailableModsForCurrentVersionAsync), Task.Run(_modProvider.LoadInstalledModsAsync));
            IsSuccess = _modProvider.AvailableMods?.Length > 0;
            IsFailed = !IsSuccess;
            IsLoading = false;
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
            IEnumerable<IMod> install = GridItems.Values.Where(x => x.IsCheckBoxChecked && (!x.IsUpToDate || _appSettings.ForceReinstallMods)).Select(x => x.AvailableMod);
            await InstallMods(install);
            IEnumerable<IMod> uninstall = GridItems.Values.Where(x => !x.IsCheckBoxChecked && x.InstalledMod is not null).Select(x => x.AvailableMod);
            await UninstallMods(uninstall);
        }

        public async Task UninstallModLoaderAsync()
        {
            IMod? modLoader = GridItems.Values.FirstOrDefault(x => x.InstalledMod?.Name.ToLowerInvariant() == _modProvider.ModLoaderName)?.AvailableMod;
            if (modLoader is null) return;
            await UninstallMods(new[] { modLoader });
        }

        public async Task UninstallAllModsAsync()
        {
            IEnumerable<IMod> mods = GridItems.Values.Where(x => x.InstalledMod is not null).Select(x => x.AvailableMod);
            await UninstallMods(mods);
            _modInstaller.RemoveAllMods();
        }

        private async Task InstallMods(IEnumerable<IMod> mods)
        {
            await foreach (IMod mod in _modInstaller.InstallModsAsync(mods))
                GridItems[mod].InstalledMod = mod;
        }

        private async Task UninstallMods(IEnumerable<IMod> mods)
        {
            await foreach (IMod mod in _modInstaller.UninstallModsAsync(mods))
                GridItems[mod].InstalledMod = null;
        }

        private void OnCheckboxUpdated(ModGridItemViewModel gridItem)
        {
            if (gridItem.IsCheckBoxChecked)
            {
                _appSettings.SelectedMods.Add(gridItem.AvailableMod.Name);
                IEnumerable<IMod> affectedMods = _dependencyResolver.ResolveDependencies(gridItem.AvailableMod);
                UpdateGridItems(affectedMods);
            }
            else
            {
                _appSettings.SelectedMods.Remove(gridItem.AvailableMod.Name);
                IEnumerable<IMod> affectedMods = _dependencyResolver.UnresolveDependencies(gridItem.AvailableMod);
                UpdateGridItems(affectedMods);
            }
        }

        private void UpdateGridItems(IEnumerable<IMod> mods)
        {
            foreach (IMod mod in mods)
            {
                ModGridItemViewModel gridItem = GridItems[mod];
                bool isDependency = _dependencyResolver.IsDependency(gridItem.AvailableMod);
                gridItem.IsCheckBoxEnabled = !isDependency;
                gridItem.IsCheckBoxChecked = gridItem.IsCheckBoxChecked || isDependency;
            }
        }
    }
}