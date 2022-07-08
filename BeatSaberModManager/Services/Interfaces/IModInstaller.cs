using System.Collections.Generic;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Provides methods to install and uninstall mods.
    /// </summary>
    public interface IModInstaller
    {
        /// <summary>
        /// Asynchronously installs multiple mods.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="mods">The mods to install.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <returns>The successfully installed mods.</returns>
        IAsyncEnumerable<IMod> InstallModsAsync(string installDir, IEnumerable<IMod> mods, IStatusProgress? progress = null);

        /// <summary>
        /// Asynchronously uninstalls multiple mods.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="mods">The mods to uninstall.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <returns>The successfully uninstalled mods.</returns>
        IAsyncEnumerable<IMod> UninstallModsAsync(string installDir, IEnumerable<IMod> mods, IStatusProgress? progress = null);

        /// <summary>
        /// Removes all installed mods.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        void RemoveAllMods(string installDir);
    }
}
