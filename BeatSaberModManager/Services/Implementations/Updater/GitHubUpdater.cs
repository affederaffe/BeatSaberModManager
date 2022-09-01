using System;
using System.Collections.Generic;
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
    /// <inheritdoc />
    public class GitHubUpdater : IUpdater
    {
        private readonly IReadOnlyList<string> _args;
        private readonly HttpProgressClient _httpClient;
        private readonly Version _version;

        private Release? _release;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubUpdater"/> class.
        /// </summary>
        public GitHubUpdater(IReadOnlyList<string> args, HttpProgressClient httpClient)
        {
            _args = args;
            _httpClient = httpClient;
            _version = new Version(ThisAssembly.Info.Version);
        }

        /// <inheritdoc />
        public async Task<bool> NeedsUpdateAsync()
        {
            if (OperatingSystem.IsLinux()) return false;
            using HttpResponseMessage response = await _httpClient.TryGetAsync(new Uri("https://api.github.com/repos/affederaffe/BeatSaberModManager/releases/latest")).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return false;
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _release = JsonSerializer.Deserialize(body, GitHubJsonSerializerContext.Default.Release);
            return _release is not null && Version.TryParse(_release.TagName.AsSpan(1, 5), out Version? version) && version > _version;
        }

        /// <inheritdoc />
        public async Task<int> UpdateAsync()
        {
            Asset? asset = _release?.Assets.FirstOrDefault(static x => x.Name.Contains("win-x64", StringComparison.Ordinal));
            if (asset is null) return -1;
            using HttpResponseMessage response = await _httpClient.TryGetAsync(new Uri(asset.DownloadUrl)).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return -1;
            string processPath = Environment.ProcessPath!;
            string oldPath = processPath.Replace(".exe", ".old.exe", StringComparison.Ordinal);
            IOUtils.TryDeleteFile(oldPath);
            IOUtils.TryMoveFile(processPath, oldPath);
            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using ZipArchive archive = new(stream);
            if (!IOUtils.TryExtractArchive(archive, Directory.GetCurrentDirectory(), true)) return -1;
            ProcessStartInfo processStartInfo = new(processPath);
            foreach (string arg in _args)
                processStartInfo.ArgumentList.Add(arg);
            return PlatformUtils.TryStartProcess(processStartInfo, out _) ? 0 : -1;
        }
    }
}
