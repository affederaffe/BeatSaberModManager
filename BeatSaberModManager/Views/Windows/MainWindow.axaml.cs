using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Windows
{
    /// <summary>
    /// Standard top-level view of the application.
    /// </summary>
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public MainWindow() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        [ActivatorUtilitiesConstructor]
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            this.Bind(viewModel, static vm => vm.AppSettings.Value.LastTabIndex, static v => v.TabControl.SelectedIndex);
            this.WhenActivated(disposable => viewModel.AppSettings.Value.WhenAnyValue(static x => x.InstallDir)
                .Where(static x => x is null)
                .SelectMany(_ => new InstallFolderDialogWindow().ShowDialog<string?>(this))
                .BindTo(viewModel.SettingsViewModel, static x => x.AppSettings.Value.InstallDir)
                .DisposeWith(disposable));
        }
    }
}