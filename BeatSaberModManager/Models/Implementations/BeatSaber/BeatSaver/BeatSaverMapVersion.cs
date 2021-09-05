using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverMapVersion
    {
        [JsonPropertyName("downloadURL")]
        public string DownloadUrl { get; init; } = null!;
    }
}