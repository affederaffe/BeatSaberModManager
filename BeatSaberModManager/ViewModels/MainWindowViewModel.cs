using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="BeatSaberModManager.Views.Windows.MainWindow"/>.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase, IActivatableViewModel
    {
        private readonly ObservableAsPropertyHelper<ProgressInfo> _progressInfo;
        private readonly ObservableAsPropertyHelper<double> _progressValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel(DashboardViewModel dashboardViewModel, ModsViewModel modsViewModel, SettingsViewModel settingsViewModel, StatusProgress statusProgress)
        {
            DashboardViewModel = dashboardViewModel;
            ModsViewModel = modsViewModel;
            SettingsViewModel = settingsViewModel;
            Activator = new ViewModelActivator();
            PickInstallDirInteraction = new Interaction<Unit, string?>();
            InstallCommand = ReactiveCommand.CreateFromTask(modsViewModel.UpdateModsAsync, modsViewModel.IsSuccessObservable);
            IObservable<bool> canOpenMoreInfoLink = modsViewModel.WhenAnyValue(static x => x.SelectedGridItem).Select(static x => x?.AvailableMod.MoreInfoLink is not null);
            MoreInfoCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(modsViewModel.SelectedGridItem!.AvailableMod.MoreInfoLink), canOpenMoreInfoLink);
            statusProgress.ProgressInfo.ToProperty(this, nameof(ProgressInfo), out _progressInfo, scheduler: RxApp.MainThreadScheduler);
            statusProgress.ProgressValue.ToProperty(this, nameof(ProgressValue), out _progressValue, scheduler: RxApp.MainThreadScheduler);
            this.WhenActivated(disposable =>
            {
                settingsViewModel.WhenAnyValue(static x => x.InstallDir)
                    .FirstAsync()
                    .Where(static x => x is null)
                    .SelectMany(PickInstallDirInteraction.Handle(Unit.Default))
                    .Subscribe(x => settingsViewModel.InstallDir = x)
                    .DisposeWith(disposable);
                settingsViewModel.ValidatedInstallDirObservable.InvokeCommand(modsViewModel.InitializeCommand).DisposeWith(disposable);
            });
        }

        /// <inheritdoc />
        public ViewModelActivator Activator { get; }

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
        /// Invokes <see cref="ViewModels.ModsViewModel.UpdateModsAsync"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InstallCommand { get; }

        /// <summary>
        /// Opens the <see cref="BeatSaberModManager.Models.Interfaces.IMod.MoreInfoLink"/> of the <see cref="BeatSaberModManager.ViewModels.ModsViewModel.SelectedGridItem"/>
        /// </summary>
        public ReactiveCommand<Unit, bool> MoreInfoCommand { get; }

        /// <summary>
        /// Ask the user to pick an installation directory.
        /// </summary>
        public Interaction<Unit, string?> PickInstallDirInteraction { get; }

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
