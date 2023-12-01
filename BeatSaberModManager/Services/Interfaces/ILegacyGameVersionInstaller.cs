using System;
using System.Threading;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// TODO
    /// </summary>
    public interface ILegacyGameVersionInstaller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="legacyGameVersion"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        Task<string?> InstallLegacyGameVersionAsync(ILegacyGameVersion legacyGameVersion, CancellationToken cancellationToken, IProgress<double>? progress = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="legacyGameVersion"></param>
        /// <returns></returns>
        Task<bool> UninstallLegacyGameVersionAsync(ILegacyGameVersion legacyGameVersion);
    }
}
