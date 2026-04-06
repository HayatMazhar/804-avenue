using System.Text.Json;

namespace Avenue804.Web.Infrastructure;

/// <summary>Reads <see cref="Domain.LookupValue.Metadata"/> JSON for property-detail filtering: <c>{"forTypes":["residential"]}</c>. Empty = all types.</summary>
public static class PropertyDetailLookupMetadata
{
    public static string ForTypesDataAttribute(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
            return "[]";

        try
        {
            using var doc = JsonDocument.Parse(metadata);
            if (doc.RootElement.TryGetProperty("forTypes", out var arr) &&
                arr.ValueKind == JsonValueKind.Array)
                return arr.GetRawText();
        }
        catch (JsonException)
        {
            /* ignore */
        }

        return "[]";
    }
}
