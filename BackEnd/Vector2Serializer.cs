using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

public class Vector2Converter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        float x = 0, y = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new Vector2(x, y);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "X":
                        x = reader.GetSingle();
                        break;
                    case "Y":
                        y = reader.GetSingle();
                        break;
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}