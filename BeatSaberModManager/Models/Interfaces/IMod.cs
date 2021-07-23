namespace BeatSaberModManager.Models.Interfaces
{
    public interface IMod
    {
        string? Name { get; set; }
        string? Version { get; set; }
        string? GameVersion { get; set; }
        string? Id { get; set; }
        string? Status { get; set; }
        string? Description { get; set; }
        string? Category { get; set; }
        string? MoreInfoLink { get; set; }
        bool Required { get; set; }
        IAuthor? Author { get; set; }
        IDownload[]? Downloads { get; set; }
        IDependency[]? Dependencies { get; set; }
    }
}