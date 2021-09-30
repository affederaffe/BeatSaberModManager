using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    public class BeatModsModProvider : IModProvider
    {
        private readonly AppSettings _appSettings;
        private readonly HttpProgressClient _httpClient;
        private readonly IHashProvider _hashProvider;
        private readonly IGameVersionProvider _gameVersionProvider;

        private const string kBeatModsBaseUrl = "https://beatmods.com";
        private const string kBeatModsApiUrl = "https://beatmods.com/api/v1/";
        private const string kBeatModsAliasUrl = "https://alias.beatmods.com/aliases.json";
        private const string kBeatModsVersionsUrl = "https://versions.beatmods.com/versions.json";

        public BeatModsModProvider(ISettings<AppSettings> appSettings, HttpProgressClient httpClient, IHashProvider hashProvider, IGameVersionProvider gameVersionProvider)
        {
            _appSettings = appSettings.Value;
            _httpClient = httpClient;
            _hashProvider = hashProvider;
            _gameVersionProvider = gameVersionProvider;
        }

        public string ModLoaderName => "bsipa";
        public IMod[]? AvailableMods { get; private set; }
        public HashSet<IMod>? InstalledMods { get; private set; }
        public Dictionary<IMod, HashSet<IMod>> Dependencies { get; } = new();

        public IEnumerable<IMod> ResolveDependencies(IMod modToResolve)
        {
            if (modToResolve is not BeatModsMod beatModsMod) yield break;
            foreach (BeatModsDependency dependency in beatModsMod.Dependencies)
            {
                dependency.DependingMod ??= AvailableMods?.FirstOrDefault(x => x.Name == dependency.Name);
                if (dependency.DependingMod is null) continue;
                if (Dependencies.TryGetValue(dependency.DependingMod, out HashSet<IMod>? dependents)) dependents.Add(beatModsMod);
                else Dependencies.Add(dependency.DependingMod, new HashSet<IMod> { beatModsMod });
                yield return dependency.DependingMod;
            }
        }

        public IEnumerable<IMod> UnresolveDependencies(IMod modToUnresolve)
        {
            if (modToUnresolve is not BeatModsMod beatModsMod) yield break;
            foreach (BeatModsDependency dependency in beatModsMod.Dependencies)
            {
                if (dependency.DependingMod is null || !Dependencies.TryGetValue(dependency.DependingMod, out HashSet<IMod>? dependents)) continue;
                if (dependents.Remove(beatModsMod))
                    yield return dependency.DependingMod;
            }
        }

        public async Task LoadInstalledModsAsync()
        {
            if (!Directory.Exists(_appSettings.InstallDir)) return;
            BeatModsMod[]? allMods = await GetModsAsync("mod?status=approved").ConfigureAwait(false);
            if (allMods is null) return;
            Dictionary<string, IMod> fileHashModPairs = new();
            foreach (BeatModsMod mod in allMods)
            {
                BeatModsDownload download = mod.GetDownloadForVrPlatform(_appSettings.VrPlatform!);
                foreach (BeatModsHash hash in download.Hashes)
                    fileHashModPairs.TryAdd(hash.Hash, mod);
            }

            InstalledMods = new HashSet<IMod>();
            IEnumerable<string> files = _installedModsLocations.Select(x => Path.Combine(_appSettings.InstallDir, x))
                .Where(Directory.Exists)
                .SelectMany(Directory.EnumerateFiles)
                .Where(x => x.EndsWith(".dll", StringComparison.Ordinal) || x.EndsWith(".manifest", StringComparison.Ordinal));
            string ipaLoaderPath = Path.Combine(_appSettings.InstallDir, "Beat Saber_Data/Managed/IPA.Loader.dll");
            if (File.Exists(ipaLoaderPath)) files = files.Concat(new[] { ipaLoaderPath });
            foreach (string hash in files.Select(_hashProvider.CalculateHashForFile))
            {
                if (fileHashModPairs.TryGetValue(hash, out IMod? mod))
                    InstalledMods.Add(mod);
            }
        }

        public async Task LoadAvailableModsForCurrentVersionAsync()
        {
            string? version = _gameVersionProvider.GetGameVersion();
            await LoadAvailableModsForVersionAsync(version).ConfigureAwait(false);
        }

        public async Task LoadAvailableModsForVersionAsync(string? version)
        {
            string? aliasedGameVersion = await GetAliasedGameVersion(version).ConfigureAwait(false);
            if (aliasedGameVersion is null) return;
            AvailableMods = await GetModsAsync($"mod?status=approved&gameVersion={aliasedGameVersion}").ConfigureAwait(false);
        }

        public async IAsyncEnumerable<ZipArchive?> DownloadModsAsync(IEnumerable<string> urls, IProgress<double>? progress = null)
        {
            await foreach (HttpResponseMessage response in _httpClient.GetAsync(urls.Select(x => kBeatModsBaseUrl + x), progress).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode) yield return null;
                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                yield return new ZipArchive(stream);
            }
        }

        private async Task<BeatModsMod[]?> GetModsAsync(string? args)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(kBeatModsApiUrl + args).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return default;
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<BeatModsMod[]>(body);
        }

        private async Task<string?> GetAliasedGameVersion(string? gameVersion)
        {
            using HttpResponseMessage versionsResponse = await _httpClient.GetAsync(kBeatModsVersionsUrl).ConfigureAwait(false);
            if (!versionsResponse.IsSuccessStatusCode) return null;
            string versionsBody = await versionsResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            string[]? versions = JsonSerializer.Deserialize<string[]>(versionsBody);
            if (versions is null) return null;
            if (versions.Contains(gameVersion)) return gameVersion;
            using HttpResponseMessage aliasResponse = await _httpClient.GetAsync(kBeatModsAliasUrl).ConfigureAwait(false);
            if (!aliasResponse.IsSuccessStatusCode) return null;
            string aliasBody = await aliasResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            Dictionary<string, string[]>? aliases = JsonSerializer.Deserialize<Dictionary<string, string[]>>(aliasBody);
            if (aliases is null) return null;

            foreach ((string version, string[] alias) in aliases)
            {
                if (alias.Any(aliasedVersions => aliasedVersions == gameVersion))
                    return version;
            }

            return versions.FirstOrDefault();
        }

        private static readonly string[] _installedModsLocations = { "IPA/Pending/Plugins", "IPA/Pending/Libs", "Plugins", "Libs" };
    }
}