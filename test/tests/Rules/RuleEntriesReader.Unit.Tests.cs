using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Rules;

/// <summary>Pins the catalogue projection, page metadata, level precedence and ordering (0001-F10 B-001 to B-008).</summary>
[Trait("Tier", "Unit")]
public sealed class RuleEntriesReaderUnitTests
{
    [Theory]
    [ClassData(typeof(RuleCataloguePins))]
    public void AnExactPin_WhenRead_ShouldReturnOnlyThatVersionsVocabulary(string pin, string expectedId)
    {
        // Given
        SchemaVersion original = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(StringComparer.Ordinal) { "SPEC001" });
        original = original with { RulePages = new Dictionary<string, string>(StringComparer.Ordinal) { ["SPEC001"] = Page, ["SPEC999"] = Page } };
        SchemaVersion later = new SchemaVersionFixture().WithNumber(new SemanticVersion(0, 2, 0))
            .WithRuleIds(new HashSet<string>(StringComparer.Ordinal) { "SPEC010" });
        later = later with { RulePages = new Dictionary<string, string>(StringComparer.Ordinal) { ["SPEC010"] = Page } };
        var versions = new SchemaVersions([original, later]);
        var parser = new SpecManifestParser(versions);
        var reader = new RuleEntriesReader(EngineServices.Schemas(new MockFileSystem(), versions), parser);
        var structure = parser.Parse($$"""{"schemaVersion":"{{pin}}"}""");

        // When
        var entries = reader.Read(structure);

        // Then
        entries.Select(static entry => entry.Id).Should().Equal(expectedId);
    }

    [Theory]
    [ClassData(typeof(RulePageMetadata))]
    public void APageMetadataValue_WhenRead_ShouldSupplyTheFamilyAndDefaultSeverity(string family, SpecRuleLevel severity)
    {
        // Given
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(StringComparer.Ordinal) { "SPEC010" });
        version = version with
        {
            RulePages = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["SPEC010"] = $$"""
                    ## Metadata

                    | Property | Value |
                    | -------- | ----- |
                    | Family | {{family}} |
                    | Default severity | {{severity}} |
                    """,
            },
        };
        var versions = new SchemaVersions([version]);
        var parser = new SpecManifestParser(versions);
        var reader = new RuleEntriesReader(EngineServices.Schemas(new MockFileSystem(), versions), parser);
        var structure = parser.Parse("{}");

        // When
        var entries = reader.Read(structure);

        // Then
        entries.Should().ContainSingle().Which.Should().BeEquivalentTo(new { Family = family, DefaultSeverity = severity });
    }

    [Theory]
    [ClassData(typeof(RuleCatalogueLevels))]
    public void AnExplicitLevel_WhenRead_ShouldOverrideThePageDefaultWithoutRemovingTheEntry(SpecRuleLevel level)
    {
        // Given
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(StringComparer.Ordinal) { "SPEC010" });
        version = version with { RulePages = new Dictionary<string, string>(StringComparer.Ordinal) { ["SPEC010"] = Page } };
        var versions = new SchemaVersions([version]);
        var parser = new SpecManifestParser(versions);
        var reader = new RuleEntriesReader(EngineServices.Schemas(new MockFileSystem(), versions), parser);
        var structure = parser.Parse("{\"rules\":{\"SPEC010\":\"" + level.ToString().ToLowerInvariant() + "\"}}");

        // When
        var entries = reader.Read(structure);

        // Then
        entries.Should().ContainSingle().Which.EffectiveLevel.Should().Be(level);
    }

    [Theory]
    [ClassData(typeof(RuleCatalogueLevels))]
    public void NoExplicitLevel_WhenRead_ShouldUseTheRulesOwnSeverity(SpecRuleLevel severity)
    {
        // Given
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(StringComparer.Ordinal) { "SPEC010" });
        version = version with
        {
            RulePages = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["SPEC010"] = $$"""
                    ## Metadata

                    | Property | Value |
                    | -------- | ----- |
                    | Family | Fixture family |
                    | Default severity | {{severity}} |
                    """,
            },
        };
        var versions = new SchemaVersions([version]);
        var parser = new SpecManifestParser(versions);
        var reader = new RuleEntriesReader(EngineServices.Schemas(new MockFileSystem(), versions), parser);
        var structure = parser.Parse("{}");

        // When
        var entries = reader.Read(structure);

        // Then
        entries.Should().ContainSingle().Which.EffectiveLevel.Should().Be(severity);
    }

    [Fact]
    public void AnUnorderedVocabulary_WhenRead_ShouldReturnEntriesInOrdinalIdOrder()
    {
        // Given
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(StringComparer.Ordinal)
        {
            "SPEC020", "SPEC010", "SPEC001",
        });
        version = version with { RulePages = version.RuleIds.ToDictionary(static id => id, static _ => Page, StringComparer.Ordinal) };
        var versions = new SchemaVersions([version]);
        var parser = new SpecManifestParser(versions);
        var reader = new RuleEntriesReader(EngineServices.Schemas(new MockFileSystem(), versions), parser);
        var structure = parser.Parse("{}");

        // When
        var entries = reader.Read(structure);

        // Then
        entries.Select(static entry => entry.Id).Should().Equal("SPEC001", "SPEC010", "SPEC020");
    }

    private const string Page = "## Metadata\n\n| Property | Value |\n| -------- | ----- |\n| Family | Fixture family |\n| Default severity | Warning |\n";
}
