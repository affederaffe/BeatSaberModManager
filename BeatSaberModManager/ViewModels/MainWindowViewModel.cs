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
    /// <summary>
    /// ViewModel for <see cref="BeatSaberModManager.Views.Windows.MainWindow"/>.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableAsPropertyHelper<ProgressInfo> _progressInfo;
        private readonly ObservableAsPropertyHelper<double> _progressValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel(DashboardViewModel dashboardViewModel, ModsViewModel modsViewModel, SettingsViewModel settingsViewModel, IOptions<AppSettings> appSettings, IStatusProgress progress)
        {
            DashboardViewModel = dashboardViewModel;
            ModsViewModel = modsViewModel;
            SettingsViewModel = settingsViewModel;
            AppSettings = appSettings;
            InstallCommand = ReactiveCommand.CreateFromTask(modsViewModel.RefreshModsAsync, modsViewModel.WhenAnyValue(static x => x.IsSuccess));
            MoreInfoCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(modsViewModel.SelectedGridItem?.AvailableMod.MoreInfoLink!), modsViewModel.WhenAnyValue(static x => x.SelectedGridItem).Select(static x => x?.AvailableMod.MoreInfoLink is not null));
            StatusProgress statusProgress = (StatusProgress)progress;
            statusProgress.ProgressInfo.ToProperty(this, nameof(ProgressInfo), out _progressInfo);
            statusProgress.ProgressValue.ToProperty(this, nameof(ProgressValue), out _progressValue);
        }

        /// <summary>
        /// Exposed for the view.
        /// </summary>
        public DashboardViewModel DashboardViewModel { get; }

        /// <summary>
        /// Exposed for the view.
        /// </summary>
        public ModsViewModel ModsViewModel { get; }

        /// <summary>
        /// Exposed for the view.
        /// </summary>
        public SettingsViewModel SettingsViewModel { get; }

        /// <summary>
        /// Exposed for the view.
        /// </summary>
        public IOptions<AppSettings> AppSettings { get; }

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
    }
}