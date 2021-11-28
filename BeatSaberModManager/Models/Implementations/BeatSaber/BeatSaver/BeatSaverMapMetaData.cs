using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverMapMetaData
    {
        [JsonPropertyName("levelAuthorName")]
        public string LevelAuthorName { get; set; } = null!;

        [JsonPropertyName("songName")]
        public string SongName { get; set; } = null!;
    }
}