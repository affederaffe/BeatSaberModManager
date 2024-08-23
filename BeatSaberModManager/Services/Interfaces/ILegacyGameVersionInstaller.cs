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
        /// <param name="gameVersion"></param>
        /// <param name="authenticator"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        Task<string?> InstallLegacyGameVersionAsync(IGameVersion gameVersion, ILegacyGameVersionAuthenticator authenticator, CancellationToken cancellationToken, IProgress<double>? progress = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameVersion"></param>
        /// <returns></returns>
        Task<bool> UninstallLegacyGameVersionAsync(IGameVersion gameVersion);
    }
}
