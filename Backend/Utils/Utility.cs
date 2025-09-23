using System.Diagnostics;
using System.Reflection;
using System.Text;
using Serilog;

namespace Backend.Utils;

public static class Utility
{
    public static T DeepClone<T>(T? input) where T : class?
    {
        ArgumentNullException.ThrowIfNull(input);

        var stopwatch = Stopwatch.StartNew();
        var typeName = typeof(T).Name;

        try
        {
            var type = input.GetType();
            var properties = type.GetProperties();
            var clonedObj = (T)Activator.CreateInstance(type)!;

            foreach (var property in properties)
            {
                if (!property.CanWrite) continue;
                var value = property.GetValue(input);
                if (value != null && value.GetType().IsClass && !value.GetType().FullName!.StartsWith("System."))
                    property.SetValue(clonedObj, DeepClone(value));
                else
                    property.SetValue(clonedObj, value);
            }

            stopwatch.Stop();

            // Only log if operation is slow (> 500ms) to avoid noise
            if (stopwatch.ElapsedMilliseconds > 500)
                Log.Warning("Slow deep clone operation for type {TypeName} with {PropertyCount} properties took {ElapsedMs}ms",
                    typeName, properties.Length, stopwatch.ElapsedMilliseconds);

            return clonedObj;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Log.Error(ex, "Deep clone failed for type {TypeName} after {ElapsedMs}ms", typeName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public static T MergeObjects<T>(T source, T target)
    {
        var stopwatch = Stopwatch.StartNew();
        var typeName = typeof(T).Name;

        try
        {
            var mergedObj = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanWrite) continue;

                var value1 = property.GetValue(target);
                var value2 = property.GetValue(source);
                var defaultValue = GetDefault(property.PropertyType);

                if (property.PropertyType.IsValueType)
                {
                    var finalValue = value2 != null && !value2.Equals(defaultValue) ? value2 : value1;
                    property.SetValue(mergedObj, finalValue);
                }
                else
                {
                    var finalValue = value2 ?? value1 ?? defaultValue;
                    property.SetValue(mergedObj, finalValue);
                }
            }

            stopwatch.Stop();

            // Only log if operation is slow (> 200ms) to avoid noise
            if (stopwatch.ElapsedMilliseconds > 200)
                Log.Warning("Slow object merge operation for type {TypeName} with {PropertyCount} properties took {ElapsedMs}ms",
                    typeName, properties.Length, stopwatch.ElapsedMilliseconds);

            return mergedObj;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Log.Error(ex, "Object merge failed for type {TypeName} after {ElapsedMs}ms", typeName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public static string GenerateAnswerHint(string answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
            return string.Empty;

        var result = new StringBuilder(answer.Length);

        foreach (var c in answer)
            if (char.IsLetter(c))
                result.Append('_');
            else
                // Preserve spaces, punctuation, and numbers
                result.Append(c);

        return result.ToString();
    }

    private static object? GetDefault(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}