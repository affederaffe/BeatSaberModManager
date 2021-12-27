using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utilities;

using Microsoft.Win32;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    public class BeatSaberInstallDirLocator : IInstallDirLocator
    {
        private readonly IInstallDirValidator _installDirValidator;

        public BeatSaberInstallDirLocator(IInstallDirValidator installDirValidator)
        {
            _installDirValidator = installDirValidator;
        }

        public ValueTask<string?> LocateInstallDir() =>
            OperatingSystem.IsWindows() ? LocateWindowsInstallDir()
                : OperatingSystem.IsLinux() ? LocateLinuxSteamInstallDir()
                    : throw new PlatformNotSupportedException();

        [SupportedOSPlatform("windows")]
        private ValueTask<string?> LocateWindowsInstallDir()
        {
            string? steamInstallDir = LocateWindowsSteamInstallDir();
            return steamInstallDir is null ? LocateOculusBeatSaberInstallDir() : LocateSteamBeatSaberInstallDir(steamInstallDir);
        }

        [SupportedOSPlatform("linux")]
        private ValueTask<string?> LocateLinuxSteamInstallDir()
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string steamInstallDir = Path.Combine(homeDir, ".steam", "root");
            return LocateSteamBeatSaberInstallDir(steamInstallDir);
        }

        private async ValueTask<string?> LocateSteamBeatSaberInstallDir(string steamInstallDir)
        {
            await foreach (string libPath in GetSteamLibraryPaths(steamInstallDir))
            {
                string? installDir = await MatchSteamBeatSaberInstallDir(libPath).ConfigureAwait(false);
                if (installDir is not null) return installDir;
            }

            return null;
        }

        private async Task<string?> MatchSteamBeatSaberInstallDir(string path)
        {
            const string beatSaberAppId = "620980";
            string acf = Path.Combine(path, "appmanifest_" + beatSaberAppId + ".acf");
            if (!IOUtils.TryOpenFile(acf, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous, out FileStream? fileStream)) return null;
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

            await fileStream.DisposeAsync().ConfigureAwait(false);
            return null;
        }

        [SupportedOSPlatform("windows")]
        private ValueTask<string?> LocateOculusBeatSaberInstallDir()
        {
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using RegistryKey? oculusInstallDirKey = baseKey.OpenSubKey("SOFTWARE")?.OpenSubKey("Wow6432Node")?.OpenSubKey("Oculus VR, LLC")?.OpenSubKey("Oculus")?.OpenSubKey("Config");
            string? oculusInstallDir = oculusInstallDirKey?.GetValue("InitialAppLibrary")?.ToString();
            if (string.IsNullOrEmpty(oculusInstallDir)) return new ValueTask<string?>();
            string finalPath = Path.Combine(oculusInstallDir, "Software", "hyperbolic-magnetism-beat-saber");
            string? installDir = _installDirValidator.ValidateInstallDir(finalPath) ? finalPath : LocateInOculusLibrary();
            return new ValueTask<string?>(installDir);
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
            using RegistryKey? steamInstallDirKey = Registry.LocalMachine.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam");
            return steamInstallDirKey?.GetValue("InstallPath")?.ToString();
        }

        private static async IAsyncEnumerable<string> GetSteamLibraryPaths(string path)
        {
            yield return path;
            string vdf = Path.Combine(path, "steamapps", "libraryfolders.vdf");
            if (!IOUtils.TryOpenFile(vdf, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous, out FileStream? fileStream)) yield break;
            Regex regex = new("\\s\"(?:\\d|path)\"\\s+\"(.+)\"");
            string? line;
            using StreamReader vdfReader = new(fileStream);
            while ((line = await vdfReader.ReadLineAsync().ConfigureAwait(false)) is not null)
            {
                Match match = regex.Match(line);
                if (match.Success)
                    yield return Path.Combine(match.Groups[1].Value.Replace(@"\\", "/", StringComparison.Ordinal), "steamapps");
            }

            await fileStream.DisposeAsync().ConfigureAwait(false);
        }
    }
}