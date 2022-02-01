using System.Diagnostics;

using BeatSaberModManager.Models.Implementations;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    public class BeatSaberGameLauncher : IGameLauncher
    {
        private readonly IInstallDirLocator _installDirLocator;

        public BeatSaberGameLauncher(IInstallDirLocator installDirLocator)
        {
            _installDirLocator = installDirLocator;
        }

        public void LaunchGame(string installDir)
        {
            if (_installDirLocator.DetectPlatform(installDir) == PlatformType.Steam)
                PlatformUtils.TryOpenUri("steam://rungameid/620980");
            else if (_installDirLocator.DetectPlatform(installDir) == PlatformType.Oculus)
                PlatformUtils.TryStartProcess(new ProcessStartInfo("Beat Saber.exe") { WorkingDirectory = installDir }, out _);
        }
    }
}