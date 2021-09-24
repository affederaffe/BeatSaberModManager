using System;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class OptionsPage : ReactiveUserControl<OptionsViewModel>, IPage
    {
        private readonly IClassicDesktopStyleApplicationLifetime _lifetime = null!;

        public OptionsPage() { }

        [ActivatorUtilitiesConstructor]
        public OptionsPage(OptionsViewModel optionsViewModel, IClassicDesktopStyleApplicationLifetime lifetime, IThemeManager themeManager, ILocalisationManager localisationManager)
        {
            InitializeComponent();
            _lifetime = lifetime;
            ViewModel = optionsViewModel;
            LocalisationManager = localisationManager;
            ThemeManager = themeManager;
            SelectInstallFolderButton.Command = ReactiveCommand.CreateFromTask(SelectInstallFolderAsync);
            SelectThemesFolderButton.Command = ReactiveCommand.CreateFromTask(SelectThemesFolderAsync);
            InstallPlaylistButton.Command = ReactiveCommand.CreateFromTask(InstallPlaylistAsync);
            ViewModel.WhenAnyValue(x => x.ThemesDir).WhereNotNull().Subscribe(ThemeManager.ReloadExternalThemes);
        }

        public IThemeManager ThemeManager { get; } = null!;

        public ILocalisationManager LocalisationManager { get; } = null!;

        private async Task SelectInstallFolderAsync()
        {
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.InstallDir = await openFolderDialog.ShowAsync(_lifetime.MainWindow);
        }

        private async Task SelectThemesFolderAsync()
        {
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.ThemesDir = await openFolderDialog.ShowAsync(_lifetime.MainWindow);
        }

        private async Task InstallPlaylistAsync()
        {
            OpenFileDialog openFileDialog = new()
            {
                AllowMultiple = true,
                Filters = { new FileDialogFilter { Extensions = { "bplist" }, Name = "BeatSaber Playlist" } }
            };

            string[] filePaths = await openFileDialog.ShowAsync(_lifetime.MainWindow);
            if (filePaths is null || filePaths.Length <= 0) return;
            await ViewModel!.InstallPlaylistsAsync(filePaths);
        }
    }
}