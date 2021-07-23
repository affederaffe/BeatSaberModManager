namespace BeatSaberModManager.Models.Interfaces
{
    public interface IDependency
    {
        string? Name { get; set; }
        string? Id { get; set; }
        IMod? DependingMod { get; set; }
    }
}