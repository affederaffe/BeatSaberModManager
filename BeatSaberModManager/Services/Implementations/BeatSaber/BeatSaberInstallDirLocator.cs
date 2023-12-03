using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Versions;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;

using Microsoft.Win32;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    /// <inheritdoc />
    public partial class BeatSaberInstallDirLocator(IInstallDirValidator installDirValidator, IGameVersionProvider gameVersionProvider) : IInstallDirLocator
    {
        /// <inheritdoc />
        public ValueTask<IGameVersion?> LocateInstallDirAsync() =>
            OperatingSystem.IsWindows() ? LocateWindowsInstallDirAsync()
                : OperatingSystem.IsLinux() ? LocateLinuxSteamInstallDirAsync()
                    : throw new PlatformNotSupportedException();

        [SupportedOSPlatform("windows")]
        private ValueTask<IGameVersion?> LocateWindowsInstallDirAsync()
        {
            string? steamInstallDir = LocateWindowsSteamInstallDir();
            return steamInstallDir is null ? LocateOculusBeatSaberInstallDirAsync() : LocateSteamBeatSaberInstallDirAsync(steamInstallDir);
        }

        [SupportedOSPlatform("linux")]
        private ValueTask<IGameVersion?> LocateLinuxSteamInstallDirAsync()
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string steamInstallDir = Path.Join(homeDir, ".steam", "root");
            return LocateSteamBeatSaberInstallDirAsync(steamInstallDir);
        }

        private async ValueTask<IGameVersion?> LocateSteamBeatSaberInstallDirAsync(string steamInstallDir)
        {
            await foreach (string libPath in EnumerateSteamLibraryPathsAsync(steamInstallDir).ConfigureAwait(false))
            {
                string? installDir = await MatchSteamBeatSaberInstallDirAsync(libPath).ConfigureAwait(false);
                if (installDir is null)
                    return null;
                string? gameVersion = await gameVersionProvider.DetectGameVersionAsync(installDir).ConfigureAwait(false);
                return gameVersion is null ? null : new SteamGameVersion
                {
                    GameVersion = gameVersion,
                    InstallDir = installDir
                };
            }

            return null;
        }

        private async Task<string?> MatchSteamBeatSaberInstallDirAsync(string path)
        {
            string acf = Path.Join(path, "appmanifest_620980.acf");
#pragma warning disable CA2007
            await using FileStream? fileStream = IOUtils.TryOpenFile(acf, FileMode.Open, FileAccess.Read);
#pragma warning restore CA2007
            if (fileStream is null)
                return null;
            Regex regex = InstallDirRegex();
            using StreamReader reader = new(fileStream);
            while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
            {
                Match match = regex.Match(line);
                if (!match.Success)
                    continue;
                string installDir = Path.Join(path, "common", match.Groups[1].Value);
                if (installDirValidator.ValidateInstallDir(installDir))
                    return installDir;
            }

            return null;
        }

        [SupportedOSPlatform("windows")]
        private async ValueTask<IGameVersion?> LocateOculusBeatSaberInstallDirAsync()
        {
            using RegistryKey? oculusInstallDirKey = Registry.LocalMachine.OpenSubKey("Software")?.OpenSubKey("Wow6432Node")?.OpenSubKey("Oculus VR, LLC")?.OpenSubKey("Oculus")?.OpenSubKey("Config");
            string? oculusInstallDir = oculusInstallDirKey?.GetValue("InitialAppLibrary")?.ToString();
            if (string.IsNullOrEmpty(oculusInstallDir))
                return null;
            string finalPath = Path.Join(oculusInstallDir, "Software", "hyperbolic-magnetism-beat-saber");
            string? installDir = installDirValidator.ValidateInstallDir(finalPath) ? finalPath : LocateInOculusLibrary();
            if (installDir is null)
                return null;
            string? gameVersion = await gameVersionProvider.DetectGameVersionAsync(installDir).ConfigureAwait(false);
            return gameVersion is null ? null : new OculusGameVersion
            {
                GameVersion = gameVersion,
                InstallDir = Path.GetFullPath(installDir)
            };
        }

        [SupportedOSPlatform("windows")]
        private string? LocateInOculusLibrary()
        {
            using RegistryKey? librariesKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Oculus VR, LLC")?.OpenSubKey("Oculus")?.OpenSubKey("Libraries");
            if (librariesKey is null)
                return null;
            foreach (string libraryKeyName in librariesKey.GetSubKeyNames())
            {
                using RegistryKey? libraryKey = librariesKey.OpenSubKey(libraryKeyName);
                string? libraryPath = libraryKey?.GetValue("Path")?.ToString();
                if (libraryPath is null)
                    continue;
                string finalPath = Path.Join(libraryPath, "Software", "hyperbolic-magnetism-beat-saber");
                if (installDirValidator.ValidateInstallDir(finalPath))
                    return finalPath;
            }

            return null;
        }

        [SupportedOSPlatform("windows")]
        private static string? LocateWindowsSteamInstallDir()
        {
            using RegistryKey? steamInstallDirKey = Registry.LocalMachine.OpenSubKey("Software")?.OpenSubKey("Wow6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam");
            return steamInstallDirKey?.GetValue("InstallPath")?.ToString();
        }

        private static async IAsyncEnumerable<string> EnumerateSteamLibraryPathsAsync(string path)
        {
            yield return path;
            string vdf = Path.Join(path, "steamapps", "libraryfolders.vdf");
#pragma warning disable CA2007
            await using FileStream? fileStream = IOUtils.TryOpenFile(vdf, FileMode.Open, FileAccess.Read);
#pragma warning restore CA2007
            if (fileStream is null)
                yield break;
            Regex regex = LibraryPathRegex();
            using StreamReader vdfReader = new(fileStream);
            while (await vdfReader.ReadLineAsync().ConfigureAwait(false) is { } line)
            {
                Match match = regex.Match(line);
                if (match.Success)
                    yield return Path.Join(match.Groups[1].Value.Replace(@"\\", "/", StringComparison.Ordinal), "steamapps");
            }
        }

        [GeneratedRegex("\\s\"installdir\"\\s+\"(.+)\"")]
        private static partial Regex InstallDirRegex();

        [GeneratedRegex("\\s\"(?:\\d|path)\"\\s+\"(.+)\"")]
        private static partial Regex LibraryPathRegex();
    }
}
