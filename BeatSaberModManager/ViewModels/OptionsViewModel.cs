using System;
using System.Collections.ObjectModel;
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
        private readonly ObservableAsPropertyHelper<bool> _openFolderButtonActive;

        public OptionsViewModel(ModsViewModel modsViewModel, Settings settings, IInstallDirValidator installDirValidator)
        {
            _settings = settings;
            _installDirValidator = installDirValidator;
            OpenInstallDirCommand = ReactiveCommand.CreateFromTask(OpenInstallDir);
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync);
            IObservable<string?> installDirObservable = this.WhenAnyValue(x => x.InstallDir);
            installDirObservable.Select(x => x is not null).ToProperty(this, nameof(OpenFolderButtonActive), out _openFolderButtonActive);
        }

        public string? InstallDir
        {
            get => _settings.InstallDir;
            set => ValidateRaiseAndSetDir(value);
        }

        public ReactiveCommand<Unit, Unit> OpenInstallDirCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallModLoaderCommand { get; }

        public bool OpenFolderButtonActive => _openFolderButtonActive.Value;

        private async Task OpenInstallDir() => await PlatformUtils.OpenBrowserOrFileExplorer(_settings.InstallDir!);

        private void ValidateRaiseAndSetDir(string? path)
        {
            if (!_installDirValidator.ValidateInstallDir(path)) return;
            this.RaisePropertyChanging(nameof(InstallDir));
            _settings.VRPlatform = _installDirValidator.DetectVRPlatform(path!);
            _settings.InstallDir = path;
            this.RaisePropertyChanged(nameof(InstallDir));
        }
    }
}