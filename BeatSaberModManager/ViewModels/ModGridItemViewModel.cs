using System;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class ModGridItemViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<string> _installedVersionColor;
        private readonly ObservableAsPropertyHelper<string> _installedVersionTextDecoration;

        public ModGridItemViewModel(IModVersionComparer modVersionComparer, IMod availableMod, IMod? installedMod)
        {
            _availableMod = availableMod;
            _installedMod = installedMod;
            this.WhenAnyValue(x => x.InstalledMod)
                .Select(x => modVersionComparer.CompareVersions(AvailableMod.Version, x?.Version) >= 0)
                .BindTo(this, x => x.IsUpToDate);
            IObservable<bool> isUpToDateObservable = this.WhenAnyValue(x => x.IsUpToDate);
            isUpToDateObservable.Select(x => x ? "Green" : "Red")
                .ToProperty(this, nameof(InstalledVersionColor), out _installedVersionColor);
            isUpToDateObservable.Select(x => x ? "None" : "Strikethrough")
                .ToProperty(this, nameof(InstalledVersionTextDecoration), out _installedVersionTextDecoration);
        }

        public IDisposable? Subscription { get; set; }

        public string InstalledVersionColor => _installedVersionColor.Value;

        public string InstalledVersionTextDecoration => _installedVersionTextDecoration.Value;

        private bool _isUpToDate;
        public bool IsUpToDate
        {
            get => _isUpToDate;
            set => this.RaiseAndSetIfChanged(ref _isUpToDate, value);
        }

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