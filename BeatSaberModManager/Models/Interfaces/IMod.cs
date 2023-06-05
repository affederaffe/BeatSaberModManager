using System;


namespace BeatSaberModManager.Models.Interfaces
{
    /// <summary>
    /// Defines a mod.
    /// </summary>
    public interface IMod
    {
        /// <summary>
        /// The mod's display name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The mod's version.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// The description or summary of the mod.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The category of the mod.
        /// </summary>
        string Category { get; }

        /// <summary>
        /// A link which provides more resources about the mod.
        /// </summary>
        string MoreInfoLink { get; }

        /// <summary>
        /// Indicates if the mod must be installed.
        /// </summary>
        bool IsRequired { get; }
    }
}
