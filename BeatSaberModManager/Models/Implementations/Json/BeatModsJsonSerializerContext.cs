using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;


namespace BeatSaberModManager.Models.Implementations.Json
{
    /// <inheritdoc />
    [JsonSerializable(typeof(BeatModsMod[]))]
    internal partial class BeatModsModJsonSerializerContext : JsonSerializerContext { }
}