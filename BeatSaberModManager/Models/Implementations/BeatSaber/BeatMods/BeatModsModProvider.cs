using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsModProvider : IModProvider
    {
        private readonly Settings _settings;
        private readonly HttpClient _httpClient;
        private readonly IHashProvider _hashProvider;

        private const string kBeatModsBaseUrl = "https://beatmods.com";
        private const string kBeatModsApiUrl = "https://beatmods.com/api/v1/";
        private const string kBeatModsAliasUrl = "https://alias.beatmods.com/aliases.json";
        private const string kBeatModsVersionsUrl = "https://versions.beatmods.com/versions.json";
        private const string kItem = "mod";
        private const string kApprovedStatus = "?status=approved";
        private const string kNotDeclinedStatus = "?status!=declined";
        private const string kGameVersion = "&gameVersion=";

        public BeatModsModProvider(Settings settings, HttpClient httpClient, IHashProvider hashProvider)
        {
            _settings = settings;
            _httpClient = httpClient;
            _hashProvider = hashProvider;
        }

        public string ModLoaderName => "bsipa";
        public IMod[]? AvailableMods { get; private set; }
        public HashSet<IMod>? InstalledMods { get; private set; }
        public Dictionary<IMod, HashSet<IMod>> Dependencies { get; } = new();

        public void ResolveDependencies(IMod modToResolve)
        {
            if (modToResolve is not BeatModsMod beatModsMod || beatModsMod.Dependencies?.Length <= 0) return;
            foreach (BeatModsDependency dependency in beatModsMod.Dependencies!)
            {
                dependency.DependingMod ??= AvailableMods?.FirstOrDefault(x => x.Name == dependency.Name);
                if (dependency.DependingMod is null) continue;
                if (Dependencies.TryGetValue(dependency.DependingMod!, out HashSet<IMod>? dependants)) dependants.Add(beatModsMod);
                else Dependencies.Add(dependency.DependingMod!, new HashSet<IMod> { beatModsMod });
            }
        }

        public void UnresolveDependencies(IMod modToUnresolve)
        {
            foreach (HashSet<IMod> dependants in Dependencies.Values)
                dependants.Remove(modToUnresolve);
        }

        public async Task LoadInstalledModsAsync()
        {
            BeatModsMod[]? allMods = await GetModsAsync(kItem + kNotDeclinedStatus);
            if (allMods is null) return;
            Dictionary<string, IMod> fileHashModPairs = new();
            foreach (BeatModsMod mod in allMods)
            {
                BeatModsDownload? download = mod.GetDownloadForVRPlatform(_settings.VRPlatform!);
                if (download?.Hashes is null) continue;
                foreach (BeatModsHash hash in download.Hashes)
                    fileHashModPairs.TryAdd(hash.Hash!, mod);
            }

            InstalledMods = new HashSet<IMod>();
            foreach (string filePath in _installedModsLocations.Select(x => Path.Combine(_settings.InstallDir!, x)).Where(Directory.Exists).SelectMany(d => Directory.EnumerateFiles(d, "*.dll")))
            {
                string hash = _hashProvider.CalculateHashForFile(filePath);
                if (!fileHashModPairs.TryGetValue(hash, out IMod? mod)) continue;
                InstalledMods.Add(mod);
            }
        }

        public async Task LoadAvailableModsForVersionAsync(string version)
        {
            string? aliasedGameVersion = await GetAliasedGameVersion(version);
            if (aliasedGameVersion is null) return;
            AvailableMods = await GetModsAsync(kItem + kApprovedStatus + kGameVersion + aliasedGameVersion);
        }

        public async Task<ZipArchive?> DownloadModAsync(string url)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(kBeatModsBaseUrl + url);
            if (!response.IsSuccessStatusCode) return null;
            Stream stream = await response.Content.ReadAsStreamAsync();
            return new ZipArchive(stream);
        }

        private async Task<BeatModsMod[]?> GetModsAsync(string? args)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(kBeatModsApiUrl + args);
            if (!response.IsSuccessStatusCode) return default;
            string body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BeatModsMod[]>(body);
        }

        private async Task<string?> GetAliasedGameVersion(string gameVersion)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(kBeatModsVersionsUrl);
            if (!response.IsSuccessStatusCode) return null;
            string body = await response.Content.ReadAsStringAsync();
            string[]? versions = JsonSerializer.Deserialize<string[]>(body);
            if (versions is null) return null;

            if (versions.Contains(gameVersion))
                return gameVersion;

            response = await _httpClient.GetAsync(kBeatModsAliasUrl);
            if (!response.IsSuccessStatusCode) return null;
            body = await response.Content.ReadAsStringAsync();
            Dictionary<string, string[]>? aliases = JsonSerializer.Deserialize<Dictionary<string, string[]>>(body);
            if (aliases is null) return null;

            foreach ((string version, string[] alias) in aliases)
            {
                if (alias.Any(aliasedVersions => aliasedVersions == gameVersion))
                    return version;
            }

            return versions.FirstOrDefault();
        }

        private static readonly string[] _installedModsLocations = { string.Empty, "IPA/Pending/Plugins", "IPA/Pending/Libs", "Plugins", "Libs" };
    }
}