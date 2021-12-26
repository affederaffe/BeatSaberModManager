using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Models.Implementations.JsonSerializerContexts;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    public class BeatModsModProvider : IModProvider
    {
        private readonly HttpProgressClient _httpClient;
        private readonly IHashProvider _hashProvider;
        private readonly IGameVersionProvider _gameVersionProvider;

        private const string kBeatModsBaseUrl = "https://beatmods.com";
        private const string kBeatModsApiUrl = "https://beatmods.com/api/v1/";
        private const string kBeatModsAliasUrl = "https://alias.beatmods.com/aliases.json";
        private const string kBeatModsVersionsUrl = "https://versions.beatmods.com/versions.json";

        public BeatModsModProvider(HttpProgressClient httpClient, IHashProvider hashProvider, IGameVersionProvider gameVersionProvider)
        {
            _httpClient = httpClient;
            _hashProvider = hashProvider;
            _gameVersionProvider = gameVersionProvider;
        }

        public IMod[]? AvailableMods { get; private set; }
        public HashSet<IMod>? InstalledMods { get; private set; }

        public bool IsModLoader(IMod? mod) => mod?.Name.ToLowerInvariant() == "bsipa";

        public IEnumerable<IMod> GetDependencies(IMod mod)
        {
            if (mod is not BeatModsMod beatModsMod) yield break;
            foreach (BeatModsDependency dependency in beatModsMod.Dependencies)
            {
                dependency.DependingMod ??= AvailableMods?.FirstOrDefault(x => x.Name == dependency.Name);
                if (dependency.DependingMod is null) continue;
                yield return dependency.DependingMod;
            }
        }

        public async Task LoadInstalledModsAsync(string installDir)
        {
            Dictionary<string, IMod>? fileHashModPairs = await GetMappedModHashesAsync().ConfigureAwait(false);
            if (fileHashModPairs is null) return;
            InstalledMods = new HashSet<IMod>();
            IEnumerable<string> files = _installedModsLocations.Select(x => Path.Combine(installDir, x))
                .Where(Directory.Exists)
                .SelectMany(Directory.EnumerateFiles)
                .Where(x => x.EndsWith(".dll", StringComparison.Ordinal) || x.EndsWith(".manifest", StringComparison.Ordinal))
                .Concat(new[] { Path.Combine(installDir, "Beat Saber_Data/Managed/IPA.Loader.dll") });
            string?[] hashes = await Task.WhenAll(files.Select(_hashProvider.CalculateHashForFile));
            foreach (string? hash in hashes)
            {
                if (hash is not null && fileHashModPairs.TryGetValue(hash, out IMod? mod))
                    InstalledMods.Add(mod);
            }
        }

        public async Task LoadAvailableModsForCurrentVersionAsync(string installDir)
        {
            string? version = await _gameVersionProvider.DetectGameVersion(installDir);
            await LoadAvailableModsForVersionAsync(version).ConfigureAwait(false);
        }

        public async Task LoadAvailableModsForVersionAsync(string? version)
        {
            string? aliasedGameVersion = await GetAliasedGameVersion(version).ConfigureAwait(false);
            if (aliasedGameVersion is null) return;
            AvailableMods = await GetModsAsync($"mod?status=approved&gameVersion={aliasedGameVersion}").ConfigureAwait(false);
        }

        public async IAsyncEnumerable<ZipArchive> DownloadModsAsync(IEnumerable<string> urls, IProgress<double>? progress = null)
        {
            await foreach (HttpResponseMessage response in _httpClient.GetAsync(urls.Select(x => kBeatModsBaseUrl + x).ToArray(), progress).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode) continue;
                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                yield return new ZipArchive(stream);
            }
        }

        private async Task<BeatModsMod[]?> GetModsAsync(string? args)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(kBeatModsApiUrl + args).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return default;
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize(body, BeatModsModJsonSerializerContext.Default.BeatModsModArray);
        }

        private async Task<string?> GetAliasedGameVersion(string? gameVersion)
        {
            using HttpResponseMessage versionsResponse = await _httpClient.GetAsync(kBeatModsVersionsUrl).ConfigureAwait(false);
            if (!versionsResponse.IsSuccessStatusCode) return null;
            string versionsBody = await versionsResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            string[]? versions = JsonSerializer.Deserialize(versionsBody, CommonJsonSerializerContext.Default.StringArray);
            if (versions is null) return null;
            if (versions.Contains(gameVersion)) return gameVersion;
            using HttpResponseMessage aliasResponse = await _httpClient.GetAsync(kBeatModsAliasUrl).ConfigureAwait(false);
            if (!aliasResponse.IsSuccessStatusCode) return null;
            string aliasBody = await aliasResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            Dictionary<string, string[]>? aliases = JsonSerializer.Deserialize(aliasBody, CommonJsonSerializerContext.Default.DictionaryStringStringArray);
            if (aliases is null) return null;
            foreach ((string version, string[] alias) in aliases)
            {
                if (alias.Any(aliasedVersions => aliasedVersions == gameVersion))
                    return version;
            }

            return null;
        }

        private async Task<Dictionary<string, IMod>?> GetMappedModHashesAsync()
        {
            BeatModsMod[]? allMods = await GetModsAsync("mod?status=approved").ConfigureAwait(false);
            if (allMods is null) return null;
            Dictionary<string, IMod> fileHashModPairs = new(allMods.Length);
            foreach (BeatModsMod mod in allMods)
            {
                BeatModsDownload download = mod.Downloads.First();
                foreach (BeatModsHash hash in download.Hashes)
                    fileHashModPairs.TryAdd(hash.Hash, mod);
            }

            return fileHashModPairs;
        }

        private static readonly string[] _installedModsLocations = { "IPA/Pending/Plugins", "IPA/Pending/Libs", "Plugins", "Libs" };
    }
}