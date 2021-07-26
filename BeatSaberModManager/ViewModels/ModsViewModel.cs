using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models;
using BeatSaberModManager.Models.Interfaces;

using ReactiveUI;

using Splat;


namespace BeatSaberModManager.ViewModels
{
    public class ModsViewModel : ReactiveObject
    {
        private readonly Settings _settings;
        private readonly IModProvider _modProvider;
        private readonly IModInstaller _modInstaller;
        private readonly IModVersionComparer _modVersionComparer;
        private readonly IGameVersionProvider _gameVersionProvider;

        private MainWindowViewModel? _mainWindowViewModel;

        public ModsViewModel(Settings settings, IModProvider modProvider, IModInstaller modInstaller, IModVersionComparer modVersionComparer, IGameVersionProvider gameVersionProvider)
        {
            _settings = settings;
            _modProvider = modProvider;
            _modInstaller = modInstaller;
            _modVersionComparer = modVersionComparer;
            _gameVersionProvider = gameVersionProvider;
        }

        private ObservableCollection<ModGridItemViewModel>? _gridItems;
        public ObservableCollection<ModGridItemViewModel>? GridItems
        {
            get => _gridItems;
            private set => this.RaiseAndSetIfChanged(ref _gridItems, value);
        }

        private ModGridItemViewModel? _selectedGridItem;
        public ModGridItemViewModel? SelectedGridItem
        {
            get => _selectedGridItem;
            set => this.RaiseAndSetIfChanged(ref _selectedGridItem, value);
        }

        public async Task RefreshDataGridAsync()
        {
            if (_settings.InstallDir is null) return;
            await Task.WhenAll(Task.Run(LoadAvailableModsForCurrentVersionAsync), Task.Run(_modProvider.LoadInstalledModsAsync));
            if (_modProvider.AvailableMods is null || _modProvider.InstalledMods is null) return;

            ObservableCollection<ModGridItemViewModel>? gridItems = null;
            if (GridItems is not null)
            {
                foreach (ModGridItemViewModel gridItem in GridItems)
                    gridItem.Subscription?.Dispose();
                GridItems.Clear();
                gridItems = GridItems;
            }

            gridItems ??= new ObservableCollection<ModGridItemViewModel>();
            foreach (IMod availableMod in _modProvider.AvailableMods)
            {
                IMod? installedMod = _modProvider.InstalledMods.FirstOrDefault(x => x.Name == availableMod.Name);
                gridItems.Add(new ModGridItemViewModel(availableMod, installedMod));
                if (installedMod is not null) _modProvider.ResolveDependencies(availableMod);
            }

            GridItems = gridItems;
            foreach (ModGridItemViewModel gridItem in gridItems)
            {
                UpdateCheckboxEnabled(gridItem);
                gridItem.Subscription = gridItem.WhenAnyValue(x => x.IsCheckBoxChecked)
                                                .Subscribe(isChecked => OnCheckboxUpdated(isChecked, gridItem));
            }
        }

        public async Task RefreshModsAsync()
        {
            _mainWindowViewModel ??= Locator.Current.GetService<MainWindowViewModel>();
            for (int i = 0; i < GridItems!.Count; i++)
            {
                _mainWindowViewModel.ProgressBarValue = i * 100f / (GridItems.Count - 1);
                switch (GridItems[i].IsCheckBoxChecked)
                {
                    case true when _modVersionComparer.CompareVersions(GridItems[i].AvailableMod.Version, GridItems[i].InstalledMod?.Version) > 0:
                        _mainWindowViewModel.ProgressBarPreTextResourceName = "MainWindow:InstallModText";
                        _mainWindowViewModel.ProgressBarText = GridItems[i].AvailableMod.Name;
                        await InstallModAsync(GridItems[i]);
                        break;
                    case false when _modProvider.InstalledMods!.Contains(GridItems[i].InstalledMod!):
                        _mainWindowViewModel.ProgressBarPreTextResourceName = "MainWindow:UninstallModText";
                        _mainWindowViewModel.ProgressBarText = GridItems[i].AvailableMod.Name;
                        await UninstallModAsync(GridItems[i]);
                        break;
                }
            }
        }

        public async Task UninstallModLoaderAsync()
        {
            _mainWindowViewModel ??= Locator.Current.GetService<MainWindowViewModel>();
            ModGridItemViewModel? gridItem = GridItems?.FirstOrDefault(x => x.AvailableMod.Name?.ToLowerInvariant() == _modProvider.ModLoaderName);
            if (gridItem is not null)
            {
                _mainWindowViewModel.ProgressBarPreTextResourceName = "MainWindow:UninstallModText";
                _mainWindowViewModel.ProgressBarText = gridItem.AvailableMod.Name;
                await UninstallModAsync(gridItem);
                _mainWindowViewModel.ProgressBarValue = 100;
                return;
            }

            IMod? modLoader = _modProvider.InstalledMods?.FirstOrDefault(x => x.Name?.ToLowerInvariant() == _modProvider.ModLoaderName);
            if (modLoader is null) return;
            _mainWindowViewModel.ProgressBarPreTextResourceName = "MainWindow:UninstallModText";
            _mainWindowViewModel.ProgressBarText = modLoader.Name;
            await Task.Run(() => _modInstaller.UninstallModAsync(modLoader));
            _mainWindowViewModel.ProgressBarValue = 100;
        }

        public async Task UninstallAllModsAsyn()
        {
            if (GridItems is not null)
                foreach (ModGridItemViewModel gridItem in GridItems)
                    await UninstallModAsync(gridItem);
            _modInstaller.RemoveAllMods();
        }

        private async Task InstallModAsync(ModGridItemViewModel gridItem)
        {
            bool success = await Task.Run(() => _modInstaller.InstallModAsync(gridItem.AvailableMod));
            if (!success) return;
            gridItem.InstalledMod = gridItem.AvailableMod;
        }

        private async Task UninstallModAsync(ModGridItemViewModel gridItem)
        {
            if (gridItem.InstalledMod is null) return;
            bool success = await Task.Run(() => _modInstaller.UninstallModAsync(gridItem.InstalledMod));
            if (!success) return;
            gridItem.InstalledMod = null;
        }

        private void OnCheckboxUpdated(bool isChecked, ModGridItemViewModel gridItem)
        {
            if (isChecked) _modProvider.ResolveDependencies(gridItem.AvailableMod);
            else _modProvider.UnresolveDependencies(gridItem.AvailableMod);
            foreach (ModGridItemViewModel listItem in GridItems!)
                UpdateCheckboxEnabled(listItem);
        }

        private void UpdateCheckboxEnabled(ModGridItemViewModel gridItem)
        {
            bool isDependency = gridItem.AvailableMod.Required ||
                                (_modProvider.Dependencies.TryGetValue(gridItem.AvailableMod, out HashSet<IMod>? dependants) &&
                                dependants.Count != 0);
            gridItem.IsCheckBoxEnabled = !isDependency;
            gridItem.IsCheckBoxChecked = gridItem.IsCheckBoxChecked || isDependency || gridItem.InstalledMod is not null;
        }

        private async Task LoadAvailableModsForCurrentVersionAsync()
        {
            string? version = _gameVersionProvider.GetGameVersion();
            if (version is null) return;
            await _modProvider.LoadAvailableModsForVersionAsync(version);
        }
    }
}