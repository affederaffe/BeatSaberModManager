using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverMapInstaller
    {
        private readonly AppSettings _appSettings;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly HttpProgressClient _httpClient;

        private const string kBeatSaverUrlPrefix = "https://api.beatsaver.com";
        private const string kBeatSaverKeyEndpoint = "/maps/id/";

        public BeatSaverMapInstaller(ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, HttpProgressClient httpClient)
        {
            _appSettings = appSettings.Value;
            _installDirValidator = installDirValidator;
            _httpClient = httpClient;
        }

        public async Task<bool> InstallBeatSaverMapAsync(string key, IStatusProgress? progress = null)
        {
            BeatSaverMap? map = await GetBeatSaverMapAsync(key);
            if (map is null || map.Versions.Length <= 0) return false;
            progress?.Report(map.Name);
            using ZipArchive? archive = await DownloadBeatSaverMapAsync(map.Versions.Last(), progress).ConfigureAwait(false);
            return archive is not null && ExtractBeatSaverMapToFolder(map, archive);
        }

        public async Task<BeatSaverMap?> GetBeatSaverMapAsync(string key, int retries = 2)
        {
            if (retries == 0) return null;
            using HttpResponseMessage response = await _httpClient.GetAsync(kBeatSaverUrlPrefix + kBeatSaverKeyEndpoint + key).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                await WaitForRateLimitAsync();
                return await GetBeatSaverMapAsync(key, retries - 1).ConfigureAwait(false);
            }

            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<BeatSaverMap>(body);
        }

        public bool ExtractBeatSaverMapToFolder(BeatSaverMap map, ZipArchive archive)
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.InstallDir.Value)) return false;
            string customLevelsDirectoryPath = Path.Combine(_appSettings.InstallDir.Value!, "Beat Saber_Data", "CustomLevels");
            if (!Directory.Exists(customLevelsDirectoryPath)) Directory.CreateDirectory(customLevelsDirectoryPath);
            string mapName = string.Concat($"{map.Id} ({map.MetaData.SongName} - {map.MetaData.LevelAuthorName})".Split(_illegalCharacters));
            string levelDirectoryPath = Path.Combine(customLevelsDirectoryPath, mapName);
            archive.ExtractToDirectory(levelDirectoryPath, true);
            return true;
        }

        private async Task<ZipArchive?> DownloadBeatSaverMapAsync(BeatSaverMapVersion mapVersion, IProgress<double>? progress = null, int retries = 2)
        {
            if (retries == 0) return null;
            HttpResponseMessage response = await _httpClient.GetAsync(mapVersion.DownloadUrl, progress).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                await WaitForRateLimitAsync();
                return await DownloadBeatSaverMapAsync(mapVersion, progress, retries - 1);
            }

            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return new ZipArchive(stream);
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