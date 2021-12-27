using System;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class SettingsPage : ReactiveUserControl<SettingsViewModel>
    {
        public SettingsPage() { }

        [ActivatorUtilitiesConstructor]
        public SettingsPage(SettingsViewModel viewModel, Window window, IStatusProgress progress)
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
                .SelectMany(x => ViewModel.InstallPlaylistAsync(x, progress))
                .Select(x => x ? ProgressBarStatusType.Completed : ProgressBarStatusType.Failed)
                .Do(_ => progress.Report(string.Empty))
                .Subscribe(progress.Report);
        }
    }
}