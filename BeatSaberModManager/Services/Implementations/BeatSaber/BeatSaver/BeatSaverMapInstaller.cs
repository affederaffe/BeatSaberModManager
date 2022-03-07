using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatSaver
{
    /// <summary>
    /// Download and install <see cref="BeatSaverMap"/>s from https://beatsaver.com.
    /// </summary>
    public class BeatSaverMapInstaller
    {
        private readonly HttpProgressClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeatSaverMapInstaller"/> class.
        /// </summary>
        public BeatSaverMapInstaller(HttpProgressClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Asynchronously downloads and installs a beatmap from https://beatsaver.com by key.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="key">The unique key for the map.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        public async Task<bool> InstallBeatSaverMapByKeyAsync(string installDir, string key, IStatusProgress? progress = null)
        {
            BeatSaverMap? map = await GetBeatSaverMapByKeyAsync(key).ConfigureAwait(false);
            return await TryInstallBeatSaverMapAsync(installDir, map, progress).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously downloads and installs a beatmap from https://beatsaver.com by hash.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="hash">The hash of the map.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        public async Task<bool> InstallBeatSaberMapByHashAsync(string installDir, string hash, IStatusProgress? progress = null)
        {
            BeatSaverMap? map = await GetBeatSaverMapByHashAsync(hash).ConfigureAwait(false);
            return await TryInstallBeatSaverMapAsync(installDir, map, progress).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously downloads a <see cref="BeatSaverMap"/> from https://beatsaver.com by key.
        /// </summary>
        /// <param name="key">The unique key for the map.</param>
        /// <param name="retries">How many times the operation should be re-run before failing.</param>
        /// <returns>The <see cref="BeatSaverMap"/> if the operation succeeds, null otherwise.</returns>
        public Task<BeatSaverMap?> GetBeatSaverMapByKeyAsync(string key, int retries = 2) => GetBeatSaverMapAsync($"https://api.beatsaver.com/maps/id/{key}", retries);

        /// <summary>
        /// Asynchronously downloads a <see cref="BeatSaverMap"/> from https://beatsaver.com by hash.
        /// </summary>
        /// <param name="hash">The hash of the map.</param>
        /// <param name="retries">How many times the operation should be re-run before failing.</param>
        /// <returns>The <see cref="BeatSaverMap"/> if the operation succeeds, null otherwise.</returns>
        public Task<BeatSaverMap?> GetBeatSaverMapByHashAsync(string hash, int retries = 2) => GetBeatSaverMapAsync($"https://api.beatsaver.com/maps/hash/{hash}", retries);

        /// <summary>
        /// Attempts to install a beatmap into the game's installation directory.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="map">The map to install.</param>
        /// <param name="archive">The content of the <see cref="BeatSaverMap"/>.</param>
        /// <returns>True when the operation succeeds, false otherwise.</returns>
        public static bool TryExtractBeatSaverMapToDir(string installDir, BeatSaverMap map, ZipArchive archive)
        {
            string customLevelsDirectoryPath = Path.Join(installDir, "Beat Saber_Data", "CustomLevels");
            IOUtils.TryCreateDirectory(customLevelsDirectoryPath);
            string mapName = string.Concat($"{map.Id} ({map.MetaData.SongName} - {map.MetaData.LevelAuthorName})".Split(Path.GetInvalidFileNameChars()));
            string levelDirectoryPath = Path.Join(customLevelsDirectoryPath, mapName);
            return IOUtils.TryExtractArchive(archive, levelDirectoryPath, true);
        }

        private async Task<BeatSaverMap?> GetBeatSaverMapAsync(string url, int retries = 2)
        {
            while (retries != 0)
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    await WaitForRateLimitAsync().ConfigureAwait(false);
                    retries--;
                    continue;
                }

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonSerializer.Deserialize(body, BeatSaverJsonSerializerContext.Default.BeatSaverMap);
            }

            return null;
        }

        private async Task<ZipArchive?> DownloadBeatSaverMapAsync(BeatSaverMapVersion mapVersion, IProgress<double>? progress = null, int retries = 2)
        {
            while (retries != 0)
            {
                HttpResponseMessage response = await _httpClient.GetAsync(mapVersion.DownloadUrl, progress).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    await WaitForRateLimitAsync().ConfigureAwait(false);
                    retries--;
                    continue;
                }

                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                return new ZipArchive(stream);
            }

            return null;
        }

        private async Task<bool> TryInstallBeatSaverMapAsync(string installDir, BeatSaverMap? map, IStatusProgress? progress = null)
        {
            if (map is null || map.Versions.Length <= 0) return false;
            progress?.Report(new ProgressInfo(StatusType.Installing, map.Name));
            using ZipArchive? archive = await DownloadBeatSaverMapAsync(map.Versions.Last(), progress).ConfigureAwait(false);
            return archive is not null && TryExtractBeatSaverMapToDir(installDir, map, archive);
        }

        private static Task WaitForRateLimitAsync() => Task.Delay(1000);
    }
}