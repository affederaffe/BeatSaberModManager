using System.Reactive;
using System.Reactive.Linq;

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

        private const string kProtocolProviderName = "BeatSaberModManager";

        public OptionsViewModel(ModsViewModel modsViewModel, Settings settings, IInstallDirValidator installDirValidator)
        {
            _settings = settings;
            _installDirValidator = installDirValidator;
            OpenInstallDirCommand = ReactiveCommand.CreateFromTask(() => PlatformUtils.OpenBrowserOrFileExplorer(_settings.InstallDir!));
            OpenThemesDirCommand = ReactiveCommand.CreateFromTask(() => PlatformUtils.OpenBrowserOrFileExplorer(_settings.ThemesDir!));
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync);
            this.WhenAnyValue(x => x.InstallDir).Select(x => x is not null).ToProperty(this, nameof(OpenInstallDirButtonActive), out _openInstallDirButtonActive);
            this.WhenAnyValue(x => x.ThemesDir).Select(x => x is not null).ToProperty(this, nameof(OpenThemesDirButtonActive), out _openThemesDirButtonActive);
        }

        public string? InstallDir
        {
            get => _settings.InstallDir;
            set
            {
                if (!_installDirValidator.ValidateInstallDir(value)) return;
                this.RaisePropertyChanging(nameof(InstallDir));
                _settings.VRPlatform = _installDirValidator.DetectVRPlatform(value!);
                _settings.InstallDir = value;
                this.RaisePropertyChanged(nameof(InstallDir));
            }
        }

        public string? ThemesDir
        {
            get => _settings.ThemesDir;
            set
            {
                this.RaisePropertyChanging(nameof(ThemesDir));
                _settings.ThemesDir = value;
                this.RaisePropertyChanged(nameof(ThemesDir));
            }
        }

        public bool BeatSaverOneClickCheckboxChecked
        {
            get => PlatformUtils.IsProtocolHandlerRegistered("beatsaver", kProtocolProviderName);
            set => ToggleOneClickHandler(nameof(BeatSaverOneClickCheckboxChecked), value, "beatsaver", "URI:BeatSaver OneClick Install");
        }

        public bool ModelSaberOneClickCheckboxChecked
        {
            get => PlatformUtils.IsProtocolHandlerRegistered("modelsaber", kProtocolProviderName);
            set => ToggleOneClickHandler(nameof(BeatSaverOneClickCheckboxChecked), value, "modelsaber", "URI:ModelSaber OneClick Install");
        }

        public ReactiveCommand<Unit, Unit> OpenInstallDirCommand { get; }

        public ReactiveCommand<Unit, Unit> OpenThemesDirCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallModLoaderCommand { get; }

        public bool OpenInstallDirButtonActive => _openInstallDirButtonActive.Value;

        public bool OpenThemesDirButtonActive => _openThemesDirButtonActive.Value;

        private void ToggleOneClickHandler(string propertyName, bool checkboxChecked, string protocol, string description)
        {
            if (checkboxChecked)
                PlatformUtils.RegisterProtocolHandler(protocol, description, kProtocolProviderName);
            else
                PlatformUtils.UnregisterProtocolHandler(protocol, kProtocolProviderName);
            this.RaisePropertyChanged(propertyName);
        }
    }
}