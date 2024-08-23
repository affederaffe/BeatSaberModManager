using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.GameVersions.Steam
{
    /// <summary>
    /// TODO
    /// </summary>
    public class SteamLegacyGameVersionProvider(HttpProgressClient httpClient, IGameInstallLocator gameInstallLocator) : ILegacyGameVersionProvider
    {
        private IReadOnlyList<IGameVersion>? _availableGameVersions;

        /// <inheritdoc />
        public async Task<IReadOnlyList<IGameVersion>?> GetAvailableGameVersionsAsync()
        {
            if (_availableGameVersions is not null)
                return _availableGameVersions;
            using HttpResponseMessage response = await httpClient.TryGetAsync(new Uri("https://raw.githubusercontent.com/Zagrios/bs-manager/master/assets/jsons/bs-versions.json")).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return null;
            _availableGameVersions = await response.Content.ReadFromJsonAsync(GameVersionJsonSerializerContext.Default.SteamGameVersionArray).ConfigureAwait(false);
            return _availableGameVersions;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<IGameVersion>?> GetInstalledLegacyGameVersionsAsync()
        {
            string appDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string legacyGameVersionsDirPath = Path.Join(appDataDirPath, ThisAssembly.Info.Product, "LegacyGameVersions");
            List<IGameVersion> installedGameVersions = [];
            try
            {
                foreach (string installDir in Directory.EnumerateDirectories(legacyGameVersionsDirPath))
                {
                    IGameVersion? installedGameVersion = await gameInstallLocator.DetectLocalInstallTypeAsync(installDir).ConfigureAwait(false);
                    if (installedGameVersion is null)
                        continue;
                    installedGameVersions.Add(installedGameVersion);
                }
            }
            catch (Exception e) when (e is DirectoryNotFoundException or IOException)
            {
                return null;
            }

            return installedGameVersions.Count == 0 ? null : installedGameVersions;
        }
    }
}
