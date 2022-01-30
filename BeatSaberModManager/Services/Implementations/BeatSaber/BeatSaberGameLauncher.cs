using System.Diagnostics;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    public class BeatSaberGameLauncher : IGameLauncher
    {
        public void LaunchGame(string installDir, string platform)
        {
            switch (platform)
            {
                case Constants.Steam:
                    Process.Start(new ProcessStartInfo($"steam://rungameid/{Constants.BeatSaberSteamId}") { UseShellExecute = true });
                    break;
                case Constants.Oculus:
                    Process.Start(new ProcessStartInfo(Constants.BeatSaberExe) { WorkingDirectory = installDir });
                    break;
            }
        }
    }
}