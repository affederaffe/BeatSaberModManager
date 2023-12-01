using System.Collections.Generic;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILegacyGameVersionProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<ILegacyGameVersion>?> GetAvailableGameVersionsAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<(ILegacyGameVersion GameVersion, string InstallDir)>?> GetInstalledLegacyGameVersionsAsync();
    }
}
