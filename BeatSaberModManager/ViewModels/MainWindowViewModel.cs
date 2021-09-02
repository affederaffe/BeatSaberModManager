using System.IO;
using System.Reactive;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Utils;

using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> _moreInfoButtonEnabled;
        private readonly ObservableAsPropertyHelper<bool> _installButtonEnabled;
        private readonly ObservableAsPropertyHelper<double> _progressBarValue;
        private readonly ObservableAsPropertyHelper<string?> _progressBarText;
        private readonly ObservableAsPropertyHelper<ProgressBarStatusType> _progressBarStatusType;

        public MainWindowViewModel(ModsViewModel modsViewModel, IOptions<SettingsStore> settingsStore, IStatusProgress progress)
        {
            _selectedIndex = settingsStore.Value.LastSelectedIndex;
            MoreInfoButtonCommand = ReactiveCommand.Create(() => PlatformUtils.OpenBrowser(modsViewModel.SelectedGridItem?.AvailableMod.MoreInfoLink));
            InstallButtonCommand = ReactiveCommand.CreateFromTask(modsViewModel.RefreshModsAsync);
            this.WhenAnyValue(x => x.SelectedIndex).BindTo(settingsStore.Value, x => x.LastSelectedIndex);
            modsViewModel.WhenAnyValue(x => x.SelectedGridItem)
                .Select(mod => mod is not null)
                .ToProperty(this, nameof(MoreInfoButtonEnabled), out _moreInfoButtonEnabled);
            modsViewModel.WhenAnyValue(x => x.GridItems.Count)
                .Select(x => x > 0 && Directory.Exists(settingsStore.Value.InstallDir))
                .ToProperty(this, nameof(InstallButtonEnabled), out _installButtonEnabled);
            StatusProgress statusProgress = (StatusProgress)progress;
            statusProgress.ProgressValue.ToProperty(this, nameof(ProgressBarValue), out _progressBarValue);
            statusProgress.StatusText.ToProperty(this, nameof(ProgressBarText), out _progressBarText);
            statusProgress.StatusType.ToProperty(this, nameof(ProgressBarStatusType), out _progressBarStatusType);
        }

        public ReactiveCommand<Unit, Unit> MoreInfoButtonCommand { get; }

        public ReactiveCommand<Unit, Unit> InstallButtonCommand { get; }

        public bool MoreInfoButtonEnabled => _moreInfoButtonEnabled.Value;

        public bool InstallButtonEnabled => _installButtonEnabled.Value;

        public double ProgressBarValue => _progressBarValue.Value;

        public string? ProgressBarText => _progressBarText.Value;

        public ProgressBarStatusType ProgressBarStatusType => _progressBarStatusType.Value;

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
        }
    }
}