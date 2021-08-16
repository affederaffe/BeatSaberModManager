namespace BeatSaberModManager.Models.Interfaces
{
    public interface IModVersionComparer
    {
        int CompareVersions(string? availableVersion, string? installedVersion);
    }
}