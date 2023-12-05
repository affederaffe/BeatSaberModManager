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
        /// <param name="gameVersion">The targeted installation of the game.</param>
        /// <param name="modification">The mod to install.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        Task<bool> InstallModAsync(IGameVersion gameVersion, IMod modification);

        /// <summary>
        /// Asynchronously uninstalls multiple mods.
        /// </summary>
        /// <param name="gameVersion">The targeted installation of the game.</param>
        /// <param name="modification">The mod to uninstall.</param>
        Task UninstallModAsync(IGameVersion gameVersion, IMod modification);

        /// <summary>
        /// Removes all installed mods.
        /// </summary>
        /// <param name="gameVersion">The targeted installation of the game.</param>
        void RemoveAllModFiles(IGameVersion gameVersion);
    }
}
