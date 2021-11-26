using Avalonia.Controls;
using Avalonia.Interactivity;


namespace BeatSaberModManager.Views.Implementations.Windows
{
    public partial class InstallFolderDialogWindow : Window
    {
        public InstallFolderDialogWindow()
        {
            InitializeComponent();
        }

        public void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => Close(null);

        public async void OnContinueButtonClicked(object? sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new();
            string? folder = await dialog.ShowAsync(this).ConfigureAwait(false);
            Close(folder);
        }
    }
}