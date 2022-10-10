using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
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
        public MainWindow(MainWindowViewModel viewModel, ISettings<AppSettings> appSettings)
        {
            InitializeComponent();
            ViewModel = viewModel;
            ExtendClientAreaToDecorationsHint = !OperatingSystem.IsLinux();
            HamburgerMenu.SelectedIndex = appSettings.Value.TabIndex;
            HamburgerMenu.GetObservable(SelectingItemsControl.SelectedIndexProperty).Subscribe(x => appSettings.Value.TabIndex = x);
            this.WhenActivated(disposable => viewModel.SettingsViewModel.WhenAnyValue(static x => x.InstallDir)
                .FirstAsync()
                .Where(static x => x is null)
                .SelectMany(_ => new InstallFolderDialogWindow().ShowDialog<string?>(this))
                .Subscribe(x => viewModel.SettingsViewModel.InstallDir = x)
                .DisposeWith(disposable));
        }
    }
}
