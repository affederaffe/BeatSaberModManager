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
        public string Hash { get; set; } = null!;

        /// <summary>
        /// The relative file path.
        /// </summary>
        [JsonPropertyName("file")]
        public string File { get; set; } = null!;
    }
}