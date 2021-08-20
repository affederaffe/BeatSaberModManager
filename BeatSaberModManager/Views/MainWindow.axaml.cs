using System.Threading.Tasks;

using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.ViewModels;

using ReactiveUI;

using Splat;


namespace BeatSaberModManager.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<MainWindowViewModel>();
            Title = nameof(BeatSaberModManager);
            this.WhenActivated(_ => ValidateOrSetInstallDir().ConfigureAwait(false));
        }

        private async Task ValidateOrSetInstallDir()
        {
            OptionsViewModel optionsViewModel = Locator.Current.GetService<OptionsViewModel>();
            if (optionsViewModel.InstallDir is not null) return;
            IInstallDirLocator installDirLocator = Locator.Current.GetService<IInstallDirLocator>();
            if (installDirLocator.TryDetectInstallDir(out string? installDir)) optionsViewModel.InstallDir = installDir;
            optionsViewModel.InstallDir ??= await new InstallFolderDialogWindow().ShowDialog<string?>(this);
        }
    }
}