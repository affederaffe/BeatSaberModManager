using System;
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
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly IHashProvider _hashProvider;

        private const string kBeatModsBaseUrl = "https://beatmods.com";
        private const string kBeatModsApiUrl = "https://beatmods.com/api/v1/";
        private const string kBeatModsAliasUrl = "https://alias.beatmods.com/aliases.json";
        private const string kBeatModsVersionsUrl = "https://versions.beatmods.com/versions.json";
        private const string kItem = "mod";
        private const string kApprovedStatus = "?status=approved";
        private const string kNotDeclinedStatus = "?status!=declined";
        private const string kGameVersion = "&gameVersion=";

        public BeatModsModProvider(Settings settings, HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions, IHashProvider hashProvider)
        {
            _settings = settings;
            _httpClient = httpClient;
            _jsonSerializerOptions = jsonSerializerOptions;
            _hashProvider = hashProvider;
        }

        public string ModLoaderName => "bsipa";
        public IMod[]? AvailableMods { get; private set; }
        public HashSet<IMod>? InstalledMods { get; private set; }
        public Dictionary<IMod, HashSet<IMod>> Dependencies { get; } = new();

        public void ResolveDependencies(IMod modToResolveFor)
        {
            if (modToResolveFor.Dependencies is null || modToResolveFor.Dependencies.Length <= 0) return;
            foreach (IDependency dependency in modToResolveFor.Dependencies)
            {
                dependency.DependingMod ??= AvailableMods?.FirstOrDefault(x => x.Name == dependency.Name);
                if (dependency.DependingMod is null) continue;
                if (Dependencies.TryGetValue(dependency.DependingMod!, out HashSet<IMod>? dependants)) dependants.Add(modToResolveFor);
                else Dependencies.Add(dependency.DependingMod!, new HashSet<IMod> { modToResolveFor });
            }
        }

        public void UnresolveDependencies(IMod modToUnresolveFor)
        {
            foreach (HashSet<IMod> dependants in Dependencies.Values)
                dependants.Remove(modToUnresolveFor);
        }

        public async Task LoadInstalledModsAsync()
        {
            IMod[]? allMods = await GetModsAsync(kItem + kNotDeclinedStatus);
            if (allMods is null) return;
            List<string> filesToCheck = new() { Path.Combine(_settings.InstallDir!, "Beat Saber_Data", "Managed", "IPA.Injector.dll") };
            foreach (string folder in new[] { "IPA/Pending/Plugins", "IPA/Pending/Libs", "Plugins", "Libs" })
            {
                string dirPath = Path.Combine(_settings.InstallDir!, folder);
                if (!Directory.Exists(dirPath)) continue;
                IEnumerable<string> files = Directory.EnumerateFiles(dirPath).Where(x =>
                    x.EndsWith(".dll", StringComparison.Ordinal) ||
                    x.EndsWith(".manifest", StringComparison.Ordinal));
                filesToCheck.AddRange(files);
            }

            InstalledMods = new HashSet<IMod>(filesToCheck.Count);
            foreach (string file in filesToCheck)
            {
                if (!File.Exists(file)) continue;
                string hash = _hashProvider.CalculateHashForFile(file);
                foreach (IMod mod in allMods)
                {
                    foreach (IDownload download in mod.Downloads!)
                    {
                        foreach (IHash downloadHash in download.Hashes!)
                        {
                            if (downloadHash.Hash == hash)
                                InstalledMods.Add(mod);
                        }
                    }
                }
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

        private async Task<IMod[]?> GetModsAsync(string? args)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(kBeatModsApiUrl + args);
            if (!response.IsSuccessStatusCode) return default;
            string body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IMod[]>(body, _jsonSerializerOptions);
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

            return versions[0];
        }
    }
}