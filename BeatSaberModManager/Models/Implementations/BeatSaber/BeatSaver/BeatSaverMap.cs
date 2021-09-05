using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverMap
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        [JsonPropertyName("metadata")]
        public BeatSaverMapMetaData MetaData { get; init; } = null!;

        [JsonPropertyName("versions")]
        public BeatSaverMapVersion[] Versions { get; init; } = null!;
    }
}