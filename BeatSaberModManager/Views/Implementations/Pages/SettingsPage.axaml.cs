using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class SettingsPage : ReactiveUserControl<SettingsViewModel>
    {
        private readonly IClassicDesktopStyleApplicationLifetime _lifetime = null!;

        public SettingsPage() { }

        [ActivatorUtilitiesConstructor]
        public SettingsPage(SettingsViewModel viewModel, IClassicDesktopStyleApplicationLifetime lifetime, IThemeManager themeManager, ILocalisationManager localisationManager)
        {
            InitializeComponent();
            _lifetime = lifetime;
            ViewModel = viewModel;
            LocalisationManager = localisationManager;
            ThemeManager = themeManager;
        }

        public IThemeManager ThemeManager { get; } = null!;

        public ILocalisationManager LocalisationManager { get; } = null!;

        public async void OnSelectInstallFolderButtonClicked(object? sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.InstallDir = await openFolderDialog.ShowAsync(_lifetime.MainWindow).ConfigureAwait(false);
        }

        public async void OnSelectThemesButtonClicked(object? sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.ThemesDir = await openFolderDialog.ShowAsync(_lifetime.MainWindow).ConfigureAwait(false);
        }

        public async void OnInstallPlaylistButtonClicked(object? sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                AllowMultiple = false,
                Filters = { new FileDialogFilter { Extensions = { "bplist" }, Name = "BeatSaber Playlist" } }
            };

            string[]? filePaths = await openFileDialog.ShowAsync(_lifetime.MainWindow).ConfigureAwait(false);
            if (filePaths?.Length is not 1) return;
            await ViewModel!.InstallPlaylistsAsync(filePaths[0]).ConfigureAwait(false);
        }
    }
}