using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
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
        private readonly Window _window = null!;

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
            _window = window;
            ViewModel = viewModel;
            ReactiveCommand<Unit, string[]?> showFileDialogCommand =
                ReactiveCommand.CreateFromTask(ShowOpenFileDialog, viewModel.ModsViewModel.IsInstallDirValidObservable);
            showFileDialogCommand.Where(static x => x?.Length == 1)
                .Select(static x => x![0])
                .SelectMany(ViewModel.InstallPlaylistAsync)
                .Select(static x => new ProgressInfo(x ? StatusType.Completed : StatusType.Failed, null))
                .Subscribe(ViewModel.StatusProgress.Report);
            InstallPlaylistButton.Command = showFileDialogCommand;
        }

        private Task<string[]?> ShowOpenFileDialog()
        {
            OpenFileDialog dialog = new();
            dialog.Filters!.Add(new FileDialogFilter { Extensions = { "bplist" }, Name = "BeatSaber Playlist" });
            return dialog.ShowAsync(_window);
        }
    }
}
