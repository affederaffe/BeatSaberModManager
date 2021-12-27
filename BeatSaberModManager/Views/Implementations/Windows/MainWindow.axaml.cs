using System.Reactive.Disposables;
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
        private readonly IInstallDirLocator _installDirLocator = null!;

        public MainWindow() { }

        [ActivatorUtilitiesConstructor]
        public MainWindow(MainWindowViewModel viewModel, ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _installDirLocator = installDirLocator;
            InitializeComponent();
            ViewModel = viewModel;
            this.WhenActivated(disposable =>
            {
                ViewModel.WhenAnyValue(x => x.ProgressBarStatusType)
                    .Select(GetLocalisedStatus)
                    .BindTo(ViewModel, x => x.ProgressBarStatusText)
                    .DisposeWith(disposable);
                appSettings.Value.InstallDir.Changed.FirstAsync()
                    .Where(x => !installDirValidator.ValidateInstallDir(x))
                    .SelectMany(_ => EnsureInstallDirAsync())
                    .BindTo(appSettings, x => x.Value.InstallDir.Value)
                    .DisposeWith(disposable);
            });
        }

        private async Task<string?> EnsureInstallDirAsync() =>
            await _installDirLocator.LocateInstallDirAsync().ConfigureAwait(false) ??
            await new InstallFolderDialogWindow().ShowDialog<string?>(this).ConfigureAwait(false);

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