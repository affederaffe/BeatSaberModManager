using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Provides methods to locate a game's installation and detect its type.
    /// </summary>
    public interface IGameInstallLocator
    {
        /// <summary>
        /// Asynchronously locates a game's installation.
        /// </summary>
        /// <returns>The installation of the game if found, null otherwise.</returns>
        Task<IGameVersion?> LocateGameInstallAsync();

        /// <summary>
        /// Asynchronously detects the platform typ (Oculus or Steam) for a given directory.
        /// </summary>
        /// <param name="installDir">The installation directory of the game.</param>
        /// <returns>The installation of the game if valid, null otherwise.</returns>
        Task<IGameVersion?> DetectLocalInstallTypeAsync(string installDir);
    }
}
