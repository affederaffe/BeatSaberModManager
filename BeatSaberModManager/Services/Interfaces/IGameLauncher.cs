using BeatSaberModManager.Models.Implementations;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IGameLauncher
    {
        void LaunchGame(string installDir, PlatformType platformType);
    }
}