using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;


namespace BeatSaberModManager.Views.Pages
{
    /// <summary>
    /// View for additional information and tools.
    /// </summary>
    public partial class DashboardPage : ReactiveUserControl<DashboardViewModel>
    {
        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public DashboardPage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardPage"/> class.
        /// </summary>
        public DashboardPage(DashboardViewModel viewModel, Window window)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            InitializeComponent();
            ViewModel = viewModel;
            viewModel.PickPlaylistInteraction.RegisterHandler(async context => context.SetOutput(await SelectPlaylistFileAsync(window).ConfigureAwait(false)));
        }

        private FilePickerOpenOptions? _filePickerOpenOptions;
        private FilePickerOpenOptions FilePickerOpenOptions => _filePickerOpenOptions ??= new FilePickerOpenOptions
        {
            FileTypeFilter =
            [
                new FilePickerFileType("BeatSaber Playlist")
                {
                    Patterns = ["*.bplist"]
                }
            ]
        };

        private async Task<string?> SelectPlaylistFileAsync(TopLevel window)
        {
            IReadOnlyList<IStorageFile> files = await window.StorageProvider.OpenFilePickerAsync(FilePickerOpenOptions).ConfigureAwait(false);
            return files.Count == 1 ? files[0].TryGetLocalPath() : null;
        }
    }
}
