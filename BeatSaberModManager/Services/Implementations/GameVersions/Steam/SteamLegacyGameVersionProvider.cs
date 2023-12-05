using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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
    public class SteamLegacyGameVersionProvider(HttpProgressClient httpClient, IGameVersionProvider gameVersionProvider) : ILegacyGameVersionProvider
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
#pragma warning disable CA2007
            await using Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#pragma warning restore CA2007
            _availableGameVersions = await JsonSerializer.DeserializeAsync(contentStream, GameVersionJsonSerializerContext.Default.SteamGameVersionArray).ConfigureAwait(false);
            return _availableGameVersions;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<IGameVersion>?> GetInstalledLegacyGameVersionsAsync()
        {
            IReadOnlyList<IGameVersion>? availableGameVersions = await GetAvailableGameVersionsAsync().ConfigureAwait(false);
            if (availableGameVersions is null)
                return null;
            string appDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string legacyGameVersionsDirPath = Path.Join(appDataDirPath, ThisAssembly.Info.Product, "LegacyGameVersions");
            List<IGameVersion> installedGameVersions = [];
            try
            {
                foreach (string dir in Directory.EnumerateDirectories(legacyGameVersionsDirPath))
                {
                    string? installedVersion = await gameVersionProvider.DetectGameVersionAsync(dir).ConfigureAwait(false);
                    if (installedVersion is null)
                        continue;
                    IGameVersion? installedGameVersion = availableGameVersions.FirstOrDefault(version => version.GameVersion == installedVersion);
                    if (installedGameVersion is null)
                        continue;
                    installedGameVersion.InstallDir = dir;
                    installedGameVersions.Add(installedGameVersion);
                }
            }
            catch (Exception e) when(e is DirectoryNotFoundException or IOException)
            {
                return null;
            }

            return installedGameVersions;
        }
    }
}
