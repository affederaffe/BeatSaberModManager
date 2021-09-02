using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

using BeatSaberModManager.Models.Interfaces;

using Microsoft.Win32;


namespace BeatSaberModManager.Models.Implementations.BeatSaber
{
    // Basically everything here is stolen from ModAssistant, thanks!
    public class BeatSaberInstallDirLocator : IInstallDirLocator
    {
        private const string kBeatSaberAppId = "620980";

        public string? DetectInstallDir()
        {
            if (OperatingSystem.IsWindows()) return LocateWindowsInstallDir();
            if (OperatingSystem.IsLinux()) return LocateLinuxSteamInstallDir();
            throw new PlatformNotSupportedException();
        }

        private static string? LocateWindowsInstallDir()
        {
            string? steamInstallDir = LocateWindowsSteamInstallDir();
            return !string.IsNullOrEmpty(steamInstallDir) ? LocateSteamBeatSaberInstallDir(steamInstallDir) : LocateWindowsOculusBeatSaberDir();
        }

        private static string? LocateWindowsSteamInstallDir()
        {
            if (!OperatingSystem.IsWindows()) return null;
            using RegistryKey? steamInstallDirKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam") ??
                  Registry.LocalMachine.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam");
            return steamInstallDirKey?.GetValue("InstallPath")?.ToString();
        }

        private static string? LocateLinuxSteamInstallDir()
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string steamInstallDir = Path.Combine(homeDir, ".steam", "root");
            return LocateSteamBeatSaberInstallDir(steamInstallDir);
        }

        private static string? LocateSteamBeatSaberInstallDir(string steamInstallDir)
        {
            string vdf = Path.Combine(steamInstallDir, "steamapps/libraryfolders.vdf");
            if (!File.Exists(vdf)) return null;
            Regex regex = new("\\s\"(?:\\d|path)\"\\s+\"(.+)\"");
            List<string> steamPaths = new() { Path.Combine(steamInstallDir, "steamapps") };

            string? line;
            using StreamReader vdfReader = new(vdf);
            while ((line = vdfReader.ReadLine()) is not null)
            {
                Match match = regex.Match(line);
                if (match.Success)
                    steamPaths.Add(Path.Combine(match.Groups[1].Value.Replace(@"\\", "/"), "steamapps"));
            }

            regex = new Regex("\\s\"installdir\"\\s+\"(.+)\"");
            foreach (string path in steamPaths)
            {
                string acf = Path.Combine(path, "appmanifest_" + kBeatSaberAppId + ".acf");
                if (!File.Exists(acf)) continue;
                using StreamReader reader = new(acf);
                while ((line = reader.ReadLine()) is not null)
                {
                    Match match = regex.Match(line);
                    if (!match.Success) continue;
                    string installDir = Path.Combine(path, "common", match.Groups[1].Value);
                    if (File.Exists(Path.Combine(installDir, "Beat Saber.exe")))
                        return installDir;
                }
            }

            return null;
        }

        private static string? LocateWindowsOculusBeatSaberDir()
        {
            if (!OperatingSystem.IsWindows()) return null;
            using RegistryKey? oculusInstallDirKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE")?.OpenSubKey("Wow6432Node")?.OpenSubKey("Oculus VR, LLC")?.OpenSubKey("Oculus")?.OpenSubKey("Config");
            string? oculusInstallDir = oculusInstallDirKey?.GetValue("InitialAppLibrary")?.ToString();
            if (string.IsNullOrEmpty(oculusInstallDir)) return null;

            if (!string.IsNullOrEmpty(oculusInstallDir) && File.Exists(Path.Combine(oculusInstallDir, "Software", "hyperbolic-magnetism-beat-saber", "Beat Saber.exe")))
                return Path.Combine(oculusInstallDir, "Software", "hyperbolic-magnetism-beat-saber");

            // Yoinked this code from Umbranox's Mod Manager. Lot's of thanks and love for Umbra <3
            using RegistryKey? librariesKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Oculus VR, LLC")?.OpenSubKey("Oculus")?.OpenSubKey("Libraries");
            if (librariesKey is null) return null;

            // Oculus libraries uses GUID volume paths like this "\\?\Volume{0fea75bf-8ad6-457c-9c24-cbe2396f1096}\Games\Oculus Apps", we need to transform these to "D:\Game"\Oculus Apps"
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

            // Search among the library folders
            foreach (string libraryKeyName in librariesKey.GetSubKeyNames())
            {
                using RegistryKey? libraryKey = librariesKey.OpenSubKey(libraryKeyName);
                string? libraryPath = libraryKey?.GetValue("Path")?.ToString();
                if (libraryPath is null) return null;
                // Yoinked this code from Megalon's fix. <3
                string guidLetter = guidLetterVolumes.FirstOrDefault(x => libraryPath.Contains(x.Key)).Value;
                if (string.IsNullOrEmpty(guidLetter)) continue;
                string finalPath = Path.Combine(guidLetter, libraryPath[49..], "Software/hyperbolic-magnetism-beat-saber");
                if (File.Exists(Path.Combine(finalPath, "Beat Saber.exe")))
                    return finalPath;
            }

            return null;
        }
    }
}