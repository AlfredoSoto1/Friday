using System.Reflection;
using System.Runtime.Serialization;

namespace Utils;

public static class EnumHelper
{
  public static T? ParseFromValue<T>(string? value) where T : struct, Enum
  {
    if (string.IsNullOrEmpty(value)) return null;

    // First try parsing by EnumMember attribute values (handles "3", "F-150", "Model 3")
    foreach (T enumValue in Enum.GetValues<T>())
    {
      var field = typeof(T).GetField(enumValue.ToString());
      var attribute = field?.GetCustomAttribute<EnumMemberAttribute>();

      if (attribute?.Value?.Equals(value, StringComparison.OrdinalIgnoreCase) == true)
        return enumValue;
    }

    // Then try direct enum name parsing (handles "Mazda3", "Camry", etc.)
    if (Enum.TryParse<T>(value, true, out T result)) return result;

    return null;
  }

  public static string ToDbString<T>(T value) where T : struct, Enum
  {
    var field = typeof(T).GetField(value.ToString());
    var attribute = field?.GetCustomAttribute<EnumMemberAttribute>();
    return attribute?.Value ?? value.ToString();
  }

  public static string? ToDbStringOrNull<T>(T? value) where T : struct, Enum
      => value.HasValue ? ToDbString(value.Value) : null;
}
