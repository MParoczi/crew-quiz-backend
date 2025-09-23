using System.ComponentModel;
using System.Reflection;

namespace Backend.Extensions;

public static class EnumExtensions
{
    public static string GetDescription<T>(this T enumValue) where T : Enum
    {
        var enumFieldName = enumValue.ToString();
        var field = typeof(T).GetField(enumFieldName);
        if (field == null) return enumValue.ToString();

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute == null ? enumValue.ToString() : attribute.Description;
    }
}