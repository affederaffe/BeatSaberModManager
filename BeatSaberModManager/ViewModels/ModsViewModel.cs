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

        private MainWindowViewModel? _mainWindowViewModel;

        public ModsViewModel(Settings settings, IModProvider modProvider, IModInstaller modInstaller, IModVersionComparer modVersionComparer)
        {
            _settings = settings;
            _modProvider = modProvider;
            _modInstaller = modInstaller;
            _modVersionComparer = modVersionComparer;
        }

        public ObservableCollection<ModGridItemViewModel> GridItems { get; } = new();

        private ModGridItemViewModel? _selectedGridItem;
        public ModGridItemViewModel? SelectedGridItem
        {
            get => _selectedGridItem;
            set => this.RaiseAndSetIfChanged(ref _selectedGridItem, value);
        }

        public async Task RefreshDataGridAsync()
        {
            if (_settings.InstallDir is null) return;
            await Task.WhenAll(Task.Run(_modProvider.LoadAvailableModsForCurrentVersionAsync), Task.Run(_modProvider.LoadInstalledModsAsync));
            if (_modProvider.AvailableMods is null || _modProvider.InstalledMods is null) return;
            for (int i = 0; i < _modProvider.AvailableMods.Length; i++)
            {
                IMod availableMod = _modProvider.AvailableMods[i];
                IMod? installedMod = _modProvider.InstalledMods.FirstOrDefault(x => x.Name == availableMod.Name);
                ModGridItemViewModel gridItem;
                if (GridItems.Count > i)
                {
                    gridItem = GridItems[i];
                    gridItem.Subscription?.Dispose();
                    gridItem.AvailableMod = availableMod;
                    gridItem.InstalledMod = installedMod;
                }
                else
                {
                    gridItem = new ModGridItemViewModel(availableMod, installedMod);
                    GridItems.Add(gridItem);
                }

                if (installedMod is not null) _modProvider.ResolveDependencies(availableMod);
                UpdateCheckboxEnabled(gridItem);
                gridItem.Subscription = gridItem.WhenAnyValue(x => x.IsCheckBoxChecked).Subscribe(_ => OnCheckboxUpdated(gridItem));
            }
        }

        public async Task RefreshModsAsync()
        {
            _mainWindowViewModel ??= Locator.Current.GetService<MainWindowViewModel>();
            for (int i = 0; i < GridItems.Count; i++)
            {
                ModGridItemViewModel gridItem = GridItems[i];
                _mainWindowViewModel.ProgressBarValue = i * 100f / (GridItems.Count - 1);
                if (gridItem.IsCheckBoxChecked && _modVersionComparer.CompareVersions(gridItem.AvailableMod.Version, gridItem.InstalledMod?.Version) > 0)
                {
                    _mainWindowViewModel.ProgressBarPreTextResourceName = "MainWindow:InstallModText";
                    _mainWindowViewModel.ProgressBarText = gridItem.AvailableMod.Name;
                    await InstallModAsync(gridItem);
                }
                else if (!GridItems[i].IsCheckBoxChecked && _modProvider.InstalledMods!.Contains(gridItem.InstalledMod!))
                {
                    _mainWindowViewModel.ProgressBarPreTextResourceName = "MainWindow:UninstallModText";
                    _mainWindowViewModel.ProgressBarText = gridItem.AvailableMod.Name;
                    await UninstallModAsync(gridItem);
                }
            }
        }

        public async Task UninstallModLoaderAsync()
        {
            _mainWindowViewModel ??= Locator.Current.GetService<MainWindowViewModel>();
            ModGridItemViewModel? gridItem = GridItems.FirstOrDefault(x => x.AvailableMod.Name?.ToLowerInvariant() == _modProvider.ModLoaderName);
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

        public async Task UninstallAllModsAsync()
        {
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

        private void OnCheckboxUpdated(ModGridItemViewModel gridItem)
        {
            if (gridItem.IsCheckBoxChecked) _modProvider.ResolveDependencies(gridItem.AvailableMod);
            else _modProvider.UnresolveDependencies(gridItem.AvailableMod);
            foreach (ModGridItemViewModel listItem in GridItems)
                UpdateCheckboxEnabled(listItem);
        }

        private void UpdateCheckboxEnabled(ModGridItemViewModel gridItem)
        {
            bool isDependency = gridItem.AvailableMod.Required || (_modProvider.Dependencies.TryGetValue(gridItem.AvailableMod, out HashSet<IMod>? dependants) && dependants.Count != 0);
            gridItem.IsCheckBoxEnabled = !isDependency;
            gridItem.IsCheckBoxChecked = gridItem.IsCheckBoxChecked || isDependency || gridItem.InstalledMod is not null;
        }
    }
}