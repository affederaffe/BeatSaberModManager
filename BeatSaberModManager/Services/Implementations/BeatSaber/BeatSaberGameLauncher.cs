using System;
using System.Diagnostics;
using System.IO;

using BeatSaberModManager.Models.Implementations;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    /// <inheritdoc />
    public class BeatSaberGameLauncher(IInstallDirLocator installDirLocator) : IGameLauncher
    {
        /// <inheritdoc />
        public void LaunchGame(string installDir)
        {
            switch (installDirLocator.DetectPlatform(installDir))
            {
                case PlatformType.Steam when new DirectoryInfo(installDir).Parent?.Parent?.Name == "steamapps":
                    PlatformUtils.TryOpenUri(new Uri("steam://rungameid/620980"));
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
