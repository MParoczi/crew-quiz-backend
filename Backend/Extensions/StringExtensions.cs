using System.Globalization;
using System.Text;

namespace Backend.Extensions;

public static class StringExtensions
{
    public static string RemoveAccents(this string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder();

        foreach (var c in normalizedString.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)) sb.Append(c);

        return sb.ToString();
    }
}