using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Windows
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private readonly AppSettings _appSettings = null!;
        private readonly IInstallDirValidator _installDirValidator = null!;

        public MainWindow() { }

        [ActivatorUtilitiesConstructor]
        public MainWindow(MainWindowViewModel mainWindowViewModel, ISettings<AppSettings> appSettings, IEnumerable<IPage> pages, IInstallDirValidator installDirValidator)
        {
            InitializeComponent();
            _appSettings = appSettings.Value;
            _installDirValidator = installDirValidator;
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
            if (_installDirValidator.ValidateInstallDir(_appSettings.InstallDir.Value)) return;
            string? installDir = await new InstallFolderDialogWindow().ShowDialog<string?>(this);
            if (_installDirValidator.ValidateInstallDir(_appSettings.InstallDir.Value))
                _appSettings.InstallDir.Value = installDir;
        }

        private object? GetLocalizedStatus(ProgressBarStatusType statusType) => this.FindResource(statusType switch
        {
            ProgressBarStatusType.None => string.Empty,
            ProgressBarStatusType.Installing => "Status:Installing",
            ProgressBarStatusType.Uninstalling => "Status:Uninstalling",
            _ => string.Empty
        });
    }
}