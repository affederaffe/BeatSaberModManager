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
    public class BeatSaverMapInstaller
    {
        private readonly HttpProgressClient _httpClient;

        public BeatSaverMapInstaller(HttpProgressClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> InstallBeatSaverMapAsync(string installDir, string key, IStatusProgress? progress = null)
        {
            BeatSaverMap? map = await GetBeatSaverMapAsync(key).ConfigureAwait(false);
            if (map is null || map.Versions.Length <= 0) return false;
            progress?.Report(new ProgressInfo(StatusType.Installing, map.Name));
            using ZipArchive? archive = await DownloadBeatSaverMapAsync(map.Versions.Last(), progress).ConfigureAwait(false);
            return archive is not null && ExtractBeatSaverMapToDir(installDir, map, archive);
        }

        public async Task<BeatSaverMap?> GetBeatSaverMapAsync(string key, int retries = 2)
        {
            while (retries != 0)
            {
                using HttpResponseMessage response = await _httpClient.GetAsync($"https://api.beatsaver.com/maps/id/{key}").ConfigureAwait(false);
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

        public static bool ExtractBeatSaverMapToDir(string installDir, BeatSaverMap map, ZipArchive archive)
        {
            string customLevelsDirectoryPath = Path.Combine(installDir, Constants.BeatSaberDataDir, "CustomLevels");
            IOUtils.TryCreateDirectory(customLevelsDirectoryPath);
            string mapName = string.Concat($"{map.Id} ({map.MetaData.SongName} - {map.MetaData.LevelAuthorName})".Split(Path.GetInvalidFileNameChars()));
            string levelDirectoryPath = Path.Combine(customLevelsDirectoryPath, mapName);
            return IOUtils.TryExtractArchive(archive, levelDirectoryPath, true);
        }

        private static Task WaitForRateLimitAsync() => Task.Delay(1000);
    }
}