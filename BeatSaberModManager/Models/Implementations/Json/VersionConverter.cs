using System;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Json
{
    public class VersionConverter : JsonConverter<Version>
    {
        public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Version.TryParse(reader.GetString() ?? string.Empty, out Version? version) ? version : new Version(0, 0, 0);

        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options) =>
            writer.WriteString("Version", value.ToString());
    }
}