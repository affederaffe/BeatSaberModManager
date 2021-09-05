using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class OptionsView : ReactiveUserControl<OptionsViewModel>
    {
        public OptionsView() { }

        [ActivatorUtilitiesConstructor]
        public OptionsView(OptionsViewModel optionsViewModel, ILocalisationManager localisationManager, IThemeManager themeManager)
        {
            InitializeComponent();
            ViewModel = optionsViewModel;
            LocalisationManager = localisationManager;
            ThemeManager = themeManager;
            SelectInstallFolderButton.Command = ReactiveCommand.CreateFromTask(SelectInstallFolderAsync);
            SelectThemesFolderButton.Command = ReactiveCommand.CreateFromTask(SelectThemesFolderAsync);
            InstallPlaylistButton.Command = ReactiveCommand.CreateFromTask(InstallPlaylistAsync);
            ViewModel.WhenAnyValue(x => x.ThemesDir).Subscribe(_ => ThemeManager.ReloadExternalThemes());
        }

        public ILocalisationManager LocalisationManager { get; } = null!;

        public IThemeManager ThemeManager { get; } = null!;

        private async Task SelectInstallFolderAsync()
        {
            if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.InstallDir = await openFolderDialog.ShowAsync(desktop.MainWindow);
        }

        private async Task SelectThemesFolderAsync()
        {
            if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.ThemesDir = await openFolderDialog.ShowAsync(desktop.MainWindow);
        }

        private async Task InstallPlaylistAsync()
        {
            if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
            OpenFileDialog openFileDialog = new();
            FileDialogFilter fileDialogFilter = new();
            fileDialogFilter.Extensions.Add("bplist");
            fileDialogFilter.Name = "BeatSaber Playlist";
            openFileDialog.Filters.Add(fileDialogFilter);
            openFileDialog.AllowMultiple = true;
            string[] filePaths = await openFileDialog.ShowAsync(desktop.MainWindow);
            if (filePaths is null || filePaths.Length <= 0) return;
            await ViewModel!.InstallPlaylistsAsync(filePaths);
        }
    }
}