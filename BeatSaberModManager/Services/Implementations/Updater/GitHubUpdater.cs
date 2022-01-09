using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.GitHub;
using BeatSaberModManager.Models.Implementations.JsonSerializerContexts;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utilities;


namespace BeatSaberModManager.Services.Implementations.Updater
{
    public class GitHubUpdater : IUpdater
    {
        private readonly HttpProgressClient _httpClient;
        private readonly Version _version;

        private Release? _release;

        private const string kLatestApiEndpoint = "https://api.github.com/repos/affederaffe/BeatSaberModManager/releases/latest";

        public GitHubUpdater(HttpProgressClient httpClient, Version version)
        {
            _httpClient = httpClient;
            _version = version;
        }

        public async Task<bool> NeedsUpdate()
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(kLatestApiEndpoint).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return false;
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _release = JsonSerializer.Deserialize(body, GitHubJsonSerializerContext.Default.Release);
            if (_release is null) return false;
            Version latestVersion = Version.Parse(_release.TagName.AsSpan(1, 5));
            return latestVersion > _version;
        }

        public async Task<int> Update()
        {
            string identifier = OperatingSystem.IsWindows() ? "win-x64" : OperatingSystem.IsLinux() ? "linux-x64" : throw new PlatformNotSupportedException();
            Asset? asset = _release?.Assets.FirstOrDefault(x => x.Name.Contains(identifier, StringComparison.OrdinalIgnoreCase));
            if (asset is null) return -1;
            HttpResponseMessage response = await _httpClient.GetAsync(asset.DownloadUrl).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return -1;
            string processPath = Environment.ProcessPath!;
            string oldPath = processPath + ".old";
            IOUtils.TryDeleteFile(oldPath);
            IOUtils.TryMoveFile(processPath, oldPath);
            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            ZipArchive archive = new(stream);
            if (!IOUtils.TryExtractArchive(archive, Directory.GetCurrentDirectory(), true)) return -1;
            Process.Start(processPath, Environment.GetCommandLineArgs());
            return 0;
        }
    }
}