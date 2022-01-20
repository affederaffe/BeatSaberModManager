namespace BeatSaberModManager.Services.Interfaces
{
    public interface IVersionComparer
    {
        int CompareVersions(string? available, string? installed);
    }
}