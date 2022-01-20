using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;


namespace BeatSaberModManager.Models.Implementations.Json
{
    [JsonSerializable(typeof(BeatSaverMap))]
    public partial class BeatSaverJsonSerializerContext : JsonSerializerContext { }
}