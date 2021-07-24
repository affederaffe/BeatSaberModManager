using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverAssetProvider : IAssetProvider
    {
        private readonly Settings _settings;
        private readonly HttpClient _httpClient;

        private const string kBeatSaverUrlPrefix = "https://beatsaver.com";
        private const string kBeatSaverKeyEndpoint = "/api/maps/detail/";
        private const string kBeatSaberDataFolderName = "Beat Saber_Data";
        private const string kCustomSongsFolderName = "CustomLevels";

        private static readonly char[] _illegalCharacters = new[]
        {
            '<', '>', ':', '/', '\\', '|', '?', '*', '"',
            '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
            '\u0008', '\u0009', '\u000a', '\u000b', '\u000c', '\u000d', '\u000e', '\u000d',
            '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016',
            '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001f'
        };

        public BeatSaverAssetProvider(Settings settings, HttpClient httpClient)
        {
            _settings = settings;
            _httpClient = httpClient;
        }

        public string Protocol => "beatsaver";

        public async Task<bool> InstallAssetAsync(Uri uri)
        {
            if (_settings.InstallDir is null) return false;
            HttpResponseMessage response = await _httpClient.GetAsync(kBeatSaverUrlPrefix + kBeatSaverKeyEndpoint + uri.Host);
            if (!response.IsSuccessStatusCode) return false;
            string body = await response.Content.ReadAsStringAsync();
            BeatSaverMap? map = JsonSerializer.Deserialize<BeatSaverMap>(body);
            if (map is null) return false;
            using ZipArchive? archive = await DownloadBeatSaverMapAsync(map);
            if (archive is null) return false;
            string customLevelsDirectoryPath = Path.Combine(_settings.InstallDir, kBeatSaberDataFolderName, kCustomSongsFolderName);
            if (!Directory.Exists(customLevelsDirectoryPath)) Directory.CreateDirectory(customLevelsDirectoryPath);
            string mapName = string.Concat($"{map.Key} ({map.MetaData?.SongName} - {map.MetaData?.LevelAuthorName})"
                                   .Split(_illegalCharacters));
            string levelDirectoryPath = Path.Combine(customLevelsDirectoryPath, mapName);
            archive.ExtractToDirectory(levelDirectoryPath, true);
            return true;
        }

        private async Task<ZipArchive?> DownloadBeatSaverMapAsync(BeatSaverMap map)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(kBeatSaverUrlPrefix + map.DownloadUrl);
            if (!response.IsSuccessStatusCode) return null;
            Stream stream = await response.Content.ReadAsStreamAsync();
            return new ZipArchive(stream);
        }
    }
}