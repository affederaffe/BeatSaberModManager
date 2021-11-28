using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;


namespace BeatSaberModManager.Models.Implementations.JsonSerializerContexts
{
    [JsonSerializable(typeof(BeatSaverMap))]
    public partial class BeatSaverJsonSerializerContext : JsonSerializerContext { }
}