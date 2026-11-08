using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Tests.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>
/// A synthetic tree in an in-memory file system, read by the model reader and evaluated by the composed rules, in which a
/// Feature declares its own claim prefix (<c>0001-F5</c> B-004, B-007, B-036, B-037, B-049, B-076 to B-080; C-2, C-12;
/// decision 0022). From schema 0.3.1 (A-7) a Feature's prefix, read by the manifest's <c>keys.feature.claimPrefix</c>
/// name, selects its claim grammar: the literal prefix, three ASCII digits and an optional lowercase ASCII letter, in
/// place of <c>identifiers.claim</c>; its matrix and companion resolve that complete id, and the shipped Feature schema
/// checks the declaration. A Feature without a prefix, and every pin below 0.3.1, keep the manifest grammar. A finding is
/// read under the id the evaluator reports at the pin.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtModelReaderClaimPrefixUnitTests
{
    [Theory]
    [ClassData(typeof(PrefixedClaimIds))]
    public void ClaimIdsInAFeatureDeclaringAPrefix_WhenEvaluated_ShouldReportEveryIdThatIsNotTheLiteralPrefixThenThreeDigitsAndAnOptionalLowercaseLetter(
        string? grammar,
        bool tagIgnoreCase,
        string prefix,
        string[] claims,
        string[] reported)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.1",
            ["claim"] = new JsonObject { ["tagIgnoreCase"] = tagIgnoreCase },
        };
        if (grammar is not null)
        {
            manifest["identifiers"] = new JsonObject { ["claim"] = grammar };
        }

        // When
        var violations = Check(
            manifest,
            (Ci, $"---\nclaim_prefix: '{prefix}'\n---\n\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n"
                + string.Concat(claims.Select(static claim => $"| {claim} | It holds. |\n"))));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC4003" && violation.File == Ci)
            .Select(static violation => violation.Identifier)
            .Should()
            .Equal(reported);
    }

    [Fact]
    public void AClaimLackingTheDeclaredPrefix_WhenEvaluated_ShouldBeReportedBySpec4003AtItsDeclarationLineNamingTheAuthoredIdAndTheRequiredPrefix()
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.1" };

        // When
        var violations = Check(
            manifest,
            (Ci, "---\nclaim_prefix: CI-AC-\n---\n\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n"
                + "| CI-AC-001 | It holds. |\n| RELEASE-AC-001 | It holds. |\n"),
            (Release, "---\nclaim_prefix: RELEASE-AC-\n---\n\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n"
                + "| RELEASE-AC-001 | It holds. |\n"));

        // Then
        var finding = violations.Where(static violation => violation.RuleId == "SPEC4003").Should().ContainSingle().Subject;
        finding.File.Should().Be(Ci);
        finding.Line.Should().Be(10);
        finding.Identifier.Should().Be("RELEASE-AC-001");
        finding.Message.Should().Contain("'RELEASE-AC-001'").And.Contain("'CI-AC-'");
    }

    [Theory]
    [ClassData(typeof(ClaimPrefixKeys))]
    public void AClaimPrefixWrittenUnderAKey_WhenEvaluatedAtZeroThreeOne_ShouldBeReadOnlyUnderTheNameTheManifestGivesItsRole(
        string? role,
        string key,
        bool read)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.1" };
        if (role is not null)
        {
            manifest["keys"] = new JsonObject { ["feature"] = new JsonObject { ["claimPrefix"] = role } };
        }

        // When
        var violations = Check(
            manifest,
            (Ci, $"---\n{key}: CI-AC-\n---\n\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| B-001 | It holds. |\n"));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC4003")
            .Select(static violation => (violation.File, violation.Identifier))
            .Should()
            .Equal(read ? new (string, string?)[] { (Ci, "B-001") } : []);
    }

    [Theory]
    [ClassData(typeof(UnprefixedSiblings))]
    public void AFeatureWithoutAPrefixBesideOneDeclaringIt_WhenEvaluated_ShouldHaveItsClaimIdsCheckedByTheManifestGrammarAlone(
        string? grammar,
        string[] claims,
        string[] reported)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.1" };
        if (grammar is not null)
        {
            manifest["identifiers"] = new JsonObject { ["claim"] = grammar };
        }

        // When
        var violations = Check(
            manifest,
            (Ci, "---\nclaim_prefix: CI-AC-\n---\n\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| CI-AC-001 | It holds. |\n"),
            (Release, "---\nid: F2\n---\n\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n"
                + string.Concat(claims.Select(static claim => $"| {claim} | It holds. |\n"))));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC4003" && violation.File == Release)
            .Select(static violation => violation.Identifier)
            .Should()
            .Equal(reported);
    }

    [Theory]
    [ClassData(typeof(ClaimPrefixDeclarations))]
    public void AClaimPrefixDeclaration_WhenEvaluatedAtZeroThreeOne_ShouldBeReportedBySpec1002UnlessItIsTextWithANonWhiteSpaceCharacter(
        string value,
        bool rejected)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.1" };

        // When
        var violations = Check(manifest, (Ci, $"---\nclaim_prefix: {value}\n---\n"));

        // Then
        var findings = violations.Where(static violation => violation.RuleId == "SPEC1002" && violation.File == Ci
            && (violation.Identifier == "claim_prefix" || violation.Message.Contains("claim_prefix", StringComparison.Ordinal)));
        if (rejected)
        {
            findings.Should().Contain(static violation => violation.Identifier == "claim_prefix" && violation.Line == 2);
        }
        else
        {
            findings.Should().BeEmpty();
        }
    }

    [Fact]
    public void AMatrixCitingAPrefixedClaimAndAnUndeclaredId_WhenEvaluated_ShouldReportOnlyTheUndeclaredId()
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.1" };

        // When
        var violations = Check(
            manifest,
            (Ci, "---\nclaim_prefix: CI-AC-\n---\n\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| CI-AC-001a | It holds. |\n\n"
                + "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -- | -- | -- | -- |\n"
                + "| CI-AC-001a | It holds | Missing | Missing |\n| CI-AC-002 | It holds | Missing | Missing |\n"));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC4004")
            .Select(static violation => (violation.File, violation.Identifier))
            .Should()
            .Equal((Ci, "CI-AC-002"));
    }

    [Theory]
    [ClassData(typeof(PrefixedClaimTags))]
    public void CompanionTagsOfAFeatureDeclaringAPrefix_WhenEvaluated_ShouldResolveTheCompleteIdUnderTheTagFormAndCaseAndReportOnlyTheUnknownTag(
        string? grammar,
        string form,
        bool ignoreCase,
        string resolved,
        string unresolved,
        string identifier)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.1",
            ["claim"] = new JsonObject { ["tag"] = form, ["tagIgnoreCase"] = ignoreCase },
        };
        if (grammar is not null)
        {
            manifest["identifiers"] = new JsonObject { ["claim"] = grammar };
        }

        // When
        var violations = Check(
            manifest,
            (Ci, "---\nclaim_prefix: CI-AC-\n---\n\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| CI-AC-001a | It holds. |\n"),
            ("src/ci/.spec/ci.feature", $"Feature: ci\n\n  {resolved}\n  Scenario: held\n    Given it\n\n  {unresolved}\n  Scenario: unheld\n    Given it\n"));

        // Then
        var finding = violations.Where(static violation => violation.RuleId == "SPEC4002").Should().ContainSingle().Subject;
        finding.File.Should().Be("src/ci/.spec/ci.feature");
        finding.Line.Should().Be(7);
        finding.Identifier.Should().Be(identifier);
        finding.Message.Should().Contain($"scenario tag '{unresolved}'");
    }

    [Theory]
    [ClassData(typeof(PinsBeforeMint))]
    public void AFeatureWritingClaimPrefixUnderAPinBelowZeroThreeOne_WhenEvaluated_ShouldHaveItsClaimIdsCheckedByTheManifestGrammarAlone(string pin)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = pin };

        // When
        var violations = Check(
            manifest,
            (Ci, "---\nclaim_prefix: CI-AC-\n---\n\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n"
                + "| B-001 | It holds. |\n| CI-AC-001 | It holds. |\n"));

        // Then
        violations.Where(static violation => violation.RuleId is "SPEC030" or "SPEC4003")
            .Select(static violation => (violation.File, violation.Identifier))
            .Should()
            .Equal((Ci, "CI-AC-001"));
    }

    [Fact]
    public void AZeroThreeZeroManifestNamingAClaimPrefixRole_WhenEvaluated_ShouldGiveTheKeyItNamesNoPrefixMeaning()
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["keys"] = new JsonObject { ["feature"] = new JsonObject { ["claimPrefix"] = "criterion_prefix" } },
        };

        // When
        var violations = Check(
            manifest,
            (Ci, "---\ncriterion_prefix: CI-AC-\n---\n\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n"
                + "| B-001 | It holds. |\n| CI-AC-001 | It holds. |\n"));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC4003")
            .Select(static violation => (violation.File, violation.Identifier))
            .Should()
            .Equal((Ci, "CI-AC-001"));
    }

    private static IReadOnlyList<SpechtViolation> Check(JsonObject manifest, params (string Path, string Text)[] files)
    {
        var fileSystem = new MockFileSystem(
            files.Append((Path: SpechtTree.ManifestPath, Text: manifest.ToJsonString()))
                .ToDictionary(
                    static file => Path.Combine(["repo", .. file.Path.Split('/')]),
                    static file => new MockFileData(file.Text)));

        return new SpechtEvaluator(EngineServices.Rules(), new SpechtViolationOrder())
            .Evaluate(EngineServices.Reader(fileSystem).Read(fileSystem.Path.GetFullPath("repo"), null))
            .Violations;
    }

    private const string Ci = "src/ci/.spec/README.md";

    private const string Release = "src/release/.spec/README.md";
}
