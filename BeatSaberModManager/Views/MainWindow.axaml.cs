using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.ViewModels;

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
        }

        protected override async void OnInitialized()
        {
            OptionsViewModel optionsViewModel = Locator.Current.GetService<OptionsViewModel>();
            if (optionsViewModel.InstallDir is not null) return;
            IInstallDirLocator installDirLocator = Locator.Current.GetService<IInstallDirLocator>();
            if (installDirLocator.TryDetectInstallDir(out string? installDir)) optionsViewModel.InstallDir = installDir;
            optionsViewModel.InstallDir ??= await new InstallFolderDialogWindow().ShowDialog<string?>(this);
        }
    }
}