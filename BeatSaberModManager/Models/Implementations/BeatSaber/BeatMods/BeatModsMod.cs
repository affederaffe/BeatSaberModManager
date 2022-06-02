using System;
using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    /// <inheritdoc cref="BeatSaberModManager.Models.Interfaces.IMod" />
    public class BeatModsMod : IMod, IEquatable<BeatModsMod>
    {
        /// <inheritdoc />
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <inheritdoc />
        [JsonPropertyName("version")]
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; } = null!;

        /// <inheritdoc />
        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        /// <inheritdoc />
        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;

        /// <inheritdoc />
        [JsonPropertyName("link")]
        public string MoreInfoLink { get; set; } = null!;

        /// <inheritdoc />
        [JsonPropertyName("required")]
        public bool IsRequired { get; set; }

        /// <summary>
        /// The downloads for the mod.
        /// </summary>
        [JsonPropertyName("downloads")]
        public BeatModsDownload[] Downloads { get; set; } = null!;

        /// <summary>
        /// The dependencies of the mod.
        /// </summary>
        [JsonPropertyName("dependencies")]
        public BeatModsDependency[] Dependencies { get; set; } = null!;

        /// <inheritdoc />
        public bool Equals(BeatModsMod? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Version.Equals(other.Version);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BeatModsMod) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Name, Version);
    }
}