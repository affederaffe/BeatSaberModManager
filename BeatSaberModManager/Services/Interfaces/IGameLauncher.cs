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
        /// <param name="installDir">The game's installation directory.</param>
        void LaunchGame(string installDir);
    }
}
