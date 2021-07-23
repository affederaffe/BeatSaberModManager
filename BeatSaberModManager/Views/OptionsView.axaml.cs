using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

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
            _modsViewModel = Locator.Current.GetService<ModsViewModel>();
        }

        private async void OnSelectInstallFolderButtonClicked(object? sender, RoutedEventArgs e)
        {
            if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
            OpenFolderDialog openFolderDialog = new();
            ViewModel!.InstallDir = await openFolderDialog.ShowAsync(desktop.MainWindow);
            await _modsViewModel.RefreshDataGridAsync();
        }
    }
}