using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Localisation;
using BeatSaberModManager.Models;
using BeatSaberModManager.Theming;
using BeatSaberModManager.ViewModels;

using Splat;


namespace BeatSaberModManager.Views
{
    public class OptionsView : ReactiveUserControl<OptionsViewModel>
    {
        private readonly ModsViewModel _modsViewModel;

        public OptionsView()
        {
            AvaloniaXamlLoader.Load(this);
            ViewModel = Locator.Current.GetService<OptionsViewModel>();
            LanguageSwitcher = Locator.Current.GetService<LanguageSwitcher>();
            ThemeSwitcher = Locator.Current.GetService<ThemeSwitcher>();
            _modsViewModel = Locator.Current.GetService<ModsViewModel>();
        }

        public LanguageSwitcher LanguageSwitcher { get; }

        public ThemeSwitcher ThemeSwitcher { get; }

        private async void OnSelectInstallFolderButtonClicked(object? sender, RoutedEventArgs e)
        {
            if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.InstallDir = await openFolderDialog.ShowAsync(desktop.MainWindow);
            await _modsViewModel.RefreshDataGridAsync();
        }

        private async void OnSelectThemesFolderButtonClicked(object? sender, RoutedEventArgs e)
        {
            if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.ThemesDir = await openFolderDialog.ShowAsync(desktop.MainWindow);
        }
    }
}