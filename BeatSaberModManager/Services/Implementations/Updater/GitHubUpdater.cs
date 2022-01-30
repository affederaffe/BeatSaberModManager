using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.GitHub;
using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.Updater
{
    public class GitHubUpdater : IUpdater
    {
        private readonly HttpProgressClient _httpClient;
        private readonly Version _version;

        private Release? _release;

        public GitHubUpdater(HttpProgressClient httpClient)
        {
            _httpClient = httpClient;
            _version = new Version(ThisAssembly.Info.Version);
        }

        public async Task<bool> NeedsUpdate()
        {
            if (OperatingSystem.IsLinux()) return false;
            using HttpResponseMessage response = await _httpClient.GetAsync("https://api.github.com/repos/affederaffe/BeatSaberModManager/releases/latest").ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return false;
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _release = JsonSerializer.Deserialize(body, GitHubJsonSerializerContext.Default.Release);
            return _release is not null && Version.TryParse(_release.TagName.AsSpan(1, 5), out Version? version) && version > _version;
        }

        public async Task<int> Update()
        {
            Asset? asset = _release?.Assets.FirstOrDefault(static x => x.Name.Contains("win-x64", StringComparison.OrdinalIgnoreCase));
            if (asset is null) return -1;
            HttpResponseMessage response = await _httpClient.GetAsync(asset.DownloadUrl).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return -1;
            string processPath = Environment.ProcessPath!;
            string oldPath = processPath.Replace(Constants.ExeExtension, Constants.OldExeExtension);
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