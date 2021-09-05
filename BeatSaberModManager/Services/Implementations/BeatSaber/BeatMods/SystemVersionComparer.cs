using System;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    public class SystemVersionComparer : IModVersionComparer
    {
        public int CompareVersions(string? availableVersion, string? installedVersion) =>
            Version.TryParse(availableVersion, out Version? parsedAvailableVersion) && Version.TryParse(installedVersion, out Version? parsedInstalledVersion)
                ? parsedInstalledVersion.CompareTo(parsedAvailableVersion)
                : -1;
    }
}