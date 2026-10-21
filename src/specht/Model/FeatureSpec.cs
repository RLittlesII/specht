using System.Text.Json.Nodes;
using Specht.Discovery;

namespace Specht.Model;

/// <summary>A Feature's specification, resolved against what sits beside it.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class FeatureSpec
{
    /// <summary>A Feature's specification from its parts.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public FeatureSpec(SpecLocation location, SpecDocument document, IReadOnlyList<FeatureFile> featureFiles)
    {
        Location = location;
        Document = document;
        FeatureFiles = Array.AsReadOnly(featureFiles.ToArray());
    }

    /// <summary>Where it was discovered, and in which layout.</summary>
    public SpecLocation Location { get; }

    /// <summary>The parsed document.</summary>
    public SpecDocument Document { get; }

    /// <summary>The Gherkin files beside it - exactly one is expected.</summary>
    public IReadOnlyList<FeatureFile> FeatureFiles { get; }

    /// <summary>Path relative to the repository root.</summary>
    public string RelativePath => Location.RelativePath;

    /// <summary>The <c>epic</c> frontmatter value, or <c>null</c>.</summary>
    public string? Epic => Document.Frontmatter.Text("epic");

    /// <summary>The <c>id</c> frontmatter value, or <c>null</c>.</summary>
    public string? Id => Document.Frontmatter.Text("id");

    /// <summary>The <c>spec_status</c> frontmatter value, or <c>null</c>.</summary>
    public string? SpecStatus => Document.Frontmatter.Text("spec_status");

    /// <summary>The qualified identity, e.g. <c>0001-F1</c>.</summary>
    public string Identity => $"{Epic}-{Id}";

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
}
