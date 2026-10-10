using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace specht.tests;

/// <summary>
/// Builds a <see cref="SpecModel"/>: the root <c>repo</c>, no Feature, item or epic, and the schemas loaded from a
/// synthetic, empty schema set held in a <see cref="MockFileSystem"/>, until a test overrides what it asserts on. A test
/// of a rule under a manifest's own names hands that manifest to <see cref="WithManifest"/>.
/// <see cref="SpecSchemas"/> has no public constructor, so a schema set is loaded, not built.
/// </summary>
[AutoFixture(typeof(SpecModel))]
internal sealed partial class SpecModelFixture
{
    public SpecModelFixture() => WithRoot("repo").WithManifest(new JsonObject());

    /// <summary>Sets the schemas to those loaded under a synthetic manifest, each value it leaves out read as the default manifest's.</summary>
    /// <param name="manifest">The manifest.</param>
    /// <returns>The fixture.</returns>
    public SpecModelFixture WithManifest(JsonObject manifest) =>
        WithSchemas(
            SpecSchemas.Load(
                new MockFileSystem(
                    new Dictionary<string, MockFileData>
                    {
                        [Path.Combine("repo", ".spec", "schema", "spec-structure.schema.json")] = new(manifest.ToJsonString()),
                        [Path.Combine("repo", ".spec", "schema", "feature-spec.frontmatter.schema.json")] = new("{}"),
                        [Path.Combine("repo", ".spec", "schema", "task.frontmatter.schema.json")] = new("{}"),
                        [Path.Combine("repo", ".spec", "schema", "epic.frontmatter.schema.json")] = new("{}"),
                    }),
                "repo"));

    /// <summary>Sets the Feature specifications.</summary>
    /// <remarks>The generator names each list's setter <c>WithList</c>, after its type; this names it after the model's.</remarks>
    /// <param name="features">The Features.</param>
    /// <returns>The fixture.</returns>
    public SpecModelFixture WithFeatures(params IReadOnlyList<FeatureSpec> features) => WithList(features);
}
