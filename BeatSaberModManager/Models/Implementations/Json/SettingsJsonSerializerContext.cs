using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.Settings;


namespace BeatSaberModManager.Models.Implementations.Json
{
    [JsonSerializable(typeof(AppSettings))]
    internal partial class SettingsJsonSerializerContext : JsonSerializerContext { }
}