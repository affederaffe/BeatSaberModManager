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
using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    public class BeatModsModProvider : IModProvider
    {
        private readonly HttpProgressClient _httpClient;
        private readonly IHashProvider _hashProvider;
        private readonly IGameVersionProvider _gameVersionProvider;

        private BeatModsMod[]? _availableMods;

        public BeatModsModProvider(HttpProgressClient httpClient, IHashProvider hashProvider, IGameVersionProvider gameVersionProvider)
        {
            _httpClient = httpClient;
            _hashProvider = hashProvider;
            _gameVersionProvider = gameVersionProvider;
        }

        public bool IsModLoader(IMod? modification) => modification?.Name.ToUpperInvariant() == "BSIPA";

        public IEnumerable<IMod> GetDependencies(IMod modification)
        {
            if (modification is not BeatModsMod beatModsMod) yield break;
            foreach (BeatModsDependency dependency in beatModsMod.Dependencies)
            {
                dependency.DependingMod ??= _availableMods?.FirstOrDefault(x => x.Name == dependency.Name);
                if (dependency.DependingMod is null) continue;
                yield return dependency.DependingMod;
            }
        }

        public async Task<IReadOnlyCollection<IMod>?> GetInstalledModsAsync(string installDir)
        {
            Dictionary<string, BeatModsMod>? fileHashModPairs = await GetMappedModHashesAsync().ConfigureAwait(false);
            if (fileHashModPairs is null) return null;
            HashSet<IMod> installedMods = new();
            IMod? bsipa = await GetInstalledModLoader(installDir, fileHashModPairs);
            if (bsipa is not null) installedMods.Add(bsipa);
            IEnumerable<string> files = _installedModsLocations.Select(x => Path.Combine(installDir, x))
                .Where(Directory.Exists)
                .SelectMany(static x => Directory.EnumerateFiles(x, string.Empty, SearchOption.AllDirectories))
                .Where(static x => Path.GetExtension(x) is Constants.DllExtension or Constants.ManifestExtension or Constants.ExeExtension);
            string?[] rawHashes = await Task.WhenAll(files.Select(_hashProvider.CalculateHashForFile)).ConfigureAwait(false);
            string[] hashes = rawHashes.Where(static x => x is not null).ToArray()!;
            foreach (string hash in hashes)
            {
                if (fileHashModPairs.TryGetValue(hash, out BeatModsMod? mod) &&
                    !installedMods.Contains(mod) &&
                    !IsModLoader(mod) &&
                    !mod.Downloads[0].Hashes.Where(static x => Path.GetExtension(x.File) is Constants.DllExtension or Constants.ManifestExtension or Constants.ExeExtension).Select(static x => x.Hash).Except(hashes).Any())
                    installedMods.Add(mod);
            }

            return installedMods;
        }

        public async Task<IReadOnlyCollection<IMod>?> GetAvailableModsForCurrentVersionAsync(string installDir)
        {
            string? version = await _gameVersionProvider.DetectGameVersionAsync(installDir).ConfigureAwait(false);
            return version is null ? null : await GetAvailableModsForVersionAsync(version).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<IMod>?> GetAvailableModsForVersionAsync(string version)
        {
            string? aliasedGameVersion = await GetAliasedGameVersion(version).ConfigureAwait(false);
            return aliasedGameVersion is null ? null : _availableMods = await GetModsAsync($"mod?status=approved&gameVersion={aliasedGameVersion}").ConfigureAwait(false);
        }

        public async IAsyncEnumerable<ZipArchive> DownloadModsAsync(IEnumerable<string> urls, IProgress<double>? progress = null)
        {
            await foreach (HttpResponseMessage response in _httpClient.GetAsync(urls.Select(static x => $"https://beatmods.com{x}").ToArray(), progress).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode) continue;
                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                yield return new ZipArchive(stream);
            }
        }

        private async Task<BeatModsMod[]?> GetModsAsync(string? args)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync($"https://beatmods.com/api/v1/{args}").ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return default;
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize(body, BeatModsModJsonSerializerContext.Default.BeatModsModArray);
        }

        private async Task<string?> GetAliasedGameVersion(string gameVersion)
        {
            using HttpResponseMessage versionsResponse = await _httpClient.GetAsync("https://versions.beatmods.com/versions.json").ConfigureAwait(false);
            if (!versionsResponse.IsSuccessStatusCode) return null;
            string versionsBody = await versionsResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            string[]? versions = JsonSerializer.Deserialize(versionsBody, CommonJsonSerializerContext.Default.StringArray);
            if (versions is null) return null;
            if (versions.Contains(gameVersion)) return gameVersion;
            using HttpResponseMessage aliasResponse = await _httpClient.GetAsync("https://alias.beatmods.com/aliases.json").ConfigureAwait(false);
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
            foreach (BeatModsMod mod in allMods.Where(static x => x.Downloads.Length == 1))
            {
                foreach (BeatModsHash hash in mod.Downloads[0].Hashes)
                    fileHashModPairs.TryAdd(hash.Hash, mod);
            }

            return fileHashModPairs;
        }

        private async Task<IMod?> GetInstalledModLoader(string installDir, IReadOnlyDictionary<string, BeatModsMod> fileHashModPairs)
        {
            string injectorPath = Path.Combine(installDir, Constants.BeatSaberDataDir, "Managed", "IPA.Injector.dll");
            string? injectorHash = await _hashProvider.CalculateHashForFile(injectorPath).ConfigureAwait(false);
            if (injectorHash is null || !fileHashModPairs.TryGetValue(injectorHash, out BeatModsMod? bsipa)) return null;
            foreach (BeatModsHash beatModsHash in bsipa.Downloads[0].Hashes.Where(static x => Path.GetExtension(x.File) is Constants.DllExtension or Constants.ManifestExtension or Constants.ExeExtension))
            {
                string fileName = beatModsHash.File.Replace("IPA/Data", Constants.BeatSaberDataDir, StringComparison.Ordinal).Replace("IPA/", null);
                string path = Path.Combine(installDir, fileName);
                string? hash = await _hashProvider.CalculateHashForFile(path);
                if (beatModsHash.Hash != hash) return null;
            }

            return bsipa;
        }

        private static readonly string[] _installedModsLocations = { Path.Combine(Constants.IpaDir, Constants.PendingDir), Constants.PluginsDir, Constants.LibsDir };
    }
}