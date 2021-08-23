using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Localisation;
using BeatSaberModManager.Theming;
using BeatSaberModManager.ViewModels;

using ReactiveUI;

using Splat;


namespace BeatSaberModManager.Views
{
    public partial class OptionsView : ReactiveUserControl<OptionsViewModel>
    {
        public OptionsView()
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<OptionsViewModel>();
            LanguageSwitcher = Locator.Current.GetService<LanguageSwitcher>();
            ThemeSwitcher = Locator.Current.GetService<ThemeSwitcher>();
            SelectInstallFolderButton.Command = ReactiveCommand.CreateFromTask(SelectInstallFolderAsync);
            SelectThemesFolderButton.Command = ReactiveCommand.CreateFromTask(SelectThemesFolderAsync);
            InstallPlaylistButton.Command = ReactiveCommand.CreateFromTask(InstallPlaylistAsync);
            ViewModel.WhenAnyValue(static x => x.ThemesDir).Subscribe(_ => ThemeSwitcher.TryLoadExternThemes());
        }

        public LanguageSwitcher LanguageSwitcher { get; }

        public ThemeSwitcher ThemeSwitcher { get; }

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