using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>Claim-tag grammar, literal form, case and diagnostic mechanisms (0001-F5 B-007, B-036, B-037).</summary>
[Trait("Tier", "Unit")]
public sealed class FeatureFileRuleClaimTagsUnitTests
{
    [Theory]
    [InlineData("^B-[0-9]{4}$", "@{claim}", false, "B-0001", "@B-0001", "@B-0002", "B-0002")]
    [InlineData("^(?:B-[0-9]{3}|REQ-[0-9]{2})$", "@{claim}", false, "REQ-01", "@REQ-01", "@REQ-02", "REQ-02")]
    [InlineData("^B-[0-9]{3}$", "@req:{claim}", false, "B-001", "@req:B-001", "@req:B-002", "B-002")]
    [InlineData("^B-[0-9]{3}$", "@req.[{claim}]", false, "B-001", "@req.[B-001]", "@req.[B-002]", "B-002")]
    [InlineData("^B-[0-9]{3}$", "@req:{claim}", true, "B-001", "@REQ:b-001", "@REQ:b-002", "b-002")]
    public void ConfiguredClaimTags_WhenEvaluated_ShouldResolveTheDeclaredIdAndReportOnlyTheOriginalUnknownTag(
        string grammar, string form, bool ignoreCase, string claim, string resolved, string unresolved, string identifier)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["identifiers"] = new JsonObject { ["claim"] = grammar },
            ["claim"] = new JsonObject { ["claimTagForm"] = form, ["claimTagIgnoreCase"] = ignoreCase },
        };
        var companion = Path.Combine("repo", "src", "sample", ".spec", "sample.feature");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [companion] = new($"# {unresolved}\nFeature: sample\n  {resolved} {unresolved} @B-9999extra\n"),
        });
        FeatureSpec feature = new FeatureSpecFixture()
            .WithDocument(SpecDocument.Parse($"## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| {claim} | Declared. |\n",
                "src/sample/.spec/README.md"))
            .WithFeatureFiles(companion);
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(feature);

        // When
        var violations = new FeatureFileRule(new FeatureFileReader(fileSystem, new FeatureTagParser())).Evaluate(model).ToList();

        // Then
        var violation = violations.Should().ContainSingle().Subject;
        violation.RuleId.Should().Be("SPEC021");
        violation.Identifier.Should().Be(identifier);
        violation.Line.Should().Be(3);
        violation.File.Should().Be("src/sample/.spec/sample.feature");
        violation.Message.Should().Be($"scenario tag '{unresolved}' does not resolve to a claim in § 3 of src/sample/.spec/README.md");
    }

    [Theory]
    [InlineData("0.1.0")]
    [InlineData("0.1.1")]
    public void OlderPins_WhenEvaluated_ShouldKeepTheLegacyScannerDespiteADifferentClaimGrammar(string pin)
    {
        // Given
        var companion = Path.Combine("repo", "src", "sample", ".spec", "sample.feature");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [companion] = new("@B-0001 @B-002\n"),
        });
        FeatureSpec feature = new FeatureSpecFixture()
            .WithDocument(SpecDocument.Parse("## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| B-0001 | Declared. |\n",
                "src/sample/.spec/README.md"))
            .WithFeatureFiles(companion);
        SpecModel model = new SpecModelFixture().WithManifest(new JsonObject
        {
            ["schemaVersion"] = pin,
            ["identifiers"] = new JsonObject { ["claim"] = "^B-[0-9]{4}$" },
        }).WithFeatures(feature);

        // When
        var violations = new FeatureFileRule(new FeatureFileReader(fileSystem, new FeatureTagParser())).Evaluate(model).ToList();

        // Then
        violations.Should().ContainSingle().Which.Identifier.Should().Be("B-002");
    }

    [Theory]
    [InlineData("@req:{claim}", "@B-002")]
    [InlineData("@req:{claim}", "@REQ:B-002")]
    [InlineData("@{claim}", "@b-002")]
    [InlineData("@req.[{claim}]", "@reqX[B-002]")]
    [InlineData("@{claim}", "@B-002extra")]
    public void TextOutsideTheExactTagForm_WhenEvaluated_ShouldNotBecomeAClaimTag(string form, string tag)
    {
        // Given
        var companion = Path.Combine("repo", "src", "sample", ".spec", "sample.feature");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { [companion] = new(tag) });
        FeatureSpec feature = new FeatureSpecFixture().WithFeatureFiles(companion);
        SpecModel model = new SpecModelFixture().WithManifest(new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["claim"] = new JsonObject { ["claimTagForm"] = form },
        }).WithFeatures(feature);

        // When
        var violations = new FeatureFileRule(new FeatureFileReader(fileSystem, new FeatureTagParser())).Evaluate(model);

        // Then
        violations.Should().BeEmpty();
    }
}
