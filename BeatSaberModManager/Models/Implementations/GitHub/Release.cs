using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.GitHub
{
    /// <summary>
    /// A release on https://github.com.
    /// </summary>
    public class Release
    {
        /// <summary>
        /// The tag of the <see cref="Release"/>.
        /// </summary>
        [JsonPropertyName("tag_name")]
        public required string TagName { get; init; }

        /// <summary>
        /// All assets of the <see cref="Release"/>.
        /// </summary>
        [JsonPropertyName("assets")]
        public required IReadOnlyList<Asset> Assets { get; init; }
    }
}
