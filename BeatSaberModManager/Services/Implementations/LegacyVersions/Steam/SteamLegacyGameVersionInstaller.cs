using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.LegacyVersions;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;

using LibDepotDownloader;

using Serilog;

using SteamKit2;


namespace BeatSaberModManager.Services.Implementations.LegacyVersions.Steam
{
    /// <inheritdoc />
    public class SteamLegacyGameVersionInstaller(ISteamAuthenticator steamAuthenticator, ILogger logger) : ILegacyGameVersionInstaller
    {
        /// <inheritdoc />
        public async Task<string?> InstallLegacyGameVersionAsync(ILegacyGameVersion legacyGameVersion, CancellationToken cancellationToken, IProgress<double>? progress = null)
        {
            ArgumentNullException.ThrowIfNull(legacyGameVersion);
            if (legacyGameVersion is not SteamLegacyGameVersion steamLegacyGameVersion)
                return null;
            string appDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string legacyGameVersionsDirPath = Path.Join(appDataDirPath, ThisAssembly.Info.Product, "LegacyGameVersions", legacyGameVersion.GameVersion);
            (string? username, string? password, bool rememberLogin) = await steamAuthenticator.ProvideLoginDetailsAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;
            DownloadConfig downloadConfig = new() { InstallDirectory = legacyGameVersionsDirPath, RememberPassword = rememberLogin };
            SteamUser.LogOnDetails logOnDetails = new() { Username = username, Password = password };
            Steam3Session steam3Session = new(downloadConfig, logOnDetails, steamAuthenticator);
            await steam3Session.LoginAsync(cancellationToken).ConfigureAwait(false);
            ContentDownloader contentDownloader = new(downloadConfig, steam3Session);
            List<(uint DepotId, ulong ManifestId)> depotManifestIds = [(620981, steamLegacyGameVersion.ManifestId)];
            try
            {
                string[]? installDirs = await contentDownloader.DownloadAppAsync(620980, depotManifestIds, SteamConstants.DefaultBranch, null, null, null, false, false, cancellationToken, progress).ConfigureAwait(false);
                return installDirs is { Length: 1 } ? installDirs[0] : null;
            }
            catch (InvalidCastException e)
            {
                logger.Error(e, "Failed to download depot");
                return null;
            }
        }

        /// <inheritdoc />
        public Task<bool> UninstallLegacyGameVersionAsync(ILegacyGameVersion legacyGameVersion) => throw new NotImplementedException();
    }
}
