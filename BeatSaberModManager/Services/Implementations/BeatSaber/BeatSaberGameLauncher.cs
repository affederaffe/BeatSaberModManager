using System;
using System.Diagnostics;

using BeatSaberModManager.Models.Implementations.Versions;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    /// <inheritdoc />
    public class BeatSaberGameLauncher : IGameLauncher
    {
        /// <inheritdoc />
        public void LaunchGame(IGameVersion gameVersion)
        {
            switch (gameVersion)
            {
                case SteamGameVersion steamGameVersion:
                    PlatformUtils.TryOpenUri(new Uri("steam://rungameid/620980"));
                    break;
                case OculusGameVersion oculusGameVersion:
                    PlatformUtils.TryStartProcess(new ProcessStartInfo("Beat Saber.exe") { WorkingDirectory = oculusGameVersion.InstallDir }, out _);
                    break;
                default:
                    throw new InvalidOperationException("Could not detect platform.");
            }
        }
    }
}
