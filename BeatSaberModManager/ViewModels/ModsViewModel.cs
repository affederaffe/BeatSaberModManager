using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Progress;
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
        private readonly IStatusProgress _progress;
        private readonly ObservableAsPropertyHelper<bool> _noModsTextVisible;

        public ModsViewModel(ISettings<AppSettings> appSettings, IModProvider modProvider, IModInstaller modInstaller, IModVersionComparer modVersionComparer, IStatusProgress progress)
        {
            _appSettings = appSettings.Value;
            _modProvider = modProvider;
            _modInstaller = modInstaller;
            _modVersionComparer = modVersionComparer;
            _progress = progress;
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
            ModGridItemViewModel[] modsToInstall = GridItems.Values.Where(x => x.IsCheckBoxChecked && !x.IsUpToDate).ToArray();
            ModGridItemViewModel[] modsToUninstall = GridItems.Values.Where(x => !x.IsCheckBoxChecked && x.InstalledMod is not null).ToArray();
            int sum = modsToInstall.Length + modsToUninstall.Length;

            _progress.Report(0.0);
            for (int i = 0; i < modsToInstall.Length; i++)
            {
                await InstallModAsync(modsToInstall[i]);
                _progress.Report(((double)i + 1) / sum);
            }

            for (int i = 0; i < modsToUninstall.Length; i++)
            {
                await UninstallModAsync(modsToUninstall[i]);
                _progress.Report(((double)i + modsToInstall.Length + 1 ) / sum);
            }
        }

        public async Task UninstallModLoaderAsync()
        {
            ModGridItemViewModel? gridItem = GridItems.Values.FirstOrDefault(x => x.InstalledMod?.Name.ToLowerInvariant() == _modProvider.ModLoaderName);
            if (gridItem is null) return;
            await UninstallModAsync(gridItem);
        }

        public async Task UninstallAllModsAsync()
        {
            foreach (ModGridItemViewModel gridItem in GridItems.Values)
                await UninstallModAsync(gridItem);
            _modInstaller.RemoveAllMods();
        }

        private async Task InstallModAsync(ModGridItemViewModel gridItem)
        {
            _progress.Report(ProgressBarStatusType.Installing);
            _progress.Report(gridItem.AvailableMod.Name);
            bool success = await _modInstaller.InstallModAsync(gridItem.AvailableMod);
            if (!success) return;
            gridItem.InstalledMod = gridItem.AvailableMod;
        }

        private async Task UninstallModAsync(ModGridItemViewModel gridItem)
        {
            if (gridItem.InstalledMod is null) return;
            _progress.Report(ProgressBarStatusType.Uninstalling);
            _progress.Report(gridItem.InstalledMod.Name);
            bool success = await _modInstaller.UninstallModAsync(gridItem.InstalledMod);
            if (!success) return;
            gridItem.InstalledMod = null;
        }

        private void OnCheckboxUpdated(ModGridItemViewModel gridItem)
        {
            if (gridItem.IsCheckBoxChecked)
            {
                _appSettings.SelectedMods.Add(gridItem.AvailableMod.Name);
                foreach (IMod mod in _modProvider.ResolveDependencies(gridItem.AvailableMod))
                    UpdateDependency(GridItems[mod]);
            }
            else
            {
                _appSettings.SelectedMods.Remove(gridItem.AvailableMod.Name);
                foreach (IMod mod in _modProvider.UnresolveDependencies(gridItem.AvailableMod))
                    UpdateDependency(GridItems[mod]);
            }
        }

        private void UpdateDependency(ModGridItemViewModel gridItem)
        {
            bool isDependency = _modProvider.Dependencies.TryGetValue(gridItem.AvailableMod, out HashSet<IMod>? dependents) && dependents.Count != 0;
            gridItem.IsCheckBoxEnabled = !isDependency;
            gridItem.IsCheckBoxChecked = gridItem.IsCheckBoxChecked || isDependency;
        }
    }
}