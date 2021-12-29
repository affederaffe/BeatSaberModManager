using System.Reactive;
using System.Reactive.Linq;

using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utilities;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableAsPropertyHelper<bool> _moreInfoButtonEnabled;
        private readonly ObservableAsPropertyHelper<bool> _installButtonEnabled;

        public MainWindowViewModel(ModsViewModel modsViewModel, SettingsViewModel settingsViewModel, IStatusProgress statusProgress)
        {
            ModsViewModel = modsViewModel;
            SettingsViewModel = settingsViewModel;
            StatusProgress = (StatusProgress)statusProgress;
            MoreInfoButtonCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(modsViewModel.SelectedGridItem!.AvailableMod.MoreInfoLink));
            InstallButtonCommand = ReactiveCommand.CreateFromTask(modsViewModel.RefreshModsAsync);
            modsViewModel.WhenAnyValue(x => x.SelectedGridItem).Select(x => x?.AvailableMod.MoreInfoLink is not null).ToProperty(this, nameof(MoreInfoButtonEnabled), out _moreInfoButtonEnabled);
            modsViewModel.WhenAnyValue(x => x.IsSuccess).ToProperty(this, nameof(InstallButtonEnabled), out _installButtonEnabled);
        }

        public ModsViewModel ModsViewModel { get; }

        public SettingsViewModel SettingsViewModel { get; }

        public StatusProgress StatusProgress { get; }

        public ReactiveCommand<Unit, Unit> MoreInfoButtonCommand { get; }

        public ReactiveCommand<Unit, Unit> InstallButtonCommand { get; }

        public bool MoreInfoButtonEnabled => _moreInfoButtonEnabled.Value;

        public bool InstallButtonEnabled => _installButtonEnabled.Value;
    }
}