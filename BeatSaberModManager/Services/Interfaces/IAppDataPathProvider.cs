namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Defines a method to get a game's AppData directory.
    /// </summary>
    public interface IAppDataPathProvider
    {
        /// <summary>
        /// Gets the AppData directory of a game.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <returns>The game's AppData directory or null if not found.</returns>
        string GetAppDataPath(string installDir);
    }
}
