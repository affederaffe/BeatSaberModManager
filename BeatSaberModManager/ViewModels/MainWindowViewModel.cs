using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations;
using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly ModsViewModel _modsViewModel;
        private readonly ObservableAsPropertyHelper<bool> _moreInfoButtonEnabled;
        private readonly ObservableAsPropertyHelper<bool> _installButtonEnabled;
        private readonly ObservableAsPropertyHelper<double> _progressBarValue;
        private readonly ObservableAsPropertyHelper<string?> _progressBarText;
        private readonly ObservableAsPropertyHelper<ProgressBarStatusType> _progressBarStatusType;

        public MainWindowViewModel(ModsViewModel modsViewModel, Settings settings, StatusProgress progress)
        {
            _modsViewModel = modsViewModel;
            MoreInfoButtonCommand = ReactiveCommand.CreateFromTask(OpenMoreInfoLink);
            InstallButtonCommand = ReactiveCommand.CreateFromTask(_modsViewModel.RefreshModsAsync);
            _modsViewModel.WhenAnyValue(x => x.SelectedGridItem)
                .Select(mod => mod is not null)
                .ToProperty(this, nameof(MoreInfoButtonEnabled), out _moreInfoButtonEnabled);
            _modsViewModel.WhenAnyValue(x => x.GridItems.Count)
                .Select(x => x > 0 && Directory.Exists(settings.InstallDir))
                .ToProperty(this, nameof(InstallButtonEnabled), out _installButtonEnabled);
            progress.ProgressValue.ToProperty(this, nameof(ProgressBarValue), out _progressBarValue);
            progress.StatusText.ToProperty(this, nameof(ProgressBarText), out _progressBarText);
            progress.StatusType.ToProperty(this, nameof(ProgressBarStatusType), out _progressBarStatusType);
        }

        public ReactiveCommand<Unit, Unit> MoreInfoButtonCommand { get; }

        public ReactiveCommand<Unit, Unit> InstallButtonCommand { get; }

        public bool MoreInfoButtonEnabled => _moreInfoButtonEnabled.Value;

        public bool InstallButtonEnabled => _installButtonEnabled.Value;

        public double ProgressBarValue => _progressBarValue.Value;

        public string? ProgressBarText => _progressBarText.Value;

        public ProgressBarStatusType ProgressBarStatusType => _progressBarStatusType.Value;

        private async Task OpenMoreInfoLink()
        {
            if (_modsViewModel.SelectedGridItem is null) return;
            await Task.Run(() => PlatformUtils.OpenBrowserOrFileExplorer(_modsViewModel.SelectedGridItem.AvailableMod.MoreInfoLink));
        }
    }
}