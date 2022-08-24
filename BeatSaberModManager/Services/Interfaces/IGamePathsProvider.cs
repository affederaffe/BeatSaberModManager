namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Defines a method to get a game's AppData directory.
    /// </summary>
    public interface IGamePathsProvider
    {
        /// <summary>
        /// Gets the AppData directory of a game.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <returns>The path of the game's AppData directory.</returns>
        string GetAppDataPath(string installDir);

        /// <summary>
        /// Gets the Logs directory of a game.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <returns>The path of the game's Logs directory.</returns>
        string GetLogsPath(string installDir);
    }
}
