using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;

using DynamicData.Binding;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class ModsViewModel : ReactiveObject
    {
        private readonly IModProvider _modProvider;
        private readonly IModInstaller _modInstaller;
        private readonly IModVersionComparer _modVersionComparer;
        private readonly IStatusProgress _progress;
        private readonly ObservableAsPropertyHelper<bool> _areModsAvailable;

        public ModsViewModel(IModProvider modProvider, IModInstaller modInstaller, IModVersionComparer modVersionComparer, IStatusProgress progress)
        {
            _modProvider = modProvider;
            _modInstaller = modInstaller;
            _modVersionComparer = modVersionComparer;
            _progress = progress;
            this.WhenAnyValue(x => x.GridItems.Count)
                .Select(x => x > 0)
                .ToProperty(this, nameof(AreModsAvailable), out _areModsAvailable);
        }

        public ObservableCollectionExtended<ModGridItemViewModel> GridItems { get; } = new();

        public bool AreModsAvailable => _areModsAvailable.Value;

        private ModGridItemViewModel? _selectedGridItem;
        public ModGridItemViewModel? SelectedGridItem
        {
            get => _selectedGridItem;
            set => this.RaiseAndSetIfChanged(ref _selectedGridItem, value);
        }

        public async Task RefreshDataGridAsync()
        {
            await Task.WhenAll(Task.Run(_modProvider.LoadAvailableModsForCurrentVersionAsync), Task.Run(_modProvider.LoadInstalledModsAsync));
            if (_modProvider.AvailableMods is null) return;
            for (int i = 0; i < _modProvider.AvailableMods.Length; i++)
            {
                IMod availableMod = _modProvider.AvailableMods[i];
                IMod? installedMod = _modProvider.InstalledMods?.FirstOrDefault(x => x.Name == availableMod.Name);
                ModGridItemViewModel gridItem;
                if (GridItems.Count > i)
                {
                    gridItem = GridItems[i];
                    gridItem.AvailableMod = availableMod;
                    gridItem.InstalledMod = installedMod;
                }
                else
                {
                    gridItem = new ModGridItemViewModel(_modVersionComparer, availableMod, installedMod);
                    GridItems.Add(gridItem);
                }

                gridItem.IsCheckBoxChecked = gridItem.InstalledMod is not null;
                gridItem.WhenAnyValue(x => x.IsCheckBoxChecked).Subscribe(_ => OnCheckboxUpdated(gridItem));
            }
        }

        public async Task RefreshModsAsync()
        {
            ModGridItemViewModel[] modsToInstall = GridItems.Where(x => x.IsCheckBoxChecked && !x.IsUpToDate).ToArray();
            ModGridItemViewModel[] modsToUninstall = GridItems.Where(x => !x.IsCheckBoxChecked && _modProvider.InstalledMods!.Contains(x.InstalledMod!)).ToArray();
            int sum = modsToInstall.Length + modsToUninstall.Length;

            _progress.Report(0);
            for (int i = 0; i < modsToInstall.Length; i++)
            {
                await InstallModAsync(modsToInstall[i]);
                _progress.Report(((double)i + 1) / sum);
            }

            for (int i = 0; i < modsToUninstall.Length; i++)
            {
                await UninstallModAsync(modsToUninstall[i]);
                _progress.Report(((double)i + 1 + modsToInstall.Length) / sum);
            }
        }

        public async Task UninstallModLoaderAsync()
        {
            ModGridItemViewModel? gridItem = GridItems.FirstOrDefault(x => x.InstalledMod?.Name?.ToLowerInvariant() == _modProvider.ModLoaderName);
            if (gridItem is not null)
            {
                await UninstallModAsync(gridItem);
                return;
            }

            IMod? modLoader = _modProvider.InstalledMods?.FirstOrDefault(x => x.Name?.ToLowerInvariant() == _modProvider.ModLoaderName);
            if (modLoader is null) return;
            await Task.Run(() => _modInstaller.UninstallModAsync(modLoader));
        }

        public async Task UninstallAllModsAsync()
        {
            foreach (ModGridItemViewModel gridItem in GridItems)
                await UninstallModAsync(gridItem);
            _modInstaller.RemoveAllMods();
        }

        private async Task InstallModAsync(ModGridItemViewModel gridItem)
        {
            _progress.Report(gridItem.AvailableMod.Name!);
            bool success = await Task.Run(() => _modInstaller.InstallModAsync(gridItem.AvailableMod));
            if (!success) return;
            gridItem.InstalledMod = gridItem.AvailableMod;
        }

        private async Task UninstallModAsync(ModGridItemViewModel gridItem)
        {
            if (gridItem.InstalledMod is null) return;
            _progress.Report(gridItem.InstalledMod.Name!);
            bool success = await Task.Run(() => _modInstaller.UninstallModAsync(gridItem.InstalledMod));
            if (!success) return;
            gridItem.InstalledMod = null;
        }

        private void OnCheckboxUpdated(ModGridItemViewModel gridItem)
        {
            if (gridItem.IsCheckBoxChecked) _modProvider.ResolveDependencies(gridItem.AvailableMod);
            else _modProvider.UnresolveDependencies(gridItem.AvailableMod);
            foreach (ModGridItemViewModel modGridItem in GridItems)
            {
                bool isDependency = modGridItem.AvailableMod.Required || (_modProvider.Dependencies.TryGetValue(modGridItem.AvailableMod, out HashSet<IMod>? dependants) && dependants.Count != 0);
                modGridItem.IsCheckBoxEnabled = !isDependency;
                modGridItem.IsCheckBoxChecked = modGridItem.IsCheckBoxChecked || isDependency;
            }
        }
    }
}