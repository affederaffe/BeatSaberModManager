using System;
using System.Reactive;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
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

        public MainWindowViewModel(ModsViewModel modsViewModel, SettingsViewModel settingsViewModel, ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator, IStatusProgress statusProgress)
        {
            ModsViewModel = modsViewModel;
            SettingsViewModel = settingsViewModel;
            StatusProgress = (StatusProgress)statusProgress;
            MoreInfoButtonCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(modsViewModel.SelectedGridItem!.AvailableMod.MoreInfoLink));
            InstallButtonCommand = ReactiveCommand.CreateFromTask(modsViewModel.RefreshModsAsync);
            modsViewModel.WhenAnyValue(x => x.SelectedGridItem).Select(x => x?.AvailableMod.MoreInfoLink is not null).ToProperty(this, nameof(MoreInfoButtonEnabled), out _moreInfoButtonEnabled);
            modsViewModel.WhenAnyValue(x => x.IsSuccess).ToProperty(this, nameof(InstallButtonEnabled), out _installButtonEnabled);
            ManualInstallDirSelectionRequested = appSettings.Value.InstallDir.Changed.FirstAsync()
                .Where(x => !installDirValidator.ValidateInstallDir(x))
                .SelectMany(async _ => appSettings.Value.InstallDir.Value = await installDirLocator.LocateInstallDirAsync())
                .Where(x => !installDirValidator.ValidateInstallDir(x))
                .Select(_ => Unit.Default);
        }

        public ModsViewModel ModsViewModel { get; }

        public SettingsViewModel SettingsViewModel { get; }

        public StatusProgress StatusProgress { get; }

        public IObservable<Unit> ManualInstallDirSelectionRequested { get; }

        public ReactiveCommand<Unit, Unit> MoreInfoButtonCommand { get; }

        public ReactiveCommand<Unit, Unit> InstallButtonCommand { get; }

        public bool MoreInfoButtonEnabled => _moreInfoButtonEnabled.Value;

        public bool InstallButtonEnabled => _installButtonEnabled.Value;
    }
}