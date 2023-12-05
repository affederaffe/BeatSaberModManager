using BeatSaberModManager.Models.Interfaces;


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
        /// <param name="gameVersion">The targeted installation of the game.</param>
        /// <returns>The path of the game's AppData directory.</returns>
        string GetAppDataPath(IGameVersion gameVersion);

        /// <summary>
        /// Gets the Logs directory of a game.
        /// </summary>
        /// <param name="gameVersion">The targeted installation of the game.</param>
        /// <returns>The path of the game's Logs directory.</returns>
        string GetLogsPath(IGameVersion gameVersion);
    }
}
