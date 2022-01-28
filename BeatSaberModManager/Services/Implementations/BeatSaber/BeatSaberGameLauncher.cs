using System.Diagnostics;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    public class BeatSaberGameLauncher : IGameLauncher
    {
        public void LaunchGame(string installDir, string platform)
        {
            switch (platform)
            {
                case "steam":
                    Process.Start(new ProcessStartInfo("steam://rungameid/620980") { UseShellExecute = true});
                    break;
                case "oculus":
                    Process.Start(new ProcessStartInfo("Beat Saber.exe") { WorkingDirectory = installDir });
                    break;
            }
        }
    }
}