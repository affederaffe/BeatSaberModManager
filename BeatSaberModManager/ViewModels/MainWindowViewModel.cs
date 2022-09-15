using System.Reactive;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="BeatSaberModManager.Views.Windows.MainWindow"/>.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ISettings<AppSettings> _appSettings;
        private readonly ObservableAsPropertyHelper<ProgressInfo> _progressInfo;
        private readonly ObservableAsPropertyHelper<double> _progressValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel(ISettings<AppSettings> appSettings, DashboardViewModel dashboardViewModel, ModsViewModel modsViewModel, SettingsViewModel settingsViewModel, IStatusProgress progress)
        {
            _appSettings = appSettings;
            DashboardViewModel = dashboardViewModel;
            ModsViewModel = modsViewModel;
            SettingsViewModel = settingsViewModel;
            InstallCommand = ReactiveCommand.CreateFromTask(modsViewModel.RefreshModsAsync, modsViewModel.IsSuccessObservable);
            MoreInfoCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(modsViewModel.SelectedGridItem!.AvailableMod.MoreInfoLink), modsViewModel.WhenAnyValue(static x => x.SelectedGridItem).Select(static x => x?.AvailableMod.MoreInfoLink is not null));
            StatusProgress statusProgress = (StatusProgress)progress;
            statusProgress.ProgressInfo.ToProperty(this, nameof(ProgressInfo), out _progressInfo, scheduler: RxApp.MainThreadScheduler);
            statusProgress.ProgressValue.ToProperty(this, nameof(ProgressValue), out _progressValue, scheduler: RxApp.MainThreadScheduler);
        }

        /// <summary>
        /// The ViewModel for a dashboard view.
        /// </summary>
        public DashboardViewModel DashboardViewModel { get; }

        /// <summary>
        /// The ViewModel for a mods view.
        /// </summary>
        public ModsViewModel ModsViewModel { get; }

        /// <summary>
        /// The ViewModel for a settings view.
        /// </summary>
        public SettingsViewModel SettingsViewModel { get; }

        /// <summary>
        /// Invokes <see cref="BeatSaberModManager.ViewModels.ModsViewModel.RefreshModsAsync"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InstallCommand { get; }

        /// <summary>
        /// Opens the <see cref="BeatSaberModManager.Models.Interfaces.IMod.MoreInfoLink"/> of the <see cref="BeatSaberModManager.ViewModels.ModsViewModel.SelectedGridItem"/>
        /// </summary>
        public ReactiveCommand<Unit, bool> MoreInfoCommand { get; }

        /// <summary>
        /// The current information of the operation.
        /// </summary>
        public ProgressInfo ProgressInfo => _progressInfo.Value;

        /// <summary>
        /// The current progress of the operation.
        /// </summary>
        public double ProgressValue => _progressValue.Value;

        /// <inheritdoc cref="AppSettings.LastTabIndex" />
        public int LastTabIndex
        {
            get => _appSettings.Value.LastTabIndex;
            set => _appSettings.Value.LastTabIndex = value;
        }
    }
}
