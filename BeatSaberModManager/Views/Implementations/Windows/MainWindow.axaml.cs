using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Services.Progress;
using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Windows
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private readonly OptionsViewModel _optionsViewModel = null!;

        public MainWindow() { }

        [ActivatorUtilitiesConstructor]
        public MainWindow(MainWindowViewModel mainWindowViewModel, Pages.IntroView introView, Pages.ModsView modsView, Pages.OptionsView optionsView)
        {
            _optionsViewModel = optionsView.ViewModel!;
            InitializeComponent();
            IntroViewTabItem.Content = introView;
            ModsViewTabItem.Content = modsView;
            OptionsViewTabItem.Content = optionsView;
            ViewModel = mainWindowViewModel;
            Title = nameof(BeatSaberModManager);
            ViewModel.WhenAnyValue(x => x.ProgressBarStatusType).Select(GetLocalizedStatus).BindTo<object?, MainWindow, object>(this, x => x.ProgressBarStatusText.Content);
            this.WhenActivated(_ => ValidateOrSetInstallDir().ConfigureAwait(false));
        }

        private async Task ValidateOrSetInstallDir()
        {
            _optionsViewModel.InstallDir ??= await new InstallFolderDialogWindow().ShowDialog<string?>(this);
        }

        private object? GetLocalizedStatus(ProgressBarStatusType statusType) => this.FindResource(statusType switch
        {
            ProgressBarStatusType.None => string.Empty,
            ProgressBarStatusType.Installing => "MainWindow:InstallModText",
            ProgressBarStatusType.Uninstalling => "MainWindow:UninstallModText",
            _ => string.Empty
        });
    }
}