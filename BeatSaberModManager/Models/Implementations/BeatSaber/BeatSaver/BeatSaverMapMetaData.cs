using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverMapMetaData
    {
        [JsonPropertyName("levelAuthorName")]
        public string LevelAuthorName { get; init; } = null!;

        [JsonPropertyName("songName")]
        public string SongName { get; init; } = null!;
    }
}