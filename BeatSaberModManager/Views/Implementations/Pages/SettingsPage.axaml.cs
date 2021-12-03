using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class SettingsPage : ReactiveUserControl<SettingsViewModel>
    {
        private readonly Window _mainWindow = null!;

        public SettingsPage() { }

        [ActivatorUtilitiesConstructor]
        public SettingsPage(SettingsViewModel viewModel, Window window)
        {
            InitializeComponent();
            _mainWindow = window;
            ViewModel = viewModel;
        }

        public async void OnSelectInstallFolderButtonClicked(object? sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.InstallDir = await openFolderDialog.ShowAsync(_mainWindow).ConfigureAwait(false);
        }

        public async void OnSelectThemesButtonClicked(object? sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.ThemesDir = await openFolderDialog.ShowAsync(_mainWindow).ConfigureAwait(false);
        }

        public async void OnInstallPlaylistButtonClicked(object? sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                AllowMultiple = false,
                Filters = { new FileDialogFilter { Extensions = { "bplist" }, Name = "BeatSaber Playlist" } }
            };

            string[]? filePaths = await openFileDialog.ShowAsync(_mainWindow).ConfigureAwait(false);
            if (filePaths?.Length is not 1) return;
            await ViewModel!.InstallPlaylistsAsync(filePaths[0]).ConfigureAwait(false);
        }
    }
}