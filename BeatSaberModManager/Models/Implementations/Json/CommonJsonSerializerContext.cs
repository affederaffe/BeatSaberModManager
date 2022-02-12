using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Json
{
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(Dictionary<string, string[]>))]
    internal partial class CommonJsonSerializerContext : JsonSerializerContext { }
}