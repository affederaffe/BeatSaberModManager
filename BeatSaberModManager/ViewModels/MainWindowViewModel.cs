using System.Reactive;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableAsPropertyHelper<bool> _moreInfoButtonEnabled;
        private readonly ObservableAsPropertyHelper<bool> _installButtonEnabled;
        private readonly ObservableAsPropertyHelper<double> _progressBarValue;
        private readonly ObservableAsPropertyHelper<string?> _progressBarText;
        private readonly ObservableAsPropertyHelper<ProgressBarStatusType> _progressBarStatusType;

        public MainWindowViewModel(ISettings<AppSettings> appSettings, ModsViewModel modsViewModel, SettingsViewModel settingsViewModel, IInstallDirValidator installDirValidator, IStatusProgress progress)
        {
            ModsViewModel = modsViewModel;
            SettingsViewModel = settingsViewModel;
            MoreInfoButtonCommand = ReactiveCommand.Create(() => PlatformUtils.OpenBrowser(modsViewModel.SelectedGridItem?.AvailableMod.MoreInfoLink));
            InstallButtonCommand = ReactiveCommand.CreateFromTask(modsViewModel.RefreshModsAsync);
            modsViewModel.WhenAnyValue(x => x.SelectedGridItem)
                .Select(mod => mod is not null)
                .ToProperty(this, nameof(MoreInfoButtonEnabled), out _moreInfoButtonEnabled);
            modsViewModel.WhenAnyValue(x => x.IsSuccess)
                .Select(x => x && installDirValidator.ValidateInstallDir(appSettings.Value.InstallDir.Value))
                .ToProperty(this, nameof(InstallButtonEnabled), out _installButtonEnabled);
            StatusProgress statusProgress = (StatusProgress)progress;
            statusProgress.ProgressValue.ToProperty(this, nameof(ProgressBarValue), out _progressBarValue);
            statusProgress.StatusText.ToProperty(this, nameof(ProgressBarText), out _progressBarText);
            statusProgress.StatusType.ToProperty(this, nameof(ProgressBarStatusType), out _progressBarStatusType);
        }

        public ModsViewModel ModsViewModel { get; }

        public SettingsViewModel SettingsViewModel { get; }

        public ReactiveCommand<Unit, Unit> MoreInfoButtonCommand { get; }

        public ReactiveCommand<Unit, Unit> InstallButtonCommand { get; }

        public bool MoreInfoButtonEnabled => _moreInfoButtonEnabled.Value;

        public bool InstallButtonEnabled => _installButtonEnabled.Value;

        public double ProgressBarValue => _progressBarValue.Value;

        public string? ProgressBarText => _progressBarText.Value;

        public ProgressBarStatusType ProgressBarStatusType => _progressBarStatusType.Value;

        private string? _progressBarStatusText;
        public string? ProgressBarStatusText
        {
            get => _progressBarStatusText;
            set => this.RaiseAndSetIfChanged(ref _progressBarStatusText, value);
        }
    }
}