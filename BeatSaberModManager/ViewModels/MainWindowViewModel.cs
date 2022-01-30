using System.Reactive;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;

using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableAsPropertyHelper<ProgressInfo> _progressInfo;
        private readonly ObservableAsPropertyHelper<double> _progressValue;

        public MainWindowViewModel(DashboardViewModel dashboardViewModel, ModsViewModel modsViewModel, SettingsViewModel settingsViewModel, IOptions<AppSettings> appSettings, IStatusProgress progress)
        {
            DashboardViewModel = dashboardViewModel;
            ModsViewModel = modsViewModel;
            SettingsViewModel = settingsViewModel;
            AppSettings = appSettings;
            InstallCommand = ReactiveCommand.CreateFromTask(modsViewModel.RefreshModsAsync, modsViewModel.IsSuccessObservable);
            MoreInfoCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(modsViewModel.SelectedGridItem?.AvailableMod.MoreInfoLink!), modsViewModel.WhenAnyValue(static x => x.SelectedGridItem).Select(static x => x?.AvailableMod.MoreInfoLink is not null));
            StatusProgress statusProgress = (StatusProgress)progress;
            statusProgress.ProgressInfo.ToProperty(this, nameof(ProgressInfo), out _progressInfo);
            statusProgress.ProgressValue.ToProperty(this, nameof(ProgressValue), out _progressValue);
        }

        public DashboardViewModel DashboardViewModel { get; }

        public ModsViewModel ModsViewModel { get; }

        public SettingsViewModel SettingsViewModel { get; }

        public IOptions<AppSettings> AppSettings { get; }

        public ReactiveCommand<Unit, Unit> InstallCommand { get; }

        public ReactiveCommand<Unit, Unit> MoreInfoCommand { get; }

        public ProgressInfo ProgressInfo => _progressInfo.Value;

        public double ProgressValue => _progressValue.Value;
    }
}