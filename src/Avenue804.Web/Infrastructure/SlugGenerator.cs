using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Avenue804.Web.Infrastructure;

public static class SlugGenerator
{
    public static string FromTitle(string title, string? fallback = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.IsNullOrWhiteSpace(fallback) ? "item" : FromTitle(fallback, "item");

        var normalized = title.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(c))
                sb.Append(c);
            else if (char.IsWhiteSpace(c) || c is '-' or '_')
                sb.Append('-');
        }

        var slug = Regex.Replace(sb.ToString().Trim('-'), "-{2,}", "-");
        return string.IsNullOrEmpty(slug) ? "item" : slug;
    }
}
