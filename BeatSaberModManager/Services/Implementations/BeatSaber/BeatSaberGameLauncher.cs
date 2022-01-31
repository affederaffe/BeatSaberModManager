using System;
using System.Diagnostics;

using BeatSaberModManager.Models.Implementations;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    public class BeatSaberGameLauncher : IGameLauncher
    {
        public void LaunchGame(string installDir, PlatformType platformType)
        {
            switch (platformType)
            {
                case PlatformType.Unknown:
                    break;
                case PlatformType.Steam:
                    PlatformUtils.OpenUri($"steam://rungameid/{Constants.BeatSaberSteamId}");
                    break;
                case PlatformType.Oculus:
                    Process.Start(new ProcessStartInfo(Constants.BeatSaberExe) { WorkingDirectory = installDir });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platformType), platformType, null);
            }
        }
    }
}