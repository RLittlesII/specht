using System.Text.Json;

using Specht.Model;

namespace Specht.Facts;

/// <summary>
/// The facts document (<c>0132-F2</c>): the shape <c>docs/schema/facts.schema.json</c> publishes, projected from one
/// <see cref="SpechtModel"/>. No rule runs, so it carries no violation, count or verdict (B-014), and it is a function of
/// the model alone, so nothing in it reads the clock, the machine or the environment (B-022).
/// </summary>
/// <param name="Specifications">Every discovered Feature specification, in discovery order.</param>
/// <implements epic="0132" feature="F2" claim="B-001" />
/// <implements epic="0132" feature="F2" claim="B-014" />
public sealed record FactsDocument(IReadOnlyList<FeatureFacts> Specifications)
{
    /// <summary>Makes the document for <paramref name="model"/>; its epic files are not read (B-026).</summary>
    /// <param name="model">The model the check's own read gives.</param>
    /// <returns>The document.</returns>
    /// <implements epic="0132" feature="F2" claim="B-001" />
    /// <implements epic="0132" feature="F2" claim="B-003" />
    /// <implements epic="0132" feature="F2" claim="B-004" />
    /// <implements epic="0132" feature="F2" claim="B-017" />
    /// <implements epic="0132" feature="F2" claim="B-026" />
    public static FactsDocument From(SpechtModel model)
    {
        var identified = model.FeaturesWithIdentity.ToHashSet();
        return new(model.Features
            .Select(feature => new FeatureFacts(
                feature.RelativePath,
                identified.Contains(feature) ? feature.Identity : null,
                feature.SpecStatus,
                feature.Status,
                feature.Strings(feature.Keys.DependsOn),
                feature.Strings(feature.Keys.Blocks),
                feature.Strings(feature.Keys.Children)))
            .ToList());
    }

    /// <summary>Serializes the document as the JSON the published facts schema describes, every member written.</summary>
    /// <returns>The JSON text.</returns>
    /// <implements epic="0132" feature="F2" claim="B-002" />
    /// <implements epic="0132" feature="F2" claim="B-022" />
    public string ToJson() => JsonSerializer.Serialize(this, Options);

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        NewLine = "\n",
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
