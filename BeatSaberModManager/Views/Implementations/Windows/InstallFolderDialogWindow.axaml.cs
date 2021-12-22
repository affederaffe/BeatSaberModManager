using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Windows
{
    public partial class InstallFolderDialogWindow : Window
    {
        public InstallFolderDialogWindow()
        {
            InitializeComponent();
            ContinueButton.Command = ReactiveCommand.CreateFromTask(OnContinueButtonClicked);
        }

        public void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => Close(null);

        private async Task OnContinueButtonClicked()
        {
            string? folder = await new OpenFolderDialog().ShowAsync(this);
            Close(folder);
        }
    }
}