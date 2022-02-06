using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;


namespace BeatSaberModManager.Models.Implementations.Json
{
    /// <inheritdoc />
    [JsonSerializable(typeof(BeatSaverMap))]
    internal partial class BeatSaverJsonSerializerContext : JsonSerializerContext { }
}