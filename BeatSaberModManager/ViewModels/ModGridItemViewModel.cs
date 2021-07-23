using System;

using BeatSaberModManager.Models.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class ModGridItemViewModel : ReactiveObject
    {
        public ModGridItemViewModel(IMod availableMod, IMod? installedMod)
        {
            AvailableMod = availableMod;
            InstalledMod = installedMod;
        }

        public IMod AvailableMod { get; }

        public IDisposable? Subscription { get; set; }

        private IMod? _installedMod;
        public IMod? InstalledMod
        {
            get => _installedMod;
            set => this.RaiseAndSetIfChanged(ref _installedMod, value);
        }

        private bool _isCheckBoxEnabled;
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
    }
}