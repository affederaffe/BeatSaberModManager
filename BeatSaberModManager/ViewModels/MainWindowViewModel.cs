using System.Reactive;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;

using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableAsPropertyHelper<bool> _moreInfoButtonEnabled;
        private readonly ObservableAsPropertyHelper<bool> _installButtonEnabled;
        private readonly ObservableAsPropertyHelper<ProgressInfo> _progressInfo;
        private readonly ObservableAsPropertyHelper<double> _progressValue;

        public MainWindowViewModel(ModsViewModel modsViewModel, SettingsViewModel settingsViewModel, IOptions<AppSettings> appSettings, IStatusProgress progress)
        {
            ModsViewModel = modsViewModel;
            SettingsViewModel = settingsViewModel;
            AppSettings = appSettings;
            InstallButtonCommand = ReactiveCommand.CreateFromTask(modsViewModel.RefreshModsAsync);
            modsViewModel.WhenAnyValue(static x => x.SelectedGridItem)
                .Select(static x => x?.AvailableMod.MoreInfoLink is not null)
                .ToProperty(this, nameof(MoreInfoButtonEnabled), out _moreInfoButtonEnabled);
            modsViewModel.WhenAnyValue(static x => x.IsSuccess)
                .ToProperty(this, nameof(InstallButtonEnabled), out _installButtonEnabled);
            StatusProgress statusProgress = (StatusProgress)progress;
            statusProgress.ProgressInfo.ToProperty(this, nameof(ProgressInfo), out _progressInfo);
            statusProgress.ProgressValue.ToProperty(this, nameof(ProgressValue), out _progressValue);
        }

        public ModsViewModel ModsViewModel { get; }

        public SettingsViewModel SettingsViewModel { get; }

        public IOptions<AppSettings> AppSettings { get; }

        public ReactiveCommand<Unit, Unit> InstallButtonCommand { get; }

        public bool MoreInfoButtonEnabled => _moreInfoButtonEnabled.Value;

        public bool InstallButtonEnabled => _installButtonEnabled.Value;

        public ProgressInfo ProgressInfo => _progressInfo.Value;

        public double ProgressValue => _progressValue.Value;
    }
}