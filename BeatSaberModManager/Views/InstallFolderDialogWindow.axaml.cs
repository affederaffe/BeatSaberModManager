using Avalonia.Controls;
using Avalonia.Interactivity;


namespace BeatSaberModManager.Views
{
    public partial class InstallFolderDialogWindow : Window
    {
        public InstallFolderDialogWindow()
        {
            InitializeComponent();
        }

        private async void OnContinueButtonClicked(object? sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new();
            string? folder = await dialog.ShowAsync(this);
            Close(folder);
        }

        private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => Close(null);
    }
}