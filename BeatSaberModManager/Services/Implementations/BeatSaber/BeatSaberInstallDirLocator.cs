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
    public class BeatSaberInstallDirLocator : IInstallDirLocator
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
            string pluginsDir = Path.Combine(installDir, "Beat Saber_Data", "Plugins");
            return File.Exists(Path.Combine(pluginsDir, "steam_api64.dll")) || File.Exists(Path.Combine(pluginsDir, "x86_64", "steam_api64.dll")) ? PlatformType.Steam : PlatformType.Oculus;
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
            string steamInstallDir = Path.Combine(homeDir, ".steam", "root");
            return LocateSteamBeatSaberInstallDirAsync(steamInstallDir);
        }

        private async ValueTask<string?> LocateSteamBeatSaberInstallDirAsync(string steamInstallDir)
        {
            await foreach (string libPath in EnumerateSteamLibraryPathsAsync(steamInstallDir).ConfigureAwait(false))
            {
                string? installDir = await MatchSteamBeatSaberInstallDirAsync(libPath).ConfigureAwait(false);
                if (installDir is not null) return Path.GetFullPath(installDir);
            }

            return null;
        }

        private async Task<string?> MatchSteamBeatSaberInstallDirAsync(string path)
        {
            string acf = Path.Combine(path, "appmanifest_620980.acf");
            await using FileStream? fileStream = IOUtils.TryOpenFile(acf, new FileStreamOptions { Options = FileOptions.Asynchronous });
            if (fileStream is null) return null;
            Regex regex = new("\\s\"installdir\"\\s+\"(.+)\"");
            string? line;
            using StreamReader reader = new(fileStream);
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) is not null)
            {
                Match match = regex.Match(line);
                if (!match.Success) continue;
                string installDir = Path.Combine(path, "common", match.Groups[1].Value);
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
            if (string.IsNullOrEmpty(oculusInstallDir)) return null;
            string finalPath = Path.Combine(oculusInstallDir, "Software", "hyperbolic-magnetism-beat-saber");
            string? installDir = _installDirValidator.ValidateInstallDir(finalPath) ? finalPath : LocateInOculusLibrary();
            return installDir is null ? null : Path.GetFullPath(installDir);
        }

        [SupportedOSPlatform("windows")]
        private string? LocateInOculusLibrary()
        {
            using RegistryKey? librariesKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Oculus VR, LLC")?.OpenSubKey("Oculus")?.OpenSubKey("Libraries");
            if (librariesKey is null) return null;
            foreach (string libraryKeyName in librariesKey.GetSubKeyNames())
            {
                using RegistryKey? libraryKey = librariesKey.OpenSubKey(libraryKeyName);
                string? libraryPath = libraryKey?.GetValue("Path")?.ToString();
                if (libraryPath is null) continue;
                string finalPath = Path.Combine(libraryPath, "Software", "hyperbolic-magnetism-beat-saber");
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
            string vdf = Path.Combine(path, "steamapps", "libraryfolders.vdf");
            await using FileStream? fileStream = IOUtils.TryOpenFile(vdf, new FileStreamOptions { Options = FileOptions.Asynchronous });
            if (fileStream is null) yield break;
            Regex regex = new("\\s\"(?:\\d|path)\"\\s+\"(.+)\"");
            string? line;
            using StreamReader vdfReader = new(fileStream);
            while ((line = await vdfReader.ReadLineAsync().ConfigureAwait(false)) is not null)
            {
                Match match = regex.Match(line);
                if (match.Success)
                    yield return Path.Combine(match.Groups[1].Value.Replace(@"\\", "/", StringComparison.Ordinal), "steamapps");
            }
        }
    }
}