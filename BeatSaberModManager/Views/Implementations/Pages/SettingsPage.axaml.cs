using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class SettingsPage : ReactiveUserControl<SettingsViewModel>
    {
        private readonly IClassicDesktopStyleApplicationLifetime _lifetime = null!;
        private readonly IInstallDirValidator _installDirValidator = null!;

        public SettingsPage() { }

        [ActivatorUtilitiesConstructor]
        public SettingsPage(SettingsViewModel viewModel, IClassicDesktopStyleApplicationLifetime lifetime, IInstallDirValidator installDirValidator, IThemeManager themeManager, ILocalisationManager localisationManager)
        {
            InitializeComponent();
            _lifetime = lifetime;
            _installDirValidator = installDirValidator;
            ViewModel = viewModel;
            LocalisationManager = localisationManager;
            ThemeManager = themeManager;
            SelectInstallFolderButton.Command = ReactiveCommand.CreateFromTask(SelectInstallFolderAsync);
            SelectThemesFolderButton.Command = ReactiveCommand.CreateFromTask(SelectThemesFolderAsync);
            InstallPlaylistButton.Command = ReactiveCommand.CreateFromTask(InstallPlaylistAsync);
        }

        public IThemeManager ThemeManager { get; } = null!;

        public ILocalisationManager LocalisationManager { get; } = null!;

        private async Task SelectInstallFolderAsync()
        {
            OpenFolderDialog openFolderDialog = new();
            string? installDir = await openFolderDialog.ShowAsync(_lifetime.MainWindow);
            if (_installDirValidator.ValidateInstallDir(installDir))
                ViewModel!.InstallDir = installDir;
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
                AllowMultiple = false,
                Filters = { new FileDialogFilter { Extensions = { "bplist" }, Name = "BeatSaber Playlist" } }
            };

            string[]? filePaths = await openFileDialog.ShowAsync(_lifetime.MainWindow);
            if (filePaths?.Length is not 1) return;
            await ViewModel!.InstallPlaylistsAsync(filePaths[0]);
        }
    }
}