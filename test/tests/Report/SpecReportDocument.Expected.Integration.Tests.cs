using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// What a frontmatter violation's <c>expected</c> names under each schema source, over a run on a tree on disk
/// (<c>0001-F3</c> B-009, B-026; decision 0004): the Feature schema file by its path under the configuration folder,
/// whichever source the run read - the embedded schemas, the files on disk, or the upstream copy the manifest records -
/// with the required keys or the failed assertion of the schema it read. The tree's schema files are the tool's own
/// (<see cref="SpecTree"/>), so each source gives the same values.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpecReportDocumentExpectedIntegrationTests
{
    [Theory]
    [ClassData(typeof(SchemaSources))]
    public void AFrontmatterViolation_WhenItsDocumentIsMadeUnderEachSchemaSource_ShouldNameTheFeatureSchemaByItsPathUnderTheConfigurationFolder(
        string because,
        Action<SpecTree> write,
        string ruleId,
        string? key,
        JsonObject members,
        Action<SpecTree> arrange,
        SchemaSourceSelection? selection)
    {
        // Given
        using var tree = new SpecTree();
        write(tree);
        arrange(tree);

        // When
        var document = SpecReportDocument.From(EngineServices.Runner().Run(tree.Root, selection));

        // Then
        var expected = document.Violations.Should()
            .ContainSingle(violation => violation.RuleId == ruleId && violation.Identifier == key, because).Which.Expected;
        members.Should().AllSatisfy(member => JsonNode.DeepEquals(expected[member.Key], member.Value).Should().BeTrue(
            "{0}: {1} should be {2}, but expected is {3}", because, member.Key, member.Value?.ToJsonString(), expected.ToJsonString()));
    }
}
