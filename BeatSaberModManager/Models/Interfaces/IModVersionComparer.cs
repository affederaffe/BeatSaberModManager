namespace BeatSaberModManager.Models.Implementations.Interfaces
{
    public interface IModVersionComparer
    {
        int CompareVersions(string? availableVersion, string? installedVersion);
    }
}