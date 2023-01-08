using System;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// ViewModel that represents an <see cref="IMod"/> which can be selected an deselected.
    /// </summary>
    public sealed class ModGridItemViewModel : ViewModelBase, IDisposable
    {
        private readonly ObservableAsPropertyHelper<bool> _isUpToDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModGridItemViewModel"/> class.
        /// </summary>
        public ModGridItemViewModel(IMod availableMod, IMod? installedMod, ISettings<AppSettings> appSettings)
        {
            _availableMod = availableMod;
            _installedMod = installedMod;
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

        /// <inheritdoc />
        public void Dispose()
        {
            _isUpToDate.Dispose();
        }
    }
}
