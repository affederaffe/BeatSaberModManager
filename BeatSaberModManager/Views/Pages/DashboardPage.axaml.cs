using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.ViewModels;

using ReactiveUI;


namespace BeatSaberModManager.Views.Pages
{
    /// <summary>
    /// View for additional information and tools.
    /// </summary>
    public partial class DashboardPage : ReactiveUserControl<DashboardViewModel>
    {
        private FilePickerOpenOptions? _filePickerOpenOptions;
        private FilePickerOpenOptions FilePickerOpenOptions => _filePickerOpenOptions ??= new FilePickerOpenOptions
        {
            FileTypeFilter = new[]
            {
                new FilePickerFileType("BeatSaber Playlist")
                {
                    Patterns = new [] { "*.bpllist" }
                }
            }
        };

        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public DashboardPage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardPage"/> class.
        /// </summary>
        public DashboardPage(DashboardViewModel viewModel, Window window)
        {
            InitializeComponent();
            ViewModel = viewModel;
            ReactiveCommand<Unit, IReadOnlyList<IStorageFile>> showFileDialogCommand = ReactiveCommand.CreateFromTask(() => window.StorageProvider.OpenFilePickerAsync(FilePickerOpenOptions), viewModel.ModsViewModel.IsInstallDirValidObservable);
            showFileDialogCommand.Where(static x => x.Count == 1)
                .Select(static x => x[0].TryGetUri(out Uri? uri) ? uri : null)
                .WhereNotNull()
                .Select(static x => x.AbsolutePath)
                .SelectMany(ViewModel.InstallPlaylistAsync)
                .Select(static x => new ProgressInfo(x ? StatusType.Completed : StatusType.Failed, null))
                .Subscribe(ViewModel.StatusProgress.Report);
            InstallPlaylistButton.Command = showFileDialogCommand;
        }
    }
}
