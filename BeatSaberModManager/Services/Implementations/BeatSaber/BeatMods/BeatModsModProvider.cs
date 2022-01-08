using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
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

        public IReadOnlyList<IMod>? AvailableMods { get; private set; }
        public HashSet<IMod>? InstalledMods { get; private set; }

        public bool IsModLoader(IMod? modification) => modification?.Name.ToUpperInvariant() == "BSIPA";

        public IEnumerable<IMod> GetDependencies(IMod modification)
        {
            if (modification is not BeatModsMod beatModsMod) yield break;
            foreach (BeatModsDependency dependency in beatModsMod.Dependencies)
            {
                dependency.DependingMod ??= AvailableMods?.FirstOrDefault(x => x.Name == dependency.Name);
                if (dependency.DependingMod is null) continue;
                yield return dependency.DependingMod;
            }
        }

        public async Task LoadInstalledModsAsync(string installDir)
        {
            Dictionary<string, BeatModsMod>? fileHashModPairs = await GetMappedModHashesAsync().ConfigureAwait(false);
            if (fileHashModPairs is null) return;
            InstalledMods = new HashSet<IMod>();
            // First loosely check if BSIPA is installed based on the injector hash
            // since it has many other files associated which could falsify the result
            string injectorPath = Path.Combine(installDir, "Beat Saber_Data/Managed/IPA.Injector.dll");
            string? injectorHash = await _hashProvider.CalculateHashForFile(injectorPath);
            if (injectorHash is not null && fileHashModPairs.TryGetValue(injectorHash, out BeatModsMod? bsipa))
                InstalledMods.Add(bsipa);
            // Then hash every file that could belong to a mod, find the mod that has a file with this hash
            // and check if all other files of the mod are installed as well
            IEnumerable<string> files = _installedModsLocations.Select(x => Path.Combine(installDir, x))
                .Where(Directory.Exists)
                .SelectMany(x => Directory.EnumerateFiles(x, string.Empty, SearchOption.AllDirectories))
                .Where(x => Path.GetExtension(x) is ".dll" or ".manifest" or ".exe");
            string?[] rawHashes = await Task.WhenAll(files.Select(_hashProvider.CalculateHashForFile)).ConfigureAwait(false);
            string[] hashes = rawHashes.Where(x => x is not null).ToArray()!;
            foreach (string hash in hashes)
            {
                if (fileHashModPairs.TryGetValue(hash, out BeatModsMod? mod) &&
                    !InstalledMods.Contains(mod) &&
                    !IsModLoader(mod) &&
                    !mod.Downloads[0].Hashes.Where(x => Path.GetExtension(x.File) is ".dll" or ".manifest" or ".exe").Select(x => x.Hash).Except(hashes).Any())
                {
                    InstalledMods.Add(mod);
                }
            }
        }

        public async Task LoadAvailableModsForCurrentVersionAsync(string installDir)
        {
            string? version = await _gameVersionProvider.DetectGameVersion(installDir).ConfigureAwait(false);
            if (version is null) return;
            await LoadAvailableModsForVersionAsync(version).ConfigureAwait(false);
        }

        public async Task LoadAvailableModsForVersionAsync(string version)
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

        private async Task<string?> GetAliasedGameVersion(string gameVersion)
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

        private async Task<Dictionary<string, BeatModsMod>?> GetMappedModHashesAsync()
        {
            ConfiguredTaskAwaitable<BeatModsMod[]?> approvedTask = GetModsAsync("mod?status=approved").ConfigureAwait(false);
            ConfiguredTaskAwaitable<BeatModsMod[]?> inactiveTask = GetModsAsync("mod?status=inactive").ConfigureAwait(false);
            BeatModsMod[]? approved = await approvedTask;
            BeatModsMod[]? inactive = await inactiveTask;
            if (approved is null || inactive is null) return null;
            BeatModsMod[] allMods = new BeatModsMod[approved.Length + inactive.Length];
            approved.CopyTo(allMods, 0);
            inactive.CopyTo(allMods, approved.Length);
            Dictionary<string, BeatModsMod> fileHashModPairs = new(allMods.Length);
            foreach (BeatModsMod mod in allMods.Where(x => x.Downloads.Length == 1))
            {
                foreach (BeatModsHash hash in mod.Downloads[0].Hashes)
                    fileHashModPairs.TryAdd(hash.Hash, mod);
            }

            return fileHashModPairs;
        }

        private static readonly string[] _installedModsLocations = { "IPA/Pending/Plugins", "IPA/Pending/Libs", "Plugins", "Libs" };
    }
}