using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverMapMetaData
    {
        [JsonPropertyName("levelAuthorName")]
        public string? LevelAuthorName { get; set; }

        [JsonPropertyName("songName")]
        public string? SongName { get; set; }
    }
}