using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations;
using BeatSaberModManager.Models.Implementations.BeatSaber.Playlist;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class OptionsViewModel : ReactiveObject
    {
        private readonly PlaylistInstaller _playlistInstaller;
        private readonly IStatusProgress _progress;
        private readonly ObservableAsPropertyHelper<bool> _hasValidatedInstallDir;
        private readonly ObservableAsPropertyHelper<bool> _openThemesDirButtonActive;

        private const string kBeatSaverProtocol = "beatsaver";
        private const string kModelSaberProtocol = "modelsaber";
        private const string kPlaylistProtocol = "bsplaylist";

        public OptionsViewModel(ModsViewModel modsViewModel, Settings settings, PlaylistInstaller playlistInstaller, IInstallDirValidator installDirValidator, IStatusProgress progress)
        {
            _playlistInstaller = playlistInstaller;
            _progress = progress;
            _installDir = settings.InstallDir;
            _themesDir = settings.ThemesDir;
            OpenInstallDirCommand = ReactiveCommand.CreateFromTask(() => OpenFolderAsync(InstallDir));
            OpenThemesDirCommand = ReactiveCommand.CreateFromTask(() => OpenFolderAsync(ThemesDir));
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync);
            ToggleBeatSaverOneClickHandlerCommand = ReactiveCommand.CreateFromTask(() => ToggleOneClickHandlerAsync(BeatSaverOneClickCheckboxChecked, kBeatSaverProtocol));
            ToggleModelSaberOneClickHandlerCommand = ReactiveCommand.CreateFromTask(() => ToggleOneClickHandlerAsync(ModelSaberOneClickCheckboxChecked, kModelSaberProtocol));
            TogglePlaylistOneClickHandlerCommand = ReactiveCommand.CreateFromTask(() => ToggleOneClickHandlerAsync(PlaylistOneClickCheckBoxChecked, kPlaylistProtocol));
            IObservable<string> validatedInstallDirObservable = this.WhenAnyValue(x => x.InstallDir).Where(installDirValidator.ValidateInstallDir)!;
            validatedInstallDirObservable.BindTo(settings, x => x.InstallDir);
            validatedInstallDirObservable.Select(installDirValidator.DetectVRPlatform).BindTo(settings, x => x.VRPlatform);
            validatedInstallDirObservable.Select(_ => true).ToProperty(this, nameof(HasValidatedInstallDir), out _hasValidatedInstallDir);
            IObservable<string?> themesDirObservable = this.WhenAnyValue(x => x.ThemesDir).Where(x => !string.IsNullOrEmpty(x));
            themesDirObservable.BindTo(settings, x => x.ThemesDir);
            themesDirObservable.Select(_ => true).ToProperty(this, nameof(OpenThemesDirButtonActive), out _openThemesDirButtonActive);
        }

        private string? _installDir;
        public string? InstallDir
        {
            get => _installDir;
            set => this.RaiseAndSetIfChanged(ref _installDir, value);
        }

        private string? _themesDir;
        public string? ThemesDir
        {
            get => _themesDir;
            set => this.RaiseAndSetIfChanged(ref _themesDir, value);
        }

        private bool _beatSaverOneClickCheckboxChecked = PlatformUtils.IsProtocolHandlerRegistered(kBeatSaverProtocol, nameof(BeatSaberModManager));
        public bool BeatSaverOneClickCheckboxChecked
        {
            get => _beatSaverOneClickCheckboxChecked;
            set => this.RaiseAndSetIfChanged(ref _beatSaverOneClickCheckboxChecked, value);
        }

        private bool _modelSaberOneClickCheckboxChecked = PlatformUtils.IsProtocolHandlerRegistered(kModelSaberProtocol, nameof(BeatSaberModManager));
        public bool ModelSaberOneClickCheckboxChecked
        {
            get => _modelSaberOneClickCheckboxChecked;
            set => this.RaiseAndSetIfChanged(ref _modelSaberOneClickCheckboxChecked, value);
        }

        private bool _playlistOneClickCheckBoxChecked = PlatformUtils.IsProtocolHandlerRegistered(kPlaylistProtocol, nameof(BeatSaberModManager));
        public bool PlaylistOneClickCheckBoxChecked
        {
            get => _playlistOneClickCheckBoxChecked;
            set => this.RaiseAndSetIfChanged(ref _playlistOneClickCheckBoxChecked, value);
        }

        public ReactiveCommand<Unit, Unit> OpenInstallDirCommand { get; }

        public ReactiveCommand<Unit, Unit> OpenThemesDirCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallModLoaderCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallAllModsCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleBeatSaverOneClickHandlerCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleModelSaberOneClickHandlerCommand { get; }

        public ReactiveCommand<Unit, Unit> TogglePlaylistOneClickHandlerCommand { get; }

        public bool HasValidatedInstallDir => _hasValidatedInstallDir.Value;

        public bool OpenThemesDirButtonActive => _openThemesDirButtonActive.Value;

        public async Task InstallPlaylistsAsync(string filePath) => await Task.Run(() => _playlistInstaller.InstallPlaylistAsync(filePath, _progress));

        private static async Task ToggleOneClickHandlerAsync(bool active, string protocol)
        {
            if (active) await Task.Run(() => PlatformUtils.RegisterProtocolHandler(protocol, nameof(BeatSaberModManager)));
            else await Task.Run(() => PlatformUtils.UnregisterProtocolHandler(protocol, nameof(BeatSaberModManager)));
        }

        private static async Task OpenFolderAsync(string? path)
        {
            if (!Directory.Exists(path)) return;
            await Task.Run(() => PlatformUtils.OpenBrowserOrFileExplorer(path));
        }
    }
}