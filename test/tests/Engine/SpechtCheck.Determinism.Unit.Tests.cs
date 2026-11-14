using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Engine;

/// <summary>
/// Nothing about the machine reaches the verdict (<c>0001-F1</c> B-006, decision 0006), in memory: the frontmatter rule's
/// scoped schema evaluation gives the invariant message under another culture and leaves the caller's culture in place.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtCheckDeterminismUnitTests
{
    [Fact]
    public void AFrontmatterValueOfTheWrongType_WhenTheSchemaRuleEvaluatesItUnderTheTurkishCulture_ShouldNameTheExpectedTypeInvariantly()
    {
        // Given
        var model = ModelExpectingAnInteger();
        var caller = CultureInfo.CurrentCulture;
        List<SpechtFinding> findings;

        // When
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            findings = [.. new FeatureFrontmatterRule(new FrontmatterSchemaCheck()).Evaluate(model)];
        }
        finally
        {
            CultureInfo.CurrentCulture = caller;
        }

        // Then
        findings.Should().ContainSingle(static finding => finding.Identifier == "github_issue")
            .Which.Message.Should().EndWith("should be \"integer\"");
    }

    [Fact]
    public void AFrontmatterSchemaEvaluation_WhenTheCallerRunsUnderTheTurkishCulture_ShouldLeaveTheCallersCultureInPlace()
    {
        // Given
        var model = ModelExpectingAnInteger();
        var caller = CultureInfo.CurrentCulture;
        CultureInfo after;

        // When
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            _ = new FeatureFrontmatterRule(new FrontmatterSchemaCheck()).Evaluate(model).ToList();
            after = CultureInfo.CurrentCulture;
        }
        finally
        {
            CultureInfo.CurrentCulture = caller;
        }

        // Then
        after.Name.Should().Be("tr-TR");
    }

    private static SpechtModel ModelExpectingAnInteger()
    {
        var schemas = EngineServices.ReadDiskSchemas(new MockFileSystem(
                new Dictionary<string, MockFileData>
                {
                    [Path.Combine("repo", SpechtTree.ManifestPath)] = new("{}"),
                    [Path.Combine("repo", SpechtTree.FeatureSchemaPath)] =
                        new("""{ "properties": { "github_issue": { "type": "integer" } } }"""),
                    [Path.Combine("repo", SpechtTree.ItemSchemaPath)] = new("{}"),
                    [Path.Combine("repo", SpechtTree.EpicSchemaPath)] = new("{}"),
                }), "repo");
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            new SpechtDocumentFixture().WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["github_issue"] = "null" })));

        return new SpechtModelFixture().WithSchemas(schemas).WithFeatures(feature);
    }
}
