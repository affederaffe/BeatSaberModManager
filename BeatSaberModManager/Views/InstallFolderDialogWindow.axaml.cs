using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;


namespace BeatSaberModManager.Views
{
    public class InstallFolderDialogWindow : Window
    {
        public InstallFolderDialogWindow()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OnContinueButtonClicked(object? sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new();
            string? folder = await dialog.ShowAsync(this);
            Close(string.IsNullOrEmpty(folder) ? null : folder);
        }

        private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => Close(null);
    }
}