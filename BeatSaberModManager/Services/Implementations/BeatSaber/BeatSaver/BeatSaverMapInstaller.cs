using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Models.Implementations.JsonSerializerContexts;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utilities;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverMapInstaller
    {
        private readonly HttpProgressClient _httpClient;

        private const string kBeatSaverUrlPrefix = "https://api.beatsaver.com";
        private const string kBeatSaverKeyEndpoint = "/maps/id/";

        public BeatSaverMapInstaller(HttpProgressClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> InstallBeatSaverMapAsync(string installDir, string key, IStatusProgress? progress = null)
        {
            BeatSaverMap? map = await GetBeatSaverMapAsync(key).ConfigureAwait(false);
            if (map is null || map.Versions.Length <= 0) return false;
            progress?.Report(map.Name);
            using ZipArchive? archive = await DownloadBeatSaverMapAsync(map.Versions.Last(), progress).ConfigureAwait(false);
            return archive is not null && ExtractBeatSaverMapToDir(installDir, map, archive);
        }

        public async Task<BeatSaverMap?> GetBeatSaverMapAsync(string key, int retries = 2)
        {
            while (retries != 0)
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(kBeatSaverUrlPrefix + kBeatSaverKeyEndpoint + key).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    await WaitForRateLimitAsync().ConfigureAwait(false);
                    --retries;
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
                    --retries;
                    continue;
                }

                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                return new ZipArchive(stream);
            }

            return null;
        }

        public static bool ExtractBeatSaverMapToDir(string installDir, BeatSaverMap map, ZipArchive archive)
        {
            string customLevelsDirectoryPath = Path.Combine(installDir, "Beat Saber_Data", "CustomLevels");
            IOUtils.TryCreateDirectory(customLevelsDirectoryPath);
            string mapName = string.Concat($"{map.Id} ({map.MetaData.SongName} - {map.MetaData.LevelAuthorName})".Split(_illegalCharacters));
            string levelDirectoryPath = Path.Combine(customLevelsDirectoryPath, mapName);
            IOUtils.TryExtractArchive(archive, levelDirectoryPath, true);
            return true;
        }

        private static Task WaitForRateLimitAsync() => Task.Delay(1000);

        private static readonly char[] _illegalCharacters = {
            '<', '>', ':', '/', '\\', '|', '?', '*', '"',
            '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
            '\u0008', '\u0009', '\u000a', '\u000b', '\u000c', '\u000d', '\u000e', '\u000d',
            '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016',
            '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001f'
        };
    }
}