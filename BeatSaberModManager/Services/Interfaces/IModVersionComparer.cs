namespace BeatSaberModManager.Services.Interfaces
{
    public interface IModVersionComparer
    {
        int CompareVersions(string? availableVersion, string? installedVersion);
    }
}