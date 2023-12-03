using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Provides a method to launch the game.
    /// </summary>
    public interface IGameLauncher
    {
        /// <summary>
        /// Launches the game.
        /// </summary>
        /// <param name="gameVersion">The game version to start.</param>
        void LaunchGame(IGameVersion gameVersion);
    }
}
