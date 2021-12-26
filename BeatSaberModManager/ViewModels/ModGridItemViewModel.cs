using System.Reactive.Linq;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class ModGridItemViewModel : ViewModelBase
    {
        private readonly ObservableAsPropertyHelper<bool> _isUpToDate;

        public ModGridItemViewModel(IModVersionComparer modVersionComparer, IMod availableMod, IMod? installedMod)
        {
            _availableMod = availableMod;
            _installedMod = installedMod;
            this.WhenAnyValue(x => x.InstalledMod).Select(x => modVersionComparer.CompareVersions(AvailableMod.Version, x?.Version) >= 0).ToProperty(this, nameof(IsUpToDate), out _isUpToDate);
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
    }
}