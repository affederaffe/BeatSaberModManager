using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models;
using BeatSaberModManager.ThemeManagement;
using BeatSaberModManager.ViewModels;

using Splat;


namespace BeatSaberModManager.Views
{
    public class OptionsView : ReactiveUserControl<OptionsViewModel>
    {
        private readonly Settings _settings;
        private readonly ModsViewModel _modsViewModel;

        public OptionsView()
        {
            AvaloniaXamlLoader.Load(this);
            ViewModel = Locator.Current.GetService<OptionsViewModel>();
            ThemeSwitcher = Locator.Current.GetService<ThemeSwitcher>();
            _settings = Locator.Current.GetService<Settings>();
            _modsViewModel = Locator.Current.GetService<ModsViewModel>();
        }

        public ThemeSwitcher ThemeSwitcher { get; }

        public Theme SelectedTheme
        {
            get => ThemeSwitcher.SelectedTheme;
            set
            {
                _settings.ThemeName = value.Name;
                ThemeSwitcher.SelectedTheme = value;
            }
        }

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