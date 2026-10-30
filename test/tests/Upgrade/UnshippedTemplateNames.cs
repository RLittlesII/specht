using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Manifests naming no template as the consumer's, each with a name the test adds to <c>consumerTemplates</c> that version
/// 0.2.0 does not ship as a template (<c>0001-F7</c> B-063, B-144): the consumer's own <c>runbook.md</c>, under the embedded
/// source and under the on-disk source with no upstream schema source recorded, and <c>Feature.md</c>, which differs from a
/// shipped template only in case and so names none, since a name matches ordinally on the bare file name (decision 0019
/// call 5, a design choice of § 7). Each manifest is version 0.2.0's shipped manifest, so it lacks no key that version
/// defines.
/// </summary>
public sealed class UnshippedTemplateNames : TheoryData<string, string>
{
    public UnshippedTemplateNames()
    {
        var defined = JsonNode.Parse(
            UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        Add(defined.ToJsonString(), "runbook.md");
        Add(defined.ToJsonString(), "Feature.md");

        var disk = defined.DeepClone().AsObject();
        disk["schemaSource"] = "disk";
        Add(disk.ToJsonString(), "runbook.md");
    }
}
