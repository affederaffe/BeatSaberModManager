using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;


namespace BeatSaberModManager.Models.Implementations.JsonSerializerContexts
{
    [JsonSerializable(typeof(BeatModsMod[]))]
    public partial class BeatModsModArrayJsonSerializerContext : JsonSerializerContext { }
}