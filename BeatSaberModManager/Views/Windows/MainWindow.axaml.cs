using System;
using System.Collections.Generic;

using Avalonia.Labs.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Dialogs;


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
            viewModel.PickInstallDirInteraction.RegisterHandler(async context =>
            {
                ContentDialogResult result = await new PickInstallDirDialog().Dialog.ShowAsync(this).ConfigureAwait(false);
                string? installDir = null;
                if (result == ContentDialogResult.Primary)
                {
                    IReadOnlyList<IStorageFolder> folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false }).ConfigureAwait(true);
                    if (folders.Count == 1)
                        installDir = folders[0].Path.LocalPath;
                }

                context.SetOutput(installDir);
            });
        }
    }
}
