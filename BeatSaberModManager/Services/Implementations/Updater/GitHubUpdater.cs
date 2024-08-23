using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.GitHub;
using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.Updater
{
    /// <inheritdoc />
    public class GitHubUpdater(IEnumerable<string> args, HttpProgressClient httpClient) : IUpdater
    {
        private readonly Version _version = new(ThisAssembly.Info.Version);

        private Release? _release;

        /// <inheritdoc />
        public async ValueTask<bool> NeedsUpdateAsync()
        {
            if (!Program.IsProduction || OperatingSystem.IsLinux())
                return false;
            using HttpResponseMessage response = await httpClient.TryGetAsync(new Uri("https://api.github.com/repos/affederaffe/BeatSaberModManager/releases/latest")).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return false;
            _release = await response.Content.ReadFromJsonAsync(GitHubJsonSerializerContext.Default.Release).ConfigureAwait(false);
            return _release is not null && Version.TryParse(_release.TagName.AsSpan(1, 5), out Version? version) && version > _version;
        }

        /// <inheritdoc />
        public async Task<int> UpdateAsync()
        {
            Asset? asset = _release?.Assets.FirstOrDefault(static x => x.Name.Contains("win-x64", StringComparison.Ordinal));
            if (asset is null)
                return -1;
            using HttpResponseMessage response = await httpClient.TryGetAsync(asset.DownloadUrl).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return -1;
            string processPath = Environment.ProcessPath!;
            string oldPath = processPath.Replace(".exe", ".old.exe", StringComparison.Ordinal);
            IOUtils.TryDeleteFile(oldPath);
            if (!IOUtils.TryMoveFile(processPath, oldPath))
                return -1;
#pragma warning disable CA2007
            await using Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#pragma warning restore CA2007
            using ZipArchive archive = new(stream);
            if (!IOUtils.TryExtractArchive(archive, Directory.GetCurrentDirectory(), true))
                return -1;
            ProcessStartInfo processStartInfo = new(processPath);
            foreach (string arg in args)
                processStartInfo.ArgumentList.Add(arg);
            return PlatformUtils.TryStartProcess(processStartInfo, out _) ? 0 : -1;
        }
    }
}
