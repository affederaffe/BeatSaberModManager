using System;
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
            switch (_installDirLocator.DetectPlatform(installDir))
            {
                case PlatformType.Steam:
                    PlatformUtils.TryOpenUri("steam://rungameid/620980");
                    break;
                case PlatformType.Oculus:
                    PlatformUtils.TryStartProcess(new ProcessStartInfo("Beat Saber.exe") { WorkingDirectory = installDir }, out _);
                    break;
                default:
                    throw new InvalidOperationException("Could not detect platform.");
            }
        }
    }
}