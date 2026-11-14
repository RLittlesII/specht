using System.Text.Json.Nodes;
using Specht.Discovery;
using Specht.Manifest;

namespace Specht.Model;

/// <summary>A Feature's specification, resolved against what sits beside it.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class FeatureSpec
{
    /// <summary>A Feature's specification from its parts, read through the manifest's keys and identity form.</summary>
    /// <param name="location">Where it was discovered.</param>
    /// <param name="document">The parsed document.</param>
    /// <param name="featureFiles">The Gherkin files beside it.</param>
    /// <param name="keys">The frontmatter key each role is read from, or null for the names every older pin reads.</param>
    /// <param name="forms">The forms its identity is written in, or null for <c>{epic}-{id}</c> (decision 0018).</param>
    /// <param name="claimGrammar">The grammar its claim ids are read with, or null for the manifest's <c>identifiers.claim</c>.</param>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-005" />
    /// <implements epic="0001" feature="F5" claim="B-079" />
    public FeatureSpec(
        SpechtLocation location,
        SpechtDocument document,
        IReadOnlyList<FeatureFile> featureFiles,
        SpechtFeatureKeys? keys = null,
        SpechtForms? forms = null,
        SpechtClaimGrammar? claimGrammar = null)
    {
        Location = location;
        Document = document;
        FeatureFiles = Array.AsReadOnly(featureFiles.ToArray());
        Keys = keys ?? new SpechtFeatureKeys();
        ClaimGrammar = claimGrammar;
        _forms = forms;
    }

    /// <summary>Where it was discovered, and in which layout.</summary>
    public SpechtLocation Location { get; }

    /// <summary>The parsed document.</summary>
    public SpechtDocument Document { get; }

    /// <summary>The Gherkin files beside it - exactly one is expected.</summary>
    public IReadOnlyList<FeatureFile> FeatureFiles { get; }

    /// <summary>Path relative to the repository root.</summary>
    public string RelativePath => Location.RelativePath;

    /// <summary>The frontmatter key each role is read from.</summary>
    public SpechtFeatureKeys Keys { get; }

    /// <summary>The grammar its claim ids and claim tags are read with; null reads the manifest's <c>identifiers.claim</c>.</summary>
    public SpechtClaimGrammar? ClaimGrammar { get; }

    /// <summary>The claim file set that names this Feature, the same instance as the manifest's entry; null for a layout's Feature.</summary>
    /// <implements epic="0001" feature="F16" claim="B-026" />
    public SpechtClaimFileSet? ClaimFileSet { get; init; }

    /// <summary>The root-relative path of the claim file set's loaded record; null for a layout's Feature or a set read with no record.</summary>
    /// <implements epic="0001" feature="F16" claim="B-003" />
    public string? Record { get; init; }

    /// <summary>
    /// A native Feature's lifecycle: its record's text at the manifest's lifecycle key, or <c>current</c> with no record;
    /// null for a layout's Feature.
    /// </summary>
    /// <implements epic="0001" feature="F16" claim="B-003" />
    public string? Lifecycle { get; init; }

    /// <summary>The epic frontmatter value, or <c>null</c>; a native Feature's is its claim file set's, whatever its record declares.</summary>
    /// <implements epic="0001" feature="F16" claim="B-037" />
    public string? Epic => ClaimFileSet is { } set ? set.Epic : Document.Frontmatter.Text(Keys.Epic);

    /// <summary>The id frontmatter value, or <c>null</c>; a native Feature's is its claim file set's, whatever its record declares.</summary>
    /// <implements epic="0001" feature="F16" claim="B-037" />
    public string? Id => ClaimFileSet is { } set ? set.Feature : Document.Frontmatter.Text(Keys.Id);

    /// <summary>The specification's maturity frontmatter value, or <c>null</c>.</summary>
    public string? SpecStatus => Document.Frontmatter.Text(Keys.SpecStatus);

    /// <summary>The delivery status frontmatter value, or <c>null</c>.</summary>
    public string? Status => Document.Frontmatter.Text(Keys.Status);

    /// <summary>The qualified identity, e.g. <c>0001-F1</c>, written in the identity form when one is read.</summary>
    public string Identity => _forms is null ? $"{Epic}-{Id}" : SpechtForms.Render(_forms.Identity, Epic ?? string.Empty, Id ?? string.Empty);

    /// <summary>A frontmatter string array, empty when absent.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public IReadOnlyList<string> Strings(string key)
    {
        if (Document.Frontmatter.Node?[key] is not JsonArray array)
        {
            return [];
        }

        var values = new List<string>();

        foreach (var item in array)
        {
            if (item is JsonValue value && value.TryGetValue<string>(out var text))
            {
                values.Add(text);
            }
        }

        return values;
    }

    private readonly SpechtForms? _forms;
}
