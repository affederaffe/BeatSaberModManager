using System;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class SystemVersionComparer : IModVersionComparer
    {
        public int CompareVersions(string? availableVersion, string? installedVersion) =>
            Version.TryParse(availableVersion, out Version? parsedAvailableVersion) && Version.TryParse(installedVersion, out Version? parsedInstalledVersion)
                ? parsedInstalledVersion.CompareTo(parsedAvailableVersion)
                : -1;
    }
}