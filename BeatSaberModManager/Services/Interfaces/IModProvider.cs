using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Provides methods to download mods, get dependencies and check for installed ones.
    /// </summary>
    public interface IModProvider
    {
        /// <summary>
        /// Checks if the provided <see cref="IMod"/> is the mod loader.
        /// </summary>
        /// <param name="modification">The <see cref="IMod"/> to check.</param>
        /// <returns>true if the provided <see cref="IMod"/> is the mod loader, false otherwise.</returns>
        bool IsModLoader(IMod? modification);

        /// <summary>
        /// Gets the dependencies of an <see cref="IMod"/>.
        /// </summary>
        /// <param name="modification">The mod to get the dependencies from.</param>
        /// <returns>The mod's dependencies.</returns>
        IEnumerable<IMod> GetDependencies(IMod modification);

        /// <summary>
        /// Asynchronously gets all currently installed mods.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <returns>All currently installed mods.</returns>
        Task<IReadOnlyCollection<IMod>?> GetInstalledModsAsync(string installDir);

        /// <summary>
        /// Asynchronously gets all available mods for the current version of the game.
        /// </summary>
        /// <param name="installDir">The game's installation directory</param>
        /// <returns>All available mods for the current version of the game</returns>
        Task<IReadOnlyCollection<IMod>?> GetAvailableModsForCurrentVersionAsync(string installDir);

        /// <summary>
        /// Asynchronously gets all available mods for the specified version of the game
        /// </summary>
        /// <param name="version">The version of the game</param>
        /// <returns>All available mods for the specified version of the game</returns>
        Task<IReadOnlyCollection<IMod>?> GetAvailableModsForVersionAsync(string version);

        /// <summary>
        /// Asynchronously downloads multiple mods.
        /// </summary>
        /// <param name="mods">The mods to download.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <returns>The mod's files as <see cref="ZipArchive"/>s.</returns>
        IAsyncEnumerable<ZipArchive> DownloadModsAsync(IEnumerable<IMod> mods, IProgress<double>? progress = null);
    }
}