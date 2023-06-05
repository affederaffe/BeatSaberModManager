using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    /// <summary>
    /// A file of a <see cref="BeatModsMod"/> and it's MD5 hash.
    /// </summary>
    public class BeatModsHash
    {
        /// <summary>
        /// The MD5 hash for the <see cref="File"/>.
        /// </summary>
        [JsonPropertyName("hash")]
        public required string Hash { get; init; }

        /// <summary>
        /// The relative file path.
        /// </summary>
        [JsonPropertyName("file")]
        public required string File { get; init; }
    }
}
