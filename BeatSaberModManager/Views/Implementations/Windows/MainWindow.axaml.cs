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
        private readonly ISettings<AppSettings> _appSettings = null!;
        private readonly IInstallDirValidator _installDirValidator = null!;
        private readonly IInstallDirLocator _installDirLocator = null!;

        public MainWindow() { }

        [ActivatorUtilitiesConstructor]
        public MainWindow(MainWindowViewModel viewModel, ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _appSettings = appSettings;
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
            InitializeComponent();
            ViewModel = viewModel;
            ViewModel.WhenAnyValue(x => x.ProgressBarStatusType).Select(GetLocalisedStatus).BindTo(ViewModel, x => x.ProgressBarStatusText);
            _appSettings.Value.InstallDir.Changed.FirstAsync().InvokeCommand(ReactiveCommand.CreateFromTask<string?>(EnsureInstallDir));
        }

        private async Task EnsureInstallDir(string? installDir)
        {
            if (_installDirValidator.ValidateInstallDir(installDir)) return;
            _appSettings.Value.InstallDir.Value = await _installDirLocator.LocateInstallDir();
            if (_installDirValidator.ValidateInstallDir(installDir)) return;
            _appSettings.Value.InstallDir.Value = await new InstallFolderDialogWindow().ShowDialog<string?>(this);
        }

        private string? GetLocalisedStatus(ProgressBarStatusType statusType) => this.FindResource(statusType switch
        {
            ProgressBarStatusType.None => string.Empty,
            ProgressBarStatusType.Installing => "Status:Installing",
            ProgressBarStatusType.Uninstalling => "Status:Uninstalling",
            ProgressBarStatusType.Completed => "Status:Completed",
            _ => string.Empty
        }) as string;
    }
}