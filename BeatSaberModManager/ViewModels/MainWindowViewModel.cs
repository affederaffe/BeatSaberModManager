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
            this.WhenActivated(disposable =>
            {
                settingsViewModel.WhenAnyValue(static x => x.GameVersion)
                    .Skip(1)
                    .FirstAsync()
                    .Where(static x => x?.InstallDir is null)
                    .Select(static _ => Unit.Default)
                    .InvokeCommand(settingsViewModel.PickGameVersionCommand)
                    .DisposeWith(disposable);
                settingsViewModel.ValidatedGameVersionObservable.InvokeCommand(modsViewModel.InitializeCommand).DisposeWith(disposable);
                legacyGameVersionsViewModel.InitializeCommand.Execute().Subscribe().DisposeWith(disposable);
            });
        }

        /// <inheritdoc />
        public ViewModelActivator Activator { get; } = new();

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
    }
}
