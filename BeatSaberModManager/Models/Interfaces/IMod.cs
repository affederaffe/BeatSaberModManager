namespace BeatSaberModManager.Models.Interfaces
{
    public interface IMod
    {
        string Name { get; init; }
        string Version { get; init; }
        string Description { get; init; }
        string Category { get; init; }
        string MoreInfoLink { get; init; }
        bool Required { get; init; }
    }
}