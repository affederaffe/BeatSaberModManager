using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Provides methods to locate an game's installation and detect the <see cref="PlatformType"/>.
    /// </summary>
    public interface IInstallDirLocator
    {
        /// <summary>
        /// Asynchronously locates a game's installation directory, optionally asynchronously.
        /// </summary>
        /// <returns>The installation directory of the game if found, null otherwise.</returns>
        ValueTask<string?> LocateInstallDirAsync();

        /// <summary>
        /// Detects the <see cref="PlatformType"/> of an installation.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <returns>The detected <see cref="PlatformType"/>.</returns>
        PlatformType DetectPlatform(string installDir);
    }
}