using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models;
using BeatSaberModManager.Models.Implementations.BeatSaber.Playlist;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class OptionsViewModel : ReactiveObject
    {
        private readonly PlaylistInstaller _playlistInstaller;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IStatusProgress _progress;
        private readonly ObservableAsPropertyHelper<bool> _openInstallDirButtonActive;
        private readonly ObservableAsPropertyHelper<bool> _openThemesDirButtonActive;

        public OptionsViewModel(ModsViewModel modsViewModel, Settings settings, PlaylistInstaller playlistInstaller, IInstallDirValidator installDirValidator, IStatusProgress progress)
        {
            _playlistInstaller = playlistInstaller;
            _installDirValidator = installDirValidator;
            _progress = progress;
            _installDir = settings.InstallDir;
            _themesDir = settings.ThemesDir;
            OpenInstallDirCommand = ReactiveCommand.CreateFromTask(() => PlatformUtils.OpenBrowserOrFileExplorer(settings.InstallDir!));
            OpenThemesDirCommand = ReactiveCommand.CreateFromTask(() => PlatformUtils.OpenBrowserOrFileExplorer(settings.ThemesDir!));
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync);
            ToggleBeatSaverOneClickHandlerCommand = ReactiveCommand.CreateFromTask(() => ToggleOneClickHandler(BeatSaverOneClickCheckboxChecked, "beatsaver", "URI:BeatSaver OneClick Install"));
            ToggleModelSaberOneClickHandlerCommand = ReactiveCommand.CreateFromTask(() => ToggleOneClickHandler(ModelSaberOneClickCheckboxChecked, "modelsaber", "URI:ModelSaber OneClick Install"));
            TogglePlaylistOneClickHandlerCommand = ReactiveCommand.CreateFromTask(() => ToggleOneClickHandler(PlaylistOneClickCheckBoxChecked, "bsplaylist", "URI:BPList OneClick Install"));
            IObservable<string?> validatedInstallDirObservable = this.WhenAnyValue(x => x.InstallDir).Where(_installDirValidator.ValidateInstallDir);
            validatedInstallDirObservable.BindTo(settings, x => x.InstallDir);
            validatedInstallDirObservable.Subscribe(x => settings.VRPlatform = _installDirValidator.DetectVRPlatform(x!));
            validatedInstallDirObservable.Select(_ => true).ToProperty(this, nameof(OpenInstallDirButtonActive), out _openInstallDirButtonActive);
            IObservable<string?> themesDirObservable = this.WhenAnyValue(x => x.ThemesDir);
            themesDirObservable.BindTo(settings, x => x.ThemesDir);
            themesDirObservable.Select(x => !string.IsNullOrEmpty(x)).ToProperty(this, nameof(OpenThemesDirButtonActive), out _openThemesDirButtonActive);
        }

        private string? _installDir;
        public string? InstallDir
        {
            get => _installDir;
            set => this.RaiseAndSetIfChangedConditional(ref _installDir, value, _installDirValidator.ValidateInstallDir);
        }

        private string? _themesDir;
        public string? ThemesDir
        {
            get => _themesDir;
            set => this.RaiseAndSetIfChangedConditional(ref _themesDir, value, x => !string.IsNullOrEmpty(x));
        }

        private bool _beatSaverOneClickCheckboxChecked = PlatformUtils.IsProtocolHandlerRegistered("beatsaver", nameof(BeatSaberModManager));
        public bool BeatSaverOneClickCheckboxChecked
        {
            get => _beatSaverOneClickCheckboxChecked;
            set => this.RaiseAndSetIfChanged(ref _beatSaverOneClickCheckboxChecked, value);
        }

        private bool _modelSaberOneClickCheckboxChecked = PlatformUtils.IsProtocolHandlerRegistered("modelsaber", nameof(BeatSaberModManager));
        public bool ModelSaberOneClickCheckboxChecked
        {
            get => _modelSaberOneClickCheckboxChecked;
            set => this.RaiseAndSetIfChanged(ref _modelSaberOneClickCheckboxChecked, value);
        }

        private bool _playlistOneClickCheckBoxChecked = PlatformUtils.IsProtocolHandlerRegistered("bsplaylist", nameof(BeatSaberModManager));
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

        public bool OpenInstallDirButtonActive => _openInstallDirButtonActive.Value;

        public bool OpenThemesDirButtonActive => _openThemesDirButtonActive.Value;

        public async Task InstallPlaylists(string filePath) => await _playlistInstaller.InstallPlaylistAsync(filePath, _progress);

        private static async Task ToggleOneClickHandler(bool active, string protocol, string description)
        {
            if (active) await Task.Run(() => PlatformUtils.RegisterProtocolHandler(protocol, description, nameof(BeatSaberModManager)));
            else await Task.Run(() => PlatformUtils.UnregisterProtocolHandler(protocol, nameof(BeatSaberModManager)));
        }
    }
}