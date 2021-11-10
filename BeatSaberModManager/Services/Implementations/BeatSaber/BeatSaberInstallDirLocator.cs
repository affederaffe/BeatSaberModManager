using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

using BeatSaberModManager.Services.Interfaces;

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

        public string? LocateInstallDir()
        {
            if (OperatingSystem.IsWindows()) return LocateWindowsInstallDir();
            if (OperatingSystem.IsLinux()) return LocateLinuxSteamInstallDir();
            throw new PlatformNotSupportedException();
        }

        private string? LocateWindowsInstallDir()
        {
            string? steamInstallDir = LocateWindowsSteamInstallDir();
            return steamInstallDir is null ? LocateOculusBeatSaberInstallDir() : LocateSteamBeatSaberInstallDir(steamInstallDir);
        }

        private string? LocateLinuxSteamInstallDir()
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string steamInstallDir = Path.Combine(homeDir, ".steam", "root");
            return LocateSteamBeatSaberInstallDir(steamInstallDir);
        }

        private string? LocateSteamBeatSaberInstallDir(string steamInstallDir) =>
            GetSteamLibraryPaths(steamInstallDir).Select(MatchSteamBeatSaberInstallDir).FirstOrDefault(installDir => installDir is not null);

        private string? MatchSteamBeatSaberInstallDir(string path)
        {
            const string beatSaberAppId = "620980";
            string acf = Path.Combine(path, "appmanifest_" + beatSaberAppId + ".acf");
            if (!File.Exists(acf)) return null;
            Regex regex = new("\\s\"installdir\"\\s+\"(.+)\"");
            string? line;
            using StreamReader reader = new(acf);
            while ((line = reader.ReadLine()) is not null)
            {
                Match match = regex.Match(line);
                if (!match.Success) continue;
                string installDir = Path.Combine(path, "common", match.Groups[1].Value);
                if (_installDirValidator.ValidateInstallDir(installDir))
                    return installDir;
            }

            return null;
        }

        private string? LocateOculusBeatSaberInstallDir()
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            using RegistryKey? oculusInstallDirKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE")?.OpenSubKey("Wow6432Node")?.OpenSubKey("Oculus VR, LLC")?.OpenSubKey("Oculus")?.OpenSubKey("Config");
            string? oculusInstallDir = oculusInstallDirKey?.GetValue("InitialAppLibrary")?.ToString();
            if (string.IsNullOrEmpty(oculusInstallDir)) return null;
            string finalPath = Path.Combine(oculusInstallDir, "Software", "hyperbolic-magnetism-beat-saber");
            if (!string.IsNullOrEmpty(oculusInstallDir) && _installDirValidator.ValidateInstallDir(finalPath))
                return finalPath;
            return LocateInOculusLibrary();
        }

        private string? LocateInOculusLibrary()
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            using RegistryKey? librariesKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Oculus VR, LLC")?.OpenSubKey("Oculus")?.OpenSubKey("Libraries");
            if (librariesKey is null) return null;
            Dictionary<string, string> guidLetterVolumes = GetWindowsDriveLetters();
            foreach (string libraryKeyName in librariesKey.GetSubKeyNames())
            {
                using RegistryKey? libraryKey = librariesKey.OpenSubKey(libraryKeyName);
                string? libraryPath = libraryKey?.GetValue("Path")?.ToString();
                if (libraryPath is null) continue;
                string guidLetter = guidLetterVolumes.FirstOrDefault(x => libraryPath.Contains(x.Key)).Value;
                if (string.IsNullOrEmpty(guidLetter)) continue;
                string finalPath = Path.Combine(guidLetter, libraryPath[49..], "Software", "hyperbolic-magnetism-beat-saber");
                if (_installDirValidator.ValidateInstallDir(finalPath))
                    return finalPath;
            }

            return null;
        }

        private static Dictionary<string, string> GetWindowsDriveLetters()
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            WqlObjectQuery wqlQuery = new("SELECT * FROM Win32_Volume");
            using ManagementObjectSearcher searcher = new(wqlQuery);
            Dictionary<string, string> guidLetterVolumes = new();
            foreach (ManagementBaseObject disk in searcher.Get())
            {
                string diskId = ((string)disk.GetPropertyValue("DeviceID")).Substring(11, 36);
                string diskLetter = (string)disk.GetPropertyValue("DriveLetter") + "/";
                if (!string.IsNullOrWhiteSpace(diskLetter))
                    guidLetterVolumes.Add(diskId, diskLetter);
            }

            return guidLetterVolumes;
        }

        private static string? LocateWindowsSteamInstallDir()
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            using RegistryKey? steamInstallDirKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam") ??
                                                    Registry.LocalMachine.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam");
            return steamInstallDirKey?.GetValue("InstallPath")?.ToString();
        }

        private static IEnumerable<string> GetSteamLibraryPaths(string path)
        {
            yield return path;
            string vdf = Path.Combine(path, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(vdf)) yield break;
            Regex regex = new("\\s\"(?:\\d|path)\"\\s+\"(.+)\"");
            string? line;
            using StreamReader vdfReader = new(vdf);
            while ((line = vdfReader.ReadLine()) is not null)
            {
                Match match = regex.Match(line);
                if (match.Success)
                    yield return Path.Combine(match.Groups[1].Value.Replace(@"\\", "/"), "steamapps");
            }
        }
    }
}