using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class SettingsPage : ReactiveUserControl<SettingsViewModel>
    {
        private readonly Window _mainWindow = null!;

        public SettingsPage() { }

        [ActivatorUtilitiesConstructor]
        public SettingsPage(SettingsViewModel viewModel, Window window, ILocalisationManager localisationManager, IThemeManager themeManager)
        {
            InitializeComponent();
            _mainWindow = window;
            ViewModel = viewModel;
            LanguagesComboBox.DataContext = localisationManager;
            LanguagesComboBox.Items = localisationManager.Languages;
            LanguagesComboBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(ILocalisationManager.SelectedLanguage)));
            ThemesComboBox.DataContext = themeManager;
            ThemesComboBox.Bind(ItemsControl.ItemsProperty, new Binding(nameof(IThemeManager.Themes)));
            ThemesComboBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(IThemeManager.SelectedTheme)));
        }

        public async void OnSelectInstallFolderButtonClicked(object? sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.InstallDir = await openFolderDialog.ShowAsync(_mainWindow);
        }

        public async void OnSelectThemesButtonClicked(object? sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.ThemesDir = await openFolderDialog.ShowAsync(_mainWindow);
        }

        public async void OnInstallPlaylistButtonClicked(object? sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                AllowMultiple = false,
                Filters = { new FileDialogFilter { Extensions = { "bplist" }, Name = "BeatSaber Playlist" } }
            };

            string[]? filePaths = await openFileDialog.ShowAsync(_mainWindow);
            if (filePaths?.Length is not 1) return;
            await ViewModel!.InstallPlaylistsAsync(filePaths[0]);
        }
    }
}