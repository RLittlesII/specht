using System.Text.Json;
using System.Text.Json.Serialization;

namespace Specht.Report;

/// <summary>Serializes the report document as the JSON the published report schema describes (<c>0001-F3</c> B-005; item <c>0167</c>).</summary>
/// <implements epic="0001" feature="F3" claim="B-001" />
public sealed class SpechtReportSerializer
{
    /// <summary>Serializes <paramref name="document"/>.</summary>
    /// <param name="document">The report document.</param>
    /// <returns>The JSON text.</returns>
    /// <implements epic="0001" feature="F3" claim="B-001" />
    public string Serialize(SpechtReportDocument document) => JsonSerializer.Serialize(document, Options);

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        NewLine = "\n",
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}
