using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;

using Microsoft.Win32;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    /// <inheritdoc />
    public partial class BeatSaberInstallDirLocator : IInstallDirLocator
    {
        private readonly IInstallDirValidator _installDirValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeatSaberInstallDirLocator"/> class.
        /// </summary>
        public BeatSaberInstallDirLocator(IInstallDirValidator installDirValidator)
        {
            _installDirValidator = installDirValidator;
        }

        /// <inheritdoc />
        public ValueTask<string?> LocateInstallDirAsync() =>
            OperatingSystem.IsWindows() ? LocateWindowsInstallDirAsync()
                : OperatingSystem.IsLinux() ? LocateLinuxSteamInstallDirAsync()
                    : throw new PlatformNotSupportedException();

        /// <inheritdoc />
        public PlatformType DetectPlatform(string installDir)
        {
            string pluginsDir = Path.Join(installDir, "Beat Saber_Data", "Plugins");
            return File.Exists(Path.Join(pluginsDir, "steam_api64.dll")) || File.Exists(Path.Join(pluginsDir, "x86_64", "steam_api64.dll")) ? PlatformType.Steam : PlatformType.Oculus;
        }

        [SupportedOSPlatform("windows")]
        private ValueTask<string?> LocateWindowsInstallDirAsync()
        {
            string? steamInstallDir = LocateWindowsSteamInstallDir();
            return steamInstallDir is null ? ValueTask.FromResult(LocateOculusBeatSaberInstallDir()) : LocateSteamBeatSaberInstallDirAsync(steamInstallDir);
        }

        [SupportedOSPlatform("linux")]
        private ValueTask<string?> LocateLinuxSteamInstallDirAsync()
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string steamInstallDir = Path.Join(homeDir, ".steam", "root");
            return LocateSteamBeatSaberInstallDirAsync(steamInstallDir);
        }

        private async ValueTask<string?> LocateSteamBeatSaberInstallDirAsync(string steamInstallDir)
        {
            await foreach (string libPath in EnumerateSteamLibraryPathsAsync(steamInstallDir).ConfigureAwait(false))
            {
                string? installDir = await MatchSteamBeatSaberInstallDirAsync(libPath).ConfigureAwait(false);
                if (installDir is not null)
                    return Path.GetFullPath(installDir);
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
                if (_installDirValidator.ValidateInstallDir(installDir))
                    return installDir;
            }

            return null;
        }

        [SupportedOSPlatform("windows")]
        private string? LocateOculusBeatSaberInstallDir()
        {
            using RegistryKey? oculusInstallDirKey = Registry.LocalMachine.OpenSubKey("Software")?.OpenSubKey("Wow6432Node")?.OpenSubKey("Oculus VR, LLC")?.OpenSubKey("Oculus")?.OpenSubKey("Config");
            string? oculusInstallDir = oculusInstallDirKey?.GetValue("InitialAppLibrary")?.ToString();
            if (string.IsNullOrEmpty(oculusInstallDir))
                return null;
            string finalPath = Path.Join(oculusInstallDir, "Software", "hyperbolic-magnetism-beat-saber");
            string? installDir = _installDirValidator.ValidateInstallDir(finalPath) ? finalPath : LocateInOculusLibrary();
            return installDir is null ? null : Path.GetFullPath(installDir);
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
                if (_installDirValidator.ValidateInstallDir(finalPath))
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
