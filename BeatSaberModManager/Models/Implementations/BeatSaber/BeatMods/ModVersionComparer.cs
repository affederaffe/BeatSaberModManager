using System;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class SemVerModVersionComparer : IModVersionComparer
    {
        public int CompareVersions(string? availableVersion, string? installedVersion)
        {
            if (!Version.TryParse(availableVersion, out Version? parsedAvailableVersion)) return -1;
            Version.TryParse(installedVersion, out Version? parsedInstalledVersion);
            return parsedAvailableVersion.CompareTo(parsedInstalledVersion);
        }
    }
}