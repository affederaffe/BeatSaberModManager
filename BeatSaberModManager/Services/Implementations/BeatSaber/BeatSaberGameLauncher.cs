using System.Diagnostics;

using BeatSaberModManager.Models.Implementations;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    /// <inheritdoc />
    public class BeatSaberGameLauncher : IGameLauncher
    {
        private readonly IInstallDirLocator _installDirLocator;

        /// <summary>
        /// Initializes a new <see cref="BeatSaberGameLauncher"/> instance.
        /// </summary>
        public BeatSaberGameLauncher(IInstallDirLocator installDirLocator)
        {
            _installDirLocator = installDirLocator;
        }

        /// <inheritdoc />
        public void LaunchGame(string installDir)
        {
            if (_installDirLocator.DetectPlatform(installDir) == PlatformType.Steam)
                PlatformUtils.TryOpenUri("steam://rungameid/620980");
            else if (_installDirLocator.DetectPlatform(installDir) == PlatformType.Oculus)
                PlatformUtils.TryStartProcess(new ProcessStartInfo("Beat Saber.exe") { WorkingDirectory = installDir }, out _);
        }
    }
}