using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    /// <summary>
    /// Defines a dependency for a <see cref="BeatModsMod"/>.
    /// </summary>
    public class BeatModsDependency
    {
        /// <summary>
        /// The name of the dependency.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        /// <summary>
        /// Cached <see cref="IMod"/> to avoid finding it again by <see cref="Name"/>.
        /// </summary>
        [JsonIgnore]
        public IMod? DependingMod { get; set; }
    }
}
