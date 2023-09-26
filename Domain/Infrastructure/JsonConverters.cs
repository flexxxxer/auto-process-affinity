using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Infrastructure;

public class JsonEnumStringConverter<T> : JsonConverter<T> where T : Enum
{
  public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    => Enum.TryParse(typeToConvert, reader.GetString(), true, out var parsedEnum) ? (T?)parsedEnum : default;

  public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    => writer.WriteStringValue(value.ToString());
}