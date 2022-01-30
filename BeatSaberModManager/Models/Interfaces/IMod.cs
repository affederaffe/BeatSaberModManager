using System;


namespace BeatSaberModManager.Models.Interfaces
{
    public interface IMod : IEquatable<IMod>
    {
        string Name { get; set; }
        Version Version { get; set; }
        string Description { get; set; }
        string Category { get; set; }
        string MoreInfoLink { get; set; }
    }
}