using System;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Interactivity;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.ViewModels;

using ReactiveUI;


namespace BeatSaberModManager.Views.Pages
{
    public partial class DashboardPage : ReactiveUserControl<DashboardViewModel>
    {
        public DashboardPage() { }

        public DashboardPage(DashboardViewModel viewModel, Window window)
        {
            InitializeComponent();
            ViewModel = viewModel;
            InstallPlaylistButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => new OpenFileDialog { Filters = { new FileDialogFilter { Extensions = { "bplist" }, Name = "BeatSaber Playlist" } } }.ShowAsync(window))
                .Where(static x => x?.Length == 1)
                .Select(static x => x![0])
                .ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(ViewModel.InstallPlaylistAsync)
                .Select(static x => new ProgressInfo(x ? StatusType.Completed : StatusType.Failed, null))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(ViewModel.StatusProgress.Report);
        }
    }
}