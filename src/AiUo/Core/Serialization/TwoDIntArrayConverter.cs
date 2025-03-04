using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiUo.Core.Serialization;

internal class TwoDIntArrayConverter : JsonConverter<int[,]>
{
    public override int[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var json = reader.GetString();
        return SerializerUtil.DeserializeJsonNet<int[,]>(json);
    }

    public override void Write(Utf8JsonWriter writer, int[,] value, JsonSerializerOptions options)
    {
        var json = SerializerUtil.SerializeJsonNet(value);
        writer.WriteStringValue(json);
    }
}