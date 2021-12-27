using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Windows
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private readonly IInstallDirValidator _installDirValidator = null!;
        private readonly IInstallDirLocator _installDirLocator = null!;

        public MainWindow() { }

        [ActivatorUtilitiesConstructor]
        public MainWindow(MainWindowViewModel viewModel, ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
            InitializeComponent();
            ViewModel = viewModel;
            ViewModel.WhenAnyValue(x => x.ProgressBarStatusType)
                .Select(GetLocalisedStatus)
                .BindTo(ViewModel, x => x.ProgressBarStatusText);
            appSettings.Value.InstallDir.Changed.FirstAsync()
                .Where(x => !installDirValidator.ValidateInstallDir(x))
                .SelectMany(_ => EnsureInstallDirAsync())
                .BindTo(appSettings, x => x.Value.InstallDir.Value);
        }

        private async Task<string?> EnsureInstallDirAsync()
        {
            string? installDir = await _installDirLocator.LocateInstallDir();
            if (_installDirValidator.ValidateInstallDir(installDir)) return installDir;
            return await new InstallFolderDialogWindow().ShowDialog<string?>(this);
        }

        private string? GetLocalisedStatus(ProgressBarStatusType statusType) => this.FindResource(statusType switch
        {
            ProgressBarStatusType.None => string.Empty,
            ProgressBarStatusType.Installing => "Status:Installing",
            ProgressBarStatusType.Uninstalling => "Status:Uninstalling",
            ProgressBarStatusType.Completed => "Status:Completed",
            ProgressBarStatusType.Failed => "Status:Failed",
            _ => string.Empty
        }) as string;
    }
}