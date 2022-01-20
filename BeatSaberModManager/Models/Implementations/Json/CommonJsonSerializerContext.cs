using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Json
{
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(Dictionary<string, string[]>))]
    public partial class CommonJsonSerializerContext : JsonSerializerContext { }
}