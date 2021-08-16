namespace BeatSaberModManager.Models.Implementations.Interfaces
{
    public interface IMod
    {
        string? Name { get; set; }
        string? Version { get; set; }
        string? Description { get; set; }
        string? Category { get; set; }
        string? MoreInfoLink { get; set; }
        bool Required { get; set; }
    }
}