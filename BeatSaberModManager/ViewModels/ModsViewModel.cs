using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
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
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IModProvider _modProvider;
        private readonly IModInstaller _modInstaller;
        private readonly IModVersionComparer _modVersionComparer;

        public ModsViewModel(ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IDependencyResolver dependencyResolver, IModProvider modProvider, IModInstaller modInstaller, IModVersionComparer modVersionComparer)
        {
            _appSettings = appSettings;
            _installDirValidator = installDirValidator;
            _dependencyResolver = dependencyResolver;
            _modProvider = modProvider;
            _modInstaller = modInstaller;
            _modVersionComparer = modVersionComparer;
            ReactiveCommand<Unit, Unit> initializeCommand = ReactiveCommand.CreateFromTask(InitializeDataGridAsync);
            _appSettings.Value.InstallDir.Changed.Where(installDirValidator.ValidateInstallDir).Select(_ => Unit.Default).InvokeCommand(initializeCommand);
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

        private async Task InitializeDataGridAsync()
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value))
            {
                IsFailed = true;
                return;
            }

            IsLoading = true;
            await Task.WhenAll(
                Task.Run(() => _modProvider.LoadAvailableModsForCurrentVersionAsync(_appSettings.Value.InstallDir.Value!)),
                Task.Run(() => _modProvider.LoadInstalledModsAsync(_appSettings.Value.InstallDir.Value!))
            ).ConfigureAwait(false);
            IsSuccess = _modProvider.AvailableMods?.Length > 0;
            IsFailed = !IsSuccess;
            IsLoading = false;
            if (_modProvider.AvailableMods is null) return;
            foreach (IMod availableMod in _modProvider.AvailableMods)
            {
                IMod? installedMod = _modProvider.InstalledMods?.FirstOrDefault(x => x.Name == availableMod.Name);
                ModGridItemViewModel gridItem = new(_modVersionComparer, availableMod, installedMod);
                GridItems.Add(availableMod, gridItem);
                gridItem.IsCheckBoxChecked = gridItem.InstalledMod is not null || _appSettings.Value.SelectedMods.Contains(availableMod.Name);
            }

            foreach (ModGridItemViewModel gridItem in GridItems.Values)
                gridItem.WhenAnyValue(x => x.IsCheckBoxChecked).Subscribe(_ => OnCheckboxUpdated(gridItem));
        }

        public async Task RefreshModsAsync()
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value)) return;
            IEnumerable<IMod> install = GridItems.Values.Where(x => x.IsCheckBoxChecked && (!x.IsUpToDate || _appSettings.Value.ForceReinstallMods)).Select(x => x.AvailableMod);
            await InstallMods(_appSettings.Value.InstallDir.Value!, install).ConfigureAwait(false);
            IEnumerable<IMod> uninstall = GridItems.Values.Where(x => !x.IsCheckBoxChecked && x.InstalledMod is not null).Select(x => x.AvailableMod);
            await UninstallMods(_appSettings.Value.InstallDir.Value!, uninstall).ConfigureAwait(false);
        }

        public async Task UninstallModLoaderAsync()
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value)) return;
            IMod? modLoader = GridItems.Values.FirstOrDefault(x => x.InstalledMod?.Name.ToLowerInvariant() == _modProvider.ModLoaderName)?.AvailableMod;
            if (modLoader is null) return;
            await UninstallMods(_appSettings.Value.InstallDir.Value!, new[] { modLoader }).ConfigureAwait(false);
        }

        public async Task UninstallAllModsAsync()
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value)) return;
            IEnumerable<IMod> mods = GridItems.Values.Where(x => x.InstalledMod is not null).Select(x => x.AvailableMod);
            await UninstallMods(_appSettings.Value.InstallDir.Value!, mods).ConfigureAwait(false);
            _modInstaller.RemoveAllMods(_appSettings.Value.InstallDir.Value!);
        }

        private async Task InstallMods(string installDir, IEnumerable<IMod> mods)
        {
            await foreach (IMod mod in _modInstaller.InstallModsAsync(installDir, mods).ConfigureAwait(false))
                GridItems[mod].InstalledMod = mod;
        }

        private async Task UninstallMods(string installDir, IEnumerable<IMod> mods)
        {
            await foreach (IMod mod in _modInstaller.UninstallModsAsync(installDir, mods).ConfigureAwait(false))
                GridItems[mod].InstalledMod = null;
        }

        private void OnCheckboxUpdated(ModGridItemViewModel gridItem)
        {
            if (gridItem.IsCheckBoxChecked)
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
                ModGridItemViewModel gridItem = GridItems[mod];
                bool isDependency = _dependencyResolver.IsDependency(gridItem.AvailableMod);
                gridItem.IsCheckBoxEnabled = !isDependency;
                gridItem.IsCheckBoxChecked = gridItem.IsCheckBoxChecked || isDependency;
            }
        }
    }
}