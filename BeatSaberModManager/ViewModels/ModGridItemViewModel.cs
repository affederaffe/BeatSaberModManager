using System;
using System.Collections.Generic;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// ViewModel that represents an <see cref="IMod"/> which can be selected an deselected.
    /// </summary>
    public class ModGridItemViewModel : ViewModelBase, IDisposable
    {
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly ObservableAsPropertyHelper<bool> _isUpToDate;

        private IDisposable? _saveSelectedModSubscription;
        private IDisposable? _resolveDependenciesSubscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModGridItemViewModel"/> class.
        /// </summary>
        public ModGridItemViewModel(IMod availableMod, IMod? installedMod, ISettings<AppSettings> appSettings, IDependencyResolver dependencyResolver)
        {
            _appSettings = appSettings;
            _availableMod = availableMod;
            _installedMod = installedMod;
            _dependencyResolver = dependencyResolver;
            _isCheckBoxChecked = installedMod is not null || availableMod.IsRequired || (appSettings.Value.SaveSelectedMods && appSettings.Value.SelectedMods.Contains(availableMod.Name));
            this.WhenAnyValue(static x => x.AvailableMod, static x => x.InstalledMod)
                .Select(static x => x.Item1.Version.CompareTo(x.Item2?.Version) <= 0)
                .ToProperty(this, nameof(IsUpToDate), out _isUpToDate);
        }

        /// <summary>
        /// Compares the available version to the installed one.
        /// </summary>
        public bool IsUpToDate => _isUpToDate.Value;

        /// <summary>
        /// The <see cref="IMod"/> available in the repository.
        /// </summary>
        public IMod AvailableMod
        {
            get => _availableMod;
            set => this.RaiseAndSetIfChanged(ref _availableMod, value);
        }

        private IMod _availableMod;

        /// <summary>
        /// The currently installed version of the mod.
        /// </summary>
        public IMod? InstalledMod
        {
            get => _installedMod;
            set => this.RaiseAndSetIfChanged(ref _installedMod, value);
        }

        private IMod? _installedMod;

        /// <summary>
        /// Enables or disables the checkbox control.
        /// </summary>
        public bool IsCheckBoxEnabled
        {
            get => _isCheckBoxEnabled;
            set => this.RaiseAndSetIfChanged(ref _isCheckBoxEnabled, value);
        }

        private bool _isCheckBoxEnabled = true;

        /// <summary>
        /// Checks or unchecks the checkbox control.
        /// </summary>
        public bool IsCheckBoxChecked
        {
            get => _isCheckBoxChecked;
            set => this.RaiseAndSetIfChanged(ref _isCheckBoxChecked, value);
        }

        private bool _isCheckBoxChecked;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gridItems"></param>
        public void Initialize(Dictionary<IMod, ModGridItemViewModel> gridItems)
        {
            IObservable<bool> whenAnyCheckBoxCheckedObservable = this.WhenAnyValue(static x => x.IsCheckBoxChecked);
            _saveSelectedModSubscription = whenAnyCheckBoxCheckedObservable.Subscribe(x => _ = x ? _appSettings.Value.SelectedMods.Add(AvailableMod.Name) : _appSettings.Value.SelectedMods.Remove(AvailableMod.Name));
            _resolveDependenciesSubscription = whenAnyCheckBoxCheckedObservable.SelectMany(x => x ? _dependencyResolver.ResolveDependencies(AvailableMod) : _dependencyResolver.UnresolveDependencies(AvailableMod))
                .Select(gridItems.GetValueOrDefault)
                .WhereNotNull()
                .Subscribe(x =>
                {
                    bool isDependency = _dependencyResolver.IsDependency(x.AvailableMod);
                    x.IsCheckBoxEnabled = !isDependency;
                    x.IsCheckBoxChecked = isDependency || (x.IsCheckBoxChecked && x.InstalledMod is not null);
                });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _isUpToDate.Dispose();
            _saveSelectedModSubscription?.Dispose();
            _resolveDependenciesSubscription?.Dispose();
        }
    }
}
