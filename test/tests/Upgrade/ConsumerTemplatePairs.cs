using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Pairs of templates of <c>0001-F7</c> B-060 and B-062, each with a manifest naming the first as the consumer's: version
/// 0.2.0's shipped manifest with <c>consumerTemplates</c> holding that one file name, so it lacks no key that version
/// defines, then the named template and a template the manifest does not name and no source hashes. Once naming the feature
/// template beside the ADR template, and once naming the lesson template beside the feature template.
/// </summary>
public sealed class ConsumerTemplatePairs : TheoryData<string, string, string>
{
    public ConsumerTemplatePairs()
    {
        var defined = JsonNode.Parse(
            UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        foreach (var (named, unnamed) in new[] { ("feature.md", "adr.md"), ("lesson.md", "feature.md") })
        {
            var manifest = defined.DeepClone().AsObject();
            manifest["consumerTemplates"] = new JsonArray(named);
            Add(manifest.ToJsonString(), named, unnamed);
        }
    }
}
