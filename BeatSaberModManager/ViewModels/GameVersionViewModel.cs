using System;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// TODO
    /// </summary>
    public sealed class GameVersionViewModel : ViewModelBase
    {
        private readonly ObservableAsPropertyHelper<bool> _isInstalled;

        /// <summary>
        /// 
        /// </summary>
        public GameVersionViewModel(IGameVersion gameVersion, IInstallDirValidator installDirValidator)
        {
            ArgumentNullException.ThrowIfNull(installDirValidator);
            GameVersion = gameVersion;
            this.WhenAnyValue(static x => x.InstallDir)
                .Select(installDirValidator.ValidateInstallDir)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, static x => x.IsInstalled, out _isInstalled);
        }

        /// <summary>
        /// 
        /// </summary>
        public IGameVersion GameVersion { get; }

        /// <summary>
        /// The directory the version is installed at.
        /// </summary>
        public string? InstallDir
        {
            get => GameVersion.InstallDir;
            set
            {
                if (GameVersion.InstallDir == value)
                    return;
                this.RaisePropertyChanging();
                GameVersion.InstallDir = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsInstalled => _isInstalled.Value;
    }
}
