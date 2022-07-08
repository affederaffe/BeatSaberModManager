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
        public bool Equals(BeatModsMod? other) => other is not null && (ReferenceEquals(this, other) || (Name == other.Name && Version.Equals(other.Version)));

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is not null && (ReferenceEquals(this, obj) || (obj.GetType() == GetType() && Equals(obj as BeatModsMod)));

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Name, Version);
    }
}
