using System;

using Avalonia;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;


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
            Margin = ExtendClientAreaToDecorationsHint ? WindowDecorationMargin : new Thickness();
            viewModel.PickInstallDirInteraction.RegisterHandler(async context => context.SetOutput(await new InstallFolderDialogWindow().ShowDialog<string?>(this)));
        }
    }
}
