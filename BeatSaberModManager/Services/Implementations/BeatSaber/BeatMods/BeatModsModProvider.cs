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


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    /// <inheritdoc />
    public class BeatModsModProvider : IModProvider
    {
        private readonly HttpProgressClient _httpClient;
        private readonly IHashProvider _hashProvider;
        private readonly IGameVersionProvider _gameVersionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeatModsModProvider"/> class.
        /// </summary>
        public BeatModsModProvider(HttpProgressClient httpClient, IHashProvider hashProvider, IGameVersionProvider gameVersionProvider)
        {
            _httpClient = httpClient;
            _hashProvider = hashProvider;
            _gameVersionProvider = gameVersionProvider;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IMod>? InstalledMods { get; private set; }

        /// <inheritdoc />
        public IReadOnlyCollection<IMod>? AvailableMods { get; private set; }

        /// <inheritdoc />
        public bool IsModLoader(IMod? modification) => modification?.Name.ToUpperInvariant() == "BSIPA";

        /// <inheritdoc />
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

        /// <summary>
        /// Asynchronously loads all currently installed mods by<br/>
        /// 1. Getting all mods and mapping their hashes to them,<br/>
        /// 2. Hashing relevant files in <see cref="_installedModsLocations"/>,<br/>
        /// 3. Adding all <see cref="IMod"/>s where all hashes of 2. are present.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <returns>All currently installed mods.</returns>
        public async Task LoadInstalledModsAsync(string installDir)
        {
            Dictionary<string, BeatModsMod>? fileHashModPairs = await GetMappedModHashesAsync().ConfigureAwait(false);
            if (fileHashModPairs is null) return;
            HashSet<IMod> installedMods = new();
            IMod? bsipa = await GetInstalledModLoaderAsync(installDir, fileHashModPairs);
            if (bsipa is not null) installedMods.Add(bsipa);
            IEnumerable<string> files = _installedModsLocations.Select(x => Path.Join(installDir, x))
                .Where(Directory.Exists)
                .SelectMany(static x => Directory.EnumerateFiles(x, string.Empty, SearchOption.AllDirectories))
                .Where(IsMod);
            string?[] rawHashes = await Task.WhenAll(files.Select(_hashProvider.CalculateHashForFileAsync)).ConfigureAwait(false);
            string[] hashes = rawHashes.Where(static x => x is not null).ToArray()!;
            foreach (string hash in hashes)
            {
                if (fileHashModPairs.TryGetValue(hash, out BeatModsMod? mod) &&
                    !installedMods.Contains(mod) &&
                    AvailableMods!.Contains(mod) &&
                    !IsModLoader(mod) &&
                    !mod.Downloads[0].Hashes.Where(IsMod).Select(static x => x.Hash).Except(hashes, StringComparer.OrdinalIgnoreCase).Any()) // Test if all mod-relevant files (e.g. .dll, .exe, .manifest) of the IMod exist on disk
                    installedMods.Add(mod);
            }

            InstalledMods = installedMods;
        }

        /// <inheritdoc />
        public async Task LoadAvailableModsForCurrentVersionAsync(string installDir)
        {
            string? version = await _gameVersionProvider.DetectGameVersionAsync(installDir).ConfigureAwait(false);
            if (version is null) return;
            await LoadAvailableModsForVersionAsync(version).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task LoadAvailableModsForVersionAsync(string version)
        {
            string? aliasedGameVersion = await GetAliasedGameVersionAsync(version).ConfigureAwait(false);
            if (aliasedGameVersion is null) return;
            AvailableMods = await GetModsAsync($"mod?status=approved&gameVersion={aliasedGameVersion}").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<ZipArchive> DownloadModsAsync(IEnumerable<IMod> mods, IProgress<double>? progress = null)
        {
            string[] urls = mods.OfType<BeatModsMod>().Select(static x => $"https://beatmods.com{x.Downloads[0].Url}").ToArray();
            await foreach (HttpResponseMessage response in _httpClient.TryGetAsync(urls, progress).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode) continue;
                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                yield return new ZipArchive(stream);
            }
        }

        private async Task<HashSet<BeatModsMod>?> GetModsAsync(string? args)
        {
            using HttpResponseMessage response = await _httpClient.TryGetAsync(new Uri($"https://beatmods.com/api/v1/{args}")).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize(body, BeatModsModJsonSerializerContext.Default.HashSetBeatModsMod);
        }

        /// <summary>
        /// Asynchronously resolves the game version which should be used to send requests to the BeatMods-API.
        /// </summary>
        /// <param name="gameVersion">The version of the game.</param>
        /// <returns>The aliased version for <paramref name="gameVersion"/>.</returns>
        private async Task<string?> GetAliasedGameVersionAsync(string gameVersion)
        {
            using HttpResponseMessage versionsResponse = await _httpClient.TryGetAsync(new Uri("https://versions.beatmods.com/versions.json")).ConfigureAwait(false);
            if (!versionsResponse.IsSuccessStatusCode) return null;
            string versionsBody = await versionsResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            string[]? versions = JsonSerializer.Deserialize(versionsBody, CommonJsonSerializerContext.Default.StringArray);
            if (versions is null) return null;
            if (versions.Contains(gameVersion)) return gameVersion;
            using HttpResponseMessage aliasResponse = await _httpClient.TryGetAsync(new Uri("https://alias.beatmods.com/aliases.json")).ConfigureAwait(false);
            if (!aliasResponse.IsSuccessStatusCode) return null;
            string aliasBody = await aliasResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            Dictionary<string, string[]>? aliases = JsonSerializer.Deserialize(aliasBody, CommonJsonSerializerContext.Default.DictionaryStringStringArray);
            return aliases?.FirstOrDefault(x => x.Value.Any(alias => alias == gameVersion)).Key;
        }

        /// <summary>
        /// Asynchronously gets all mods from https://beatmods.com and maps their files' hashes to themselves.
        /// </summary>
        /// <returns>A map of all hashes and their corresponding <see cref="BeatModsMod"/>.</returns>
        private async Task<Dictionary<string, BeatModsMod>?> GetMappedModHashesAsync()
        {
            ConfiguredTaskAwaitable<HashSet<BeatModsMod>?> approvedTask = GetModsAsync("mod?status=approved").ConfigureAwait(false);
            ConfiguredTaskAwaitable<HashSet<BeatModsMod>?> inactiveTask = GetModsAsync("mod?status=inactive").ConfigureAwait(false);
            HashSet<BeatModsMod>? approved = await approvedTask;
            HashSet<BeatModsMod>? inactive = await inactiveTask;
            if (approved is null || inactive is null) return null;
            approved.UnionWith(inactive);
            Dictionary<string, BeatModsMod> fileHashModPairs = new(approved.Count, StringComparer.OrdinalIgnoreCase);
            foreach (BeatModsMod mod in approved.Where(static x => x.Downloads.Length == 1))
            {
                foreach (BeatModsHash hash in mod.Downloads[0].Hashes)
                    fileHashModPairs.TryAdd(hash.Hash, mod);
            }

            return fileHashModPairs;
        }

        /// <summary>
        /// Attempts to detect if BSIPA is already installed by hashing the IPA.Injector.dll, then finding it in <paramref name="fileHashModPairs"/>
        /// and finally ensuring that every file was correctly installed.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="fileHashModPairs">A map of all hashes and their corresponding <see cref="BeatModsMod"/>.</param>
        /// <returns>The <see cref="IMod"/> of BSIPA if found, null otherwise.</returns>
        private async Task<IMod?> GetInstalledModLoaderAsync(string installDir, IReadOnlyDictionary<string, BeatModsMod> fileHashModPairs)
        {
            string injectorPath = Path.Join(installDir, "Beat Saber_Data", "Managed", "IPA.Injector.dll");
            string? injectorHash = await _hashProvider.CalculateHashForFileAsync(injectorPath).ConfigureAwait(false);
            if (injectorHash is null || !fileHashModPairs.TryGetValue(injectorHash, out BeatModsMod? bsipa)) return null;
            foreach (BeatModsHash beatModsHash in bsipa.Downloads[0].Hashes.Where(IsMod))
            {
                string fileName = beatModsHash.File.Replace("IPA/Data", "Beat Saber_Data", StringComparison.Ordinal).Replace("IPA/", null, StringComparison.Ordinal);
                string path = Path.Join(installDir, fileName);
                string? hash = await _hashProvider.CalculateHashForFileAsync(path);
                if (beatModsHash.Hash != hash) return null;
            }

            return bsipa;
        }

        /// <summary>
        /// Checks if the <paramref name="beatModsHash"/> is a mod and not e.g. a config file.
        /// </summary>
        /// <param name="beatModsHash">The <see cref="BeatModsHash"/> of the mod.</param>
        /// <returns>true if the <paramref name="beatModsHash"/> is a mod, false otherwise.</returns>
        private static bool IsMod(BeatModsHash beatModsHash) => IsMod(beatModsHash.File);

        /// <summary>
        /// Checks if the <paramref name="file"/> is a mod and not e.g. a config file.
        /// </summary>
        /// <param name="file">The path of the mod's file.</param>
        /// <returns>true if the file is a mod, false otherwise.</returns>
        private static bool IsMod(string file) => Path.GetExtension(file) is ".dll" or ".manifest" or ".exe";

        private static readonly string[] _installedModsLocations = { Path.Join("IPA", "Pending"), "Plugins", "Libs" };
    }
}
