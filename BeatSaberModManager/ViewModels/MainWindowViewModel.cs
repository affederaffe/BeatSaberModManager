using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly ModsViewModel _modsViewModel;
        private readonly ObservableAsPropertyHelper<bool> _moreInfoButtonEnabled;
        private readonly ObservableAsPropertyHelper<bool> _installButtonEnabled;

        public MainWindowViewModel(ModsViewModel modsViewModel)
        {
            _modsViewModel = modsViewModel;
            MoreInfoButtonCommand = ReactiveCommand.CreateFromTask(OpenMoreInfoLink);
            InstallButtonCommand = ReactiveCommand.CreateFromTask(_modsViewModel.RefreshModsAsync);
            _modsViewModel.WhenAnyValue(x => x.SelectedGridItem)
                .Select(mod => mod is not null)
                .ToProperty(this, nameof(MoreInfoButtonEnabled), out _moreInfoButtonEnabled);
            _modsViewModel.WhenAnyValue(x => x.GridItems)
                .Select(x => x is not null)
                .ToProperty(this, nameof(InstallButtonEnabled), out _installButtonEnabled);
        }

        public ReactiveCommand<Unit, Unit> MoreInfoButtonCommand { get; }

        public ReactiveCommand<Unit, Unit> InstallButtonCommand { get; }

        public bool MoreInfoButtonEnabled => _moreInfoButtonEnabled.Value;

        public bool InstallButtonEnabled => _installButtonEnabled.Value;

        private double _progressBarValue;
        public double ProgressBarValue
        {
            get => _progressBarValue;
            set => this.RaiseAndSetIfChanged(ref _progressBarValue, value);
        }

        private string _progressBarPreTextResourceName = string.Empty;
        public string ProgressBarPreTextResourceName
        {
            get => _progressBarPreTextResourceName;
            set => this.RaiseAndSetIfChanged(ref _progressBarPreTextResourceName, value);
        }

        private string? _progressBarText;
        public string? ProgressBarText
        {
            get => _progressBarText;
            set => this.RaiseAndSetIfChanged(ref _progressBarText, value);
        }

        private async Task OpenMoreInfoLink()
        {
            if (_modsViewModel.SelectedGridItem!.AvailableMod.MoreInfoLink is not null)
                await PlatformUtils.OpenBrowserOrFileExplorer(_modsViewModel.SelectedGridItem.AvailableMod.MoreInfoLink);
        }
    }
}