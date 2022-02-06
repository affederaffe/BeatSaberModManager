using System;


namespace BeatSaberModManager.Models.Interfaces
{
    /// <summary>
    /// A
    /// </summary>
    public interface IMod
    {
        /// <summary>
        /// The mod's display name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The mod's version
        /// </summary>
        Version Version { get; set; }

        /// <summary>
        /// A description or summary of the mod
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Category { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string MoreInfoLink { get; set; }
    }
}