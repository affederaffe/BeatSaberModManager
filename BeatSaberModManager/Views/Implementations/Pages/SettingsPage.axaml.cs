using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class SettingsPage : ReactiveUserControl<SettingsViewModel>
    {
        private readonly Window _mainWindow = null!;

        public SettingsPage() { }

        [ActivatorUtilitiesConstructor]
        public SettingsPage(SettingsViewModel viewModel, Window window, ILocalisationManager localisationManager, IThemeManager themeManager)
        {
            _mainWindow = window;
            InitializeComponent();
            ViewModel = viewModel;
            LanguagesComboBox.DataContext = localisationManager;
            LanguagesComboBox.Items = localisationManager.Languages;
            LanguagesComboBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(ILocalisationManager.SelectedLanguage)));
            ThemesComboBox.DataContext = themeManager;
            ThemesComboBox.Bind(ItemsControl.ItemsProperty, new Binding(nameof(IThemeManager.Themes)));
            ThemesComboBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(IThemeManager.SelectedTheme)));
            SelectInstallFolderButton.Command = ReactiveCommand.CreateFromTask(OnSelectInstallFolderButtonClicked);
            SelectThemeFolderButton.Command = ReactiveCommand.CreateFromTask(OnSelectThemeFolderButtonClicked);
            InstallPlaylistButton.Command = ReactiveCommand.CreateFromTask(OnInstallPlaylistButtonClicked);
        }

        private async Task OnSelectInstallFolderButtonClicked()
        {
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.InstallDir = await openFolderDialog.ShowAsync(_mainWindow);
        }

        private async Task OnSelectThemeFolderButtonClicked()
        {
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.ThemesDir = await openFolderDialog.ShowAsync(_mainWindow);
        }

        private async Task OnInstallPlaylistButtonClicked()
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