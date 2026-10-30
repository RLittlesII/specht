using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Manifests under which <c>specht upgrade</c> reads the embedded source (<c>0001-F7</c> B-005; outside B-030's condition):
/// version 0.2.0's shipped manifest recording no upstream schema source, once selecting no source and once with
/// <c>schemaSource</c> set to <c>embedded</c>.
/// </summary>
public sealed class UpgradeEmbeddedManifests : TheoryData<string>
{
    public UpgradeEmbeddedManifests()
    {
        var defined = JsonNode.Parse(
            UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        Add(defined.ToJsonString());

        var embedded = defined.DeepClone().AsObject();
        embedded["schemaSource"] = "embedded";
        Add(embedded.ToJsonString());
    }
}
