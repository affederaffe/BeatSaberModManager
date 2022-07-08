using System.Collections.Generic;
using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;


namespace BeatSaberModManager.Models.Implementations.Json
{
    [JsonSerializable(typeof(HashSet<BeatModsMod>))]
    internal partial class BeatModsModJsonSerializerContext : JsonSerializerContext { }
}
