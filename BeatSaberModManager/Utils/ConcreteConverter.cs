using System;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace BeatSaberModManager.Utils
{
    public class ConcreteConverter<TInterface, TImplementation> : JsonConverter<TInterface> where TImplementation : TInterface
    {
        public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<TImplementation>(ref reader, options);

        public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, (TImplementation)value!, options);
    }
}