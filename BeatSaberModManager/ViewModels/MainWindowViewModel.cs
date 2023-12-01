using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="BeatSaberModManager.Views.Windows.MainWindow"/>.
    /// </summary>
    public sealed class MainWindowViewModel : ViewModelBase, IActivatableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel(DashboardViewModel dashboardViewModel, ModsViewModel modsViewModel, LegacyGameVersionsViewModel legacyGameVersionsViewModel, SettingsViewModel settingsViewModel)
        {
            ArgumentNullException.ThrowIfNull(modsViewModel);
            DashboardViewModel = dashboardViewModel;
            ModsViewModel = modsViewModel;
            LegacyGameVersionsViewModel = legacyGameVersionsViewModel;
            SettingsViewModel = settingsViewModel;
            Activator = new ViewModelActivator();
            PickInstallDirInteraction = new Interaction<Unit, string?>();
            this.WhenActivated(disposable =>
            {
                settingsViewModel.WhenAnyValue(static x => x.InstallDir)
                    .FirstAsync()
                    .Where(static x => x is null)
                    .SelectMany(PickInstallDirInteraction.Handle(Unit.Default))
                    .Subscribe(x => settingsViewModel.InstallDir = x)
                    .DisposeWith(disposable);
                settingsViewModel.ValidatedInstallDirObservable.InvokeCommand(modsViewModel.InitializeCommand).DisposeWith(disposable);
                legacyGameVersionsViewModel.InitializeCommand.Execute().Subscribe().DisposeWith(disposable);
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
        /// The ViewModel for a legacy game versions view.
        /// </summary>
        public LegacyGameVersionsViewModel LegacyGameVersionsViewModel { get; }

        /// <summary>
        /// The ViewModel for a settings view.
        /// </summary>
        public SettingsViewModel SettingsViewModel { get; }

        /// <summary>
        /// Ask the user to pick an installation directory.
        /// </summary>
        public Interaction<Unit, string?> PickInstallDirInteraction { get; }
    }
}
