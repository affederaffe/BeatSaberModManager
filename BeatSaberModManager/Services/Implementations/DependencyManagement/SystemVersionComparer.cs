using System;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.DependencyManagement
{
    public class SystemVersionComparer : IVersionComparer
    {
        public int CompareVersions(string? available, string? installed) =>
            Version.TryParse(available, out Version? availableVersion) && Version.TryParse(installed, out Version? installedVersion)
                ? installedVersion.CompareTo(availableVersion)
                : -1;
    }
}