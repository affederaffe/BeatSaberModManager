using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;


namespace BeatSaberModManager.Models.Implementations.Json
{
    [JsonSerializable(typeof(BeatModsMod[]))]
    public partial class BeatModsModJsonSerializerContext : JsonSerializerContext { }
}