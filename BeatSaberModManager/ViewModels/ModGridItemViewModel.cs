using System;
using System.Collections.Generic;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;

using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class ModGridItemViewModel : ViewModelBase
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly ObservableAsPropertyHelper<bool> _isUpToDate;

        public ModGridItemViewModel(IMod availableMod, IMod? installedMod, IOptions<AppSettings> appSettings, IDependencyResolver dependencyResolver, IVersionComparer versionComparer)
        {
            _availableMod = availableMod;
            _installedMod = installedMod;
            _appSettings = appSettings;
            _dependencyResolver = dependencyResolver;
            _isCheckBoxChecked = installedMod is not null || _appSettings.Value.SelectedMods.Contains(availableMod.Name);
            this.WhenAnyValue(x => x.AvailableMod, x => x.InstalledMod)
                .Select(x => versionComparer.CompareVersions(x.Item1.Version, x.Item2?.Version) >= 0)
                .ToProperty(this, nameof(IsUpToDate), out _isUpToDate);
        }

        public bool IsUpToDate => _isUpToDate.Value;

        private IMod _availableMod;
        public IMod AvailableMod
        {
            get => _availableMod;
            set => this.RaiseAndSetIfChanged(ref _availableMod, value);
        }

        private IMod? _installedMod;
        public IMod? InstalledMod
        {
            get => _installedMod;
            set => this.RaiseAndSetIfChanged(ref _installedMod, value);
        }

        private bool _isCheckBoxEnabled = true;
        public bool IsCheckBoxEnabled
        {
            get => _isCheckBoxEnabled;
            set => this.RaiseAndSetIfChanged(ref _isCheckBoxEnabled, value);
        }

        private bool _isCheckBoxChecked;
        public bool IsCheckBoxChecked
        {
            get => _isCheckBoxChecked;
            set => this.RaiseAndSetIfChanged(ref _isCheckBoxChecked, value);
        }

        public void Initialize(Dictionary<IMod, ModGridItemViewModel> gridItems)
        {
            IObservable<(ModGridItemViewModel gridItem, bool isDependency)> affectedGridItemObservable = this.WhenAnyValue(x => x.IsCheckBoxChecked)
                .SelectMany(OnCheckboxUpdated)
                .Select(gridItems.GetValueOrDefault)
                .WhereNotNull()
                .Select(x => (gridItem: x, _dependencyResolver.IsDependency(x.AvailableMod)));
            affectedGridItemObservable.Subscribe(x => x.gridItem.IsCheckBoxEnabled = !x.isDependency);
            affectedGridItemObservable.Subscribe(x => x.gridItem.IsCheckBoxChecked = x.isDependency || x.gridItem.IsCheckBoxChecked && x.gridItem.InstalledMod is not null);
        }

        private IEnumerable<IMod> OnCheckboxUpdated(bool checkBoxChecked)
        {
            if (checkBoxChecked)
            {
                _appSettings.Value.SelectedMods.Add(AvailableMod.Name);
                return _dependencyResolver.ResolveDependencies(AvailableMod);
            }

            _appSettings.Value.SelectedMods.Remove(AvailableMod.Name);
            return _dependencyResolver.UnresolveDependencies(AvailableMod);
        }
    }
}