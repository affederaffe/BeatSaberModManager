using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Provides a method to detect a game's version.
    /// </summary>
    public interface IGameVersionProvider
    {
        /// <summary>
        /// Asynchronously detects a game's version.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <returns>The game's version, or null when failed.</returns>
        Task<string?> DetectGameVersionAsync(string installDir);
    }
}