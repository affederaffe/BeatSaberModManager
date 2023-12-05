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
        /// The result of <see cref="LoadInstalledModsAsync"/>.
        /// </summary>
        IReadOnlyCollection<IMod>? InstalledMods { get; }

        /// <summary>
        /// The result of <see cref="LoadAvailableModsForVersionAsync"/>.
        /// </summary>
        IReadOnlyCollection<IMod>? AvailableMods { get; }

        /// <summary>
        /// Checks if the provided <see cref="IMod"/> is the mod loader.
        /// </summary>
        /// <param name="modification">The <see cref="IMod"/> to check.</param>
        /// <returns>True if the provided <see cref="IMod"/> is the mod loader, false otherwise.</returns>
        bool IsModLoader(IMod? modification);

        /// <summary>
        /// Gets the dependencies of an <see cref="IMod"/>.
        /// </summary>
        /// <param name="modification">The mod to get the dependencies from.</param>
        /// <returns>The mod's dependencies.</returns>
        IEnumerable<IMod> GetDependencies(IMod modification);

        /// <summary>
        /// Asynchronously loads all currently installed mods.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        Task LoadInstalledModsAsync(string installDir);

        /// <summary>
        /// Asynchronously loads all available mods for the specified version of the game
        /// </summary>
        /// <param name="version">The version of the game</param>
        Task LoadAvailableModsForVersionAsync(string version);

        /// <summary>
        /// Asynchronously downloads multiple mods.
        /// </summary>
        /// <param name="modification">The mod to download.</param>
        /// <returns>The mod's file as <see cref="ZipArchive"/>.</returns>
        Task<ZipArchive?> DownloadModAsync(IMod modification);
    }
}
