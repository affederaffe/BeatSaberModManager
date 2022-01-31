namespace BeatSaberModManager.Models.Implementations
{
    public readonly struct InstallDirLocatorResult
    {
        public InstallDirLocatorResult(string? installDir, PlatformType platformType)
        {
            InstallDir = installDir;
            PlatformType = platformType;
        }

        public string? InstallDir { get; }

        public PlatformType PlatformType { get; }

        public static InstallDirLocatorResult Empty => new();
    }
}