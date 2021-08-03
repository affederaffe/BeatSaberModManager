using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class OptionsViewModel : ReactiveObject
    {
        private readonly Settings _settings;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly ObservableAsPropertyHelper<bool> _openInstallDirButtonActive;
        private readonly ObservableAsPropertyHelper<bool> _openThemesDirButtonActive;

        public OptionsViewModel(ModsViewModel modsViewModel, Settings settings, IInstallDirValidator installDirValidator)
        {
            _settings = settings;
            _installDirValidator = installDirValidator;
            OpenInstallDirCommand = ReactiveCommand.CreateFromTask(() => PlatformUtils.OpenBrowserOrFileExplorer(_settings.InstallDir!));
            OpenThemesDirCommand = ReactiveCommand.CreateFromTask(() => PlatformUtils.OpenBrowserOrFileExplorer(_settings.ThemesDir!));
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync);
            ToggleBeatSaverOneClickHandlerCommand = ReactiveCommand.CreateFromTask(() => ToggleOneClickHandler(BeatSaverOneClickCheckboxChecked, "beatsaver", "URI:BeatSaver OneClick Install"));
            ToggleModelSaberOneClickHandlerCommand = ReactiveCommand.CreateFromTask(() => ToggleOneClickHandler(BeatSaverOneClickCheckboxChecked, "modelsaber", "URI:ModelSaber OneClick Install"));
            IObservable<string?> installDirObservable = this.WhenAnyValue(x => x.InstallDir);
            installDirObservable.BindTo(_settings, x => x.InstallDir);
            installDirObservable.Subscribe(x => _settings.VRPlatform = _installDirValidator.DetectVRPlatform(x!));
            installDirObservable.Select(x => !string.IsNullOrEmpty(x)).ToProperty(this, nameof(OpenInstallDirButtonActive), out _openInstallDirButtonActive);
            IObservable<string?> themesDirObservable = this.WhenAnyValue(x => x.ThemesDir);
            themesDirObservable.BindTo(_settings, x => x.ThemesDir);
            themesDirObservable.Select(x => !string.IsNullOrEmpty(x)).ToProperty(this, nameof(OpenThemesDirButtonActive), out _openThemesDirButtonActive);
        }

        private string? _installDir;
        public string? InstallDir
        {
            get => _installDir ??= _settings.InstallDir;
            set => this.RaiseAndSetIfChangedConditional(ref _installDir, value, _installDirValidator.ValidateInstallDir);
        }

        private string? _themesDir;
        public string? ThemesDir
        {
            get => _themesDir ??= _settings.ThemesDir;
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

        public ReactiveCommand<Unit, Unit> OpenInstallDirCommand { get; }

        public ReactiveCommand<Unit, Unit> OpenThemesDirCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallModLoaderCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallAllModsCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleBeatSaverOneClickHandlerCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleModelSaberOneClickHandlerCommand { get; }

        public bool OpenInstallDirButtonActive => _openInstallDirButtonActive.Value;

        public bool OpenThemesDirButtonActive => _openThemesDirButtonActive.Value;

        private static async Task ToggleOneClickHandler(bool active, string protocol, string description)
        {
            if (active) await Task.Run(() => PlatformUtils.RegisterProtocolHandler(protocol, description, nameof(BeatSaberModManager)));
            else await Task.Run(() => PlatformUtils.UnregisterProtocolHandler(protocol, nameof(BeatSaberModManager)));
        }
    }
}