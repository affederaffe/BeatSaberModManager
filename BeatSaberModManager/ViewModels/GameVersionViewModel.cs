using System.Reactive.Linq;

using BeatSaberModManager.Models.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// TODO
    /// </summary>
    public class GameVersionViewModel : ViewModelBase
    {
        private readonly ObservableAsPropertyHelper<bool> _isInstalled;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameVersion"></param>
        public GameVersionViewModel(IGameVersion gameVersion)
        {
            GameVersion = gameVersion;
            this.WhenAnyValue(static x => x.InstallDir)
                .Select(static x => x is not null)
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
            get => _installDir;
            set => this.RaiseAndSetIfChanged(ref _installDir, value);
        }

        private string? _installDir;

        /// <summary>
        /// 
        /// </summary>
        public bool IsInstalled => _isInstalled.Value;
    }
}
