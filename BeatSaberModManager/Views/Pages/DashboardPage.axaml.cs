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
    public partial class DashboardPage : ReactiveUserControl<DashboardViewModel>
    {
        private readonly Window _window = null!;

        public DashboardPage() { }

        public DashboardPage(DashboardViewModel viewModel, Window window)
        {
            InitializeComponent();
            _window = window;
            ViewModel = viewModel;
            ReactiveCommand<Unit, string[]?> installPlaylistCommand = ReactiveCommand.CreateFromTask(ShowOpenFolderDialog, viewModel.ModsViewModel.IsInstallDirValidObservable);
            installPlaylistCommand.Where(static x => x?.Length == 1)
                .Select(static x => x![0])
                .SelectMany(ViewModel.InstallPlaylistAsync)
                .Select(static x => new ProgressInfo(x ? StatusType.Completed : StatusType.Failed, null))
                .Subscribe(ViewModel.StatusProgress.Report);
            InstallPlaylistButton.Command = installPlaylistCommand;
        }

        private Task<string[]?> ShowOpenFolderDialog() =>
            new OpenFileDialog { Filters = { new FileDialogFilter { Extensions = { "bplist" }, Name = "BeatSaber Playlist" } } }.ShowAsync(_window);
    }
}