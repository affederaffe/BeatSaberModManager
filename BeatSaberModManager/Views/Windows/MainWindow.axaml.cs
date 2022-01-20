using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Windows
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow() { }

        [ActivatorUtilitiesConstructor]
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            this.Bind(viewModel, static vm => vm.AppSettings.Value.LastTabIndex, static v => v.TabControl.SelectedIndex);
            this.WhenActivated(disposable => viewModel.AppSettings.Value.InstallDir.Changed
                .Where(static x => x is null)
                .SelectMany(_ => new InstallFolderDialogWindow().ShowDialog<string?>(this))
                .BindTo(ViewModel.SettingsViewModel, static x => x.InstallDir)
                .DisposeWith(disposable));
            this.AttachDevTools();
        }
    }
}