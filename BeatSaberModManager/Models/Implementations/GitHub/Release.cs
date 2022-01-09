using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.GitHub
{
    public class Release
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = null!;

        [JsonPropertyName("assets")]
        public Asset[] Assets { get; set; } = null!;
    }
}