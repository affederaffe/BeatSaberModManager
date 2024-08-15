using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Versions;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;

using LibDepotDownloader;

using Serilog;


namespace BeatSaberModManager.Services.Implementations.GameVersions.Steam
{
    /// <inheritdoc />
    public class SteamLegacyGameVersionInstaller(Steam3Session steam3Session, ISteamAuthenticator steamAuthenticator, ILogger logger) : ILegacyGameVersionInstaller
    {
        /// <inheritdoc />
        public async Task<string?> InstallLegacyGameVersionAsync(IGameVersion gameVersion, CancellationToken cancellationToken, IProgress<double>? progress = null)
        {
            ArgumentNullException.ThrowIfNull(gameVersion);
            if (gameVersion is not SteamGameVersion steamLegacyGameVersion)
                return null;
            string appDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string legacyGameVersionsDirPath = Path.Join(appDataDirPath, ThisAssembly.Info.Product, "LegacyGameVersions", gameVersion.GameVersion);
            DownloadConfig downloadConfig = new() { InstallDirectory = legacyGameVersionsDirPath };
            await steam3Session.LoginAsync(steamAuthenticator, cancellationToken).ConfigureAwait(false);
            ContentDownloader contentDownloader = new(downloadConfig, steam3Session);
            List<(uint DepotId, ulong ManifestId)> depotManifestIds = [(620981, steamLegacyGameVersion.ManifestId)];
            try
            {
                string[]? installDirs = await contentDownloader.DownloadAppAsync(620980, depotManifestIds, SteamConstants.DefaultBranch, null, null, null, false, false, cancellationToken, progress).ConfigureAwait(false);
                return installDirs is { Length: 1 } ? installDirs[0] : null;
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e, "Failed to download depot");
                return null;
            }
        }

        /// <inheritdoc />
        public Task<bool> UninstallLegacyGameVersionAsync(IGameVersion gameVersion)
        {
            // TODO: doesn't work
            ArgumentNullException.ThrowIfNull(gameVersion);
            if (gameVersion is not SteamGameVersion)
                return Task.FromResult(false);
            string appDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string legacyGameVersionsDirPath = Path.Join(appDataDirPath, ThisAssembly.Info.Product, "LegacyGameVersions", gameVersion.GameVersion);
            return Task.FromResult(Utils.IOUtils.TryDeleteDirectory(legacyGameVersionsDirPath, true));
        }
    }
}
