using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

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
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            ExtendClientAreaToDecorationsHint = !OperatingSystem.IsLinux();
            TransparencyLevelHint = OperatingSystem.IsWindowsVersionAtLeast(11) ? WindowTransparencyLevel.Mica : WindowTransparencyLevel.Blur;
            Margin = ExtendClientAreaToDecorationsHint ? WindowDecorationMargin : new Thickness();
            this.WhenActivated(disposable => viewModel.SettingsViewModel.WhenAnyValue(static x => x.InstallDir)
                .FirstAsync()
                .Where(static x => x is null)
                .SelectMany(_ => new InstallFolderDialogWindow().ShowDialog<string?>(this))
                .Subscribe(x => viewModel.SettingsViewModel.InstallDir = x)
                .DisposeWith(disposable));
        }
    }
}
