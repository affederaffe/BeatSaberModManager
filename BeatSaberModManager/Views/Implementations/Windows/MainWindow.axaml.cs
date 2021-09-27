using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Windows
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private readonly OptionsViewModel _optionsViewModel = null!;

        public MainWindow() { }

        [ActivatorUtilitiesConstructor]
        public MainWindow(MainWindowViewModel mainWindowViewModel, OptionsViewModel optionsViewModel, IEnumerable<IPage> pages)
        {
            InitializeComponent();
            _optionsViewModel = optionsViewModel;
            ViewModel = mainWindowViewModel;
            Title = nameof(BeatSaberModManager);
            Pages.Items = pages.Select(x => new TabItem { Content = x }).ToArray();
            ViewModel.WhenAnyValue(x => x.ProgressBarStatusType)
                .Select(GetLocalizedStatus)
                .BindTo(this, x => x.ProgressBarStatusText.Content);
        }

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            _optionsViewModel.InstallDir ??= await new InstallFolderDialogWindow().ShowDialog<string?>(this);
        }

        private object? GetLocalizedStatus(ProgressBarStatusType statusType) => this.FindResource(statusType switch
        {
            ProgressBarStatusType.None => string.Empty,
            ProgressBarStatusType.Installing => "MainWindow:InstallModText",
            ProgressBarStatusType.Uninstalling => "MainWindow:UninstallModText",
            _ => string.Empty
        });
    }
}