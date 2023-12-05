using System.Reactive.Linq;

using BeatSaberModManager.Models.Interfaces;

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
        /// <param name="gameVersion"></param>
        public GameVersionViewModel(IGameVersion gameVersion)
        {
            GameVersion = gameVersion;
            this.WhenAnyValue(static x => x.GameVersion)
                .Select(static x => x is { InstallDir: not null })
                .ToProperty(this, nameof(IsInstalled), out _isInstalled);
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
            get => _installDir ??= GameVersion.InstallDir;
            set => GameVersion.InstallDir = this.RaiseAndSetIfChanged(ref _installDir, value);
        }

        private string? _installDir;

        /// <summary>
        /// 
        /// </summary>
        public bool IsInstalled => _isInstalled.Value;
    }
}
