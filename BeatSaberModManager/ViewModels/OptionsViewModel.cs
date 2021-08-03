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
        private readonly ObservableAsPropertyHelper<bool> _openInstallDirButtonActive;
        private readonly ObservableAsPropertyHelper<bool> _openThemesDirButtonActive;

        private const string kProtocolProviderName = "BeatSaberModManager";

        public OptionsViewModel(ModsViewModel modsViewModel, Settings settings, IInstallDirValidator installDirValidator)
        {
            _settings = settings;
            OpenInstallDirCommand = ReactiveCommand.CreateFromTask(() => PlatformUtils.OpenBrowserOrFileExplorer(_settings.InstallDir!));
            OpenThemesDirCommand = ReactiveCommand.CreateFromTask(() => PlatformUtils.OpenBrowserOrFileExplorer(_settings.ThemesDir!));
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync);
            ToggleBeatSaverOneClickHandlerCommand = ReactiveCommand.CreateFromTask(() => Task.Run(() => ToggleOneClickHandler(BeatSaverOneClickCheckboxChecked, "beatsaver", "URI:BeatSaver OneClick Install")));
            ToggleModelSaberOneClickHandlerCommand = ReactiveCommand.CreateFromTask(() => Task.Run(() => ToggleOneClickHandler(BeatSaverOneClickCheckboxChecked, "modelsaber", "URI:ModelSaber OneClick Install")));
            IObservable<string?> installDirObservable = this.WhenAnyValue(x => x.InstallDir);
            IObservable<string> validatedDirObservable = installDirObservable.Where(installDirValidator.ValidateInstallDir)!;
            validatedDirObservable.Subscribe(x => _settings.InstallDir = x);
            validatedDirObservable.Subscribe(x => _settings.VRPlatform = installDirValidator.DetectVRPlatform(x));
            installDirObservable.Select(x => !string.IsNullOrEmpty(x)).ToProperty(this, nameof(OpenInstallDirButtonActive), out _openInstallDirButtonActive);
            this.WhenAnyValue(x => x.ThemesDir).Select(x => !string.IsNullOrEmpty(x)).ToProperty(this, nameof(OpenThemesDirButtonActive), out _openThemesDirButtonActive);
        }

        private string? _installDir;
        public string? InstallDir
        {
            get => _installDir ??= _settings.InstallDir;
            set => this.RaiseAndSetIfChanged(ref _installDir, value);
        }

        private string? _themesDir;
        public string? ThemesDir
        {
            get => _settings.ThemesDir ??= _settings.ThemesDir;
            set => this.RaiseAndSetIfChanged(ref _themesDir, value);
        }

        private bool _beatSaverOneClickCheckboxChecked = PlatformUtils.IsProtocolHandlerRegistered("beatsaver", kProtocolProviderName);
        public bool BeatSaverOneClickCheckboxChecked
        {
            get => _beatSaverOneClickCheckboxChecked;
            set => this.RaiseAndSetIfChanged(ref _beatSaverOneClickCheckboxChecked, value);
        }

        private bool _modelSaberOneClickCheckboxChecked = PlatformUtils.IsProtocolHandlerRegistered("modelsaber", kProtocolProviderName);
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

        private static void ToggleOneClickHandler(bool active, string protocol, string description)
        {
            if (active) PlatformUtils.RegisterProtocolHandler(protocol, description, kProtocolProviderName);
            else PlatformUtils.UnregisterProtocolHandler(protocol, kProtocolProviderName);
        }
    }
}