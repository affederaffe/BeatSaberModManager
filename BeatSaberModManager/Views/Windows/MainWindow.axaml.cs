using System;
using System.Linq;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

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
            ArgumentNullException.ThrowIfNull(viewModel);
            InitializeComponent();
            ViewModel = viewModel;
            ExtendClientAreaToDecorationsHint = !OperatingSystem.IsLinux();
            viewModel.PickInstallDirInteraction.RegisterHandler(async context => context.SetOutput(await new InstallFolderDialogWindow().ShowDialog<string?>(this).ConfigureAwait(false)));
        }
    }
}
