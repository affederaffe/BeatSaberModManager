using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.JsonSerializerContexts
{
    [JsonSerializable(typeof(Settings))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class SettingsJsonSerializerContext : JsonSerializerContext { }
}