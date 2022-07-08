using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver
{
    /// <summary>
    /// Additional metadata for a <see cref="BeatSaverMap"/>.
    /// </summary>
    public class BeatSaverMapMetaData
    {
        /// <summary>
        /// The name of the mapper.
        /// </summary>
        [JsonPropertyName("levelAuthorName")]
        public string LevelAuthorName { get; set; } = null!;

        /// <summary>
        /// The name of the song.
        /// </summary>
        [JsonPropertyName("songName")]
        public string SongName { get; set; } = null!;
    }
}
