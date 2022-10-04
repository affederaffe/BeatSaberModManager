using System.Threading.Tasks;

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
        /// <param name="modification">The mod to install.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        Task<bool> InstallModAsync(string installDir, IMod modification);

        /// <summary>
        /// Asynchronously uninstalls multiple mods.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="modification">The mod to uninstall.</param>
        Task UninstallModAsync(string installDir, IMod modification);

        /// <summary>
        /// Removes all installed mods.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        void RemoveAllModFiles(string installDir);
    }
}
