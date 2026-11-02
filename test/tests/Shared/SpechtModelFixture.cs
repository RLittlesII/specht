using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Model;

namespace Specht.Tests.Shared;

/// <summary>
/// Builds a <see cref="SpechtModel"/>: no Feature, item or epic, and the schemas read from a synthetic, empty schema set
/// under <c>repo</c> in a <see cref="MockFileSystem"/>, until a test overrides what it asserts on. A test of a rule under a
/// manifest's own names hands that manifest to <see cref="WithManifest"/>.
/// The schema set is read through the model reader, as a check reads one; <see cref="SpechtSchemaCompiler"/> builds one in
/// memory, with no file system, where a test needs no manifest.
/// </summary>
[AutoFixture(typeof(SpechtModel))]
internal sealed partial class SpechtModelFixture
{
    public SpechtModelFixture() => WithManifest(new JsonObject());

    /// <summary>Sets the schemas to those read under a synthetic manifest, each value it leaves out read as the default manifest's.</summary>
    /// <param name="manifest">The manifest.</param>
    /// <returns>The fixture.</returns>
    public SpechtModelFixture WithManifest(JsonObject manifest) =>
        WithSchemas(
            EngineServices.ReadDiskSchemas(new MockFileSystem(
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
