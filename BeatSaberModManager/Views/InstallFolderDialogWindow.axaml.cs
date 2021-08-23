using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;

using ReactiveUI;


namespace BeatSaberModManager.Views
{
    public partial class InstallFolderDialogWindow : Window
    {
        public InstallFolderDialogWindow()
        {
            InitializeComponent();
            ContinueButton.Command = ReactiveCommand.CreateFromTask(ShowFolderDialogAsync);
        }

        private async Task ShowFolderDialogAsync()
        {
            OpenFolderDialog dialog = new();
            string? folder = await dialog.ShowAsync(this);
            Close(folder);
        }

        private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => Close(null);
    }
}