using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Model;

namespace Specht.Tests.Shared;

/// <summary>
/// Builds a <see cref="SpechtModel"/>: the root <c>repo</c>, no Feature, item or epic, and the schemas loaded from a
/// synthetic, empty schema set held in a <see cref="MockFileSystem"/>, until a test overrides what it asserts on. A test
/// of a rule under a manifest's own names hands that manifest to <see cref="WithManifest"/>.
/// <see cref="SpechtSchemas"/> has no public constructor, so a schema set is loaded, not built.
/// </summary>
[AutoFixture(typeof(SpechtModel))]
internal sealed partial class SpechtModelFixture
{
    public SpechtModelFixture() => WithRoot("repo").WithManifest(new JsonObject());

    /// <summary>Sets the schemas to those loaded under a synthetic manifest, each value it leaves out read as the default manifest's.</summary>
    /// <param name="manifest">The manifest.</param>
    /// <returns>The fixture.</returns>
    public SpechtModelFixture WithManifest(JsonObject manifest) =>
        WithSchemas(
            EngineServices.LoadOnDiskSchemas(new MockFileSystem(
                    new Dictionary<string, MockFileData>
                    {
                        [Path.Combine("repo", SpechtTree.ManifestPath)] = new(manifest.ToJsonString()),
                        [Path.Combine("repo", SpechtTree.FeatureSchemaPath)] = new("{}"),
                        [Path.Combine("repo", SpechtTree.ItemSchemaPath)] = new("{}"),
                        [Path.Combine("repo", SpechtTree.EpicSchemaPath)] = new("{}"),
                    }), "repo"));

    /// <summary>Sets the Feature specifications.</summary>
    /// <remarks>The generator names each list's setter <c>WithList</c>, after its type; this names it after the model's.</remarks>
    /// <param name="features">The Features.</param>
    /// <returns>The fixture.</returns>
    public SpechtModelFixture WithFeatures(params IReadOnlyList<FeatureSpec> features) => WithList(features);

    /// <summary>Sets the items beside the specifications.</summary>
    /// <param name="items">The items.</param>
    /// <returns>The fixture.</returns>
    public SpechtModelFixture WithItems(params IReadOnlyList<ChildItem> items) => WithList(items);
}
