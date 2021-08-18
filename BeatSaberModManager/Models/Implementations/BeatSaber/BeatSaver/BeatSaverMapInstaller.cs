using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverMapInstaller
    {
        private readonly Settings _settings;
        private readonly HttpClient _httpClient;

        private const string kBeatSaverUrlPrefix = "https://api.beatsaver.com";
        private const string kBeatSaverKeyEndpoint = "/maps/id/";

        public BeatSaverMapInstaller(Settings settings, HttpClient httpClient)
        {
            _settings = settings;
            _httpClient = httpClient;
        }

        public async Task<bool> InstallBeatSaverMapAsync(string key, IStatusProgress? progress = null)
        {
            if (!Directory.Exists(_settings.InstallDir)) return false;
            using HttpResponseMessage response = await _httpClient.GetAsync(kBeatSaverUrlPrefix + kBeatSaverKeyEndpoint + key).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return false;
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            BeatSaverMap? map = JsonSerializer.Deserialize<BeatSaverMap>(body);
            if (map is null || map.Versions!.Length <= 0) return false;
            progress?.Report(map.Name!);
            using ZipArchive? archive = await DownloadBeatSaverMapAsync(map.Versions.Last()).ConfigureAwait(false);
            if (archive is null) return false;
            string customLevelsDirectoryPath = Path.Combine(_settings.InstallDir, "Beat Saber_Data", "CustomLevels");
            if (!Directory.Exists(customLevelsDirectoryPath)) Directory.CreateDirectory(customLevelsDirectoryPath);
            string mapName = string.Concat($"{map.Id} ({map.MetaData?.SongName} - {map.MetaData?.LevelAuthorName})".Split(_illegalCharacters));
            string levelDirectoryPath = Path.Combine(customLevelsDirectoryPath, mapName);
            archive.ExtractToDirectory(levelDirectoryPath, true);
            return true;
        }

        private async Task<ZipArchive?> DownloadBeatSaverMapAsync(BeatSaverMapVersion mapVersion)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(mapVersion.DownloadUrl!).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;
            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return new ZipArchive(stream);
        }

        private static readonly char[] _illegalCharacters = new[]
        {
            '<', '>', ':', '/', '\\', '|', '?', '*', '"',
            '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
            '\u0008', '\u0009', '\u000a', '\u000b', '\u000c', '\u000d', '\u000e', '\u000d',
            '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016',
            '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001f'
        };
    }
}