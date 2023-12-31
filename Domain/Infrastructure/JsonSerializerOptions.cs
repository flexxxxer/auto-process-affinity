﻿using System.Text.Json;

namespace Domain.Infrastructure;

public static class SerializerOptions
{
  public static JsonSerializerOptions Default 
    => new()
    {
      WriteIndented = true,
      Converters =
      {
        new JsonEnumStringConverter<AffinityMode>(),
        new JsonEnumStringConverter<AffinityApplyingMode>(),
        new JsonEnumStringConverter<StartupWindowState>(),
        new JsonEnumStringConverter<StartupLocationMode>(),
        new JsonEnumStringConverter<StartupSizeMode>(),
        new JsonEnumStringConverter<AppTheme>(),
        new JsonEnumStringConverter<AppDarkThemeVariant>(),
      }
    };
}
