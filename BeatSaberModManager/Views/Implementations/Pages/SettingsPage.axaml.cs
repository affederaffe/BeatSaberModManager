using System;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class SettingsPage : ReactiveUserControl<SettingsViewModel>
    {
        public SettingsPage() { }

        [ActivatorUtilitiesConstructor]
        public SettingsPage(SettingsViewModel viewModel, Window window)
        {
            InitializeComponent();
            ViewModel = viewModel;
            SelectInstallFolderButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => new OpenFolderDialog().ShowAsync(window))
                .BindTo(ViewModel, x => x.InstallDir);
            SelectThemesFolderButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => new OpenFolderDialog().ShowAsync(window))
                .BindTo(ViewModel, x => x.ThemesDir);
            InstallPlaylistButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => new OpenFileDialog { AllowMultiple = false, Filters = { new FileDialogFilter { Extensions = { "bplist" }, Name = "BeatSaber Playlist" } } }.ShowAsync(window))
                .Where(x => x?.Length is 1)
                .Select(x => x![0])
                .ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(ViewModel.InstallPlaylistAsync)
                .Select(x => new ProgressInfo(x ? StatusType.Completed : StatusType.Failed, null))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(ViewModel.StatusProgress.Report);
        }
    }
}