using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>
/// The tests, matrix, owners and required-specification keys and the five status markers at schema 0.2.0, read in memory
/// over the shipped catalog (0001-F5 B-055 to B-060).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecManifestParserMatrixKeysUnitTests
{
    public static TheoryData<string, string[]> TestsSettings => new()
    {
        { """{"files":["test/**/*.cs"],"declarations":["void {test}("]}""", ["void {test}("] },
        { """{"files":["test/**/*.cs"]}""", [] },
    };

    public static TheoryData<string> DeclarationFormsWithoutExactlyOneTestPlaceholder => new()
    {
        "void Test(",
        "{test} and {test}",
        "{test}{test}{test}",
    };

    public static TheoryData<string?, bool> ScenarioTitles => new()
    {
        { null, false },
        { "{}", false },
        { """{"scenarioTitles":false}""", false },
        { """{"scenarioTitles":true}""", true },
    };

    public static TheoryData<string, string> StatusMarkers => new()
    {
        { "covered", "Covered" },
        { "partial", "Partial" },
        { "waived", "Waived" },
        { "withdrawn", "Withdrawn" },
        { "superseded", "Superseded" },
    };

    public static TheoryData<string, string[]> OwnersTheSectionsDoNotBearOut => new()
    {
        { "3. Claims", ["spec-author"] },
        { "3. Acceptance Criteria", [] },
    };

    public static TheoryData<string?, string[]> RequiredSpecifications => new()
    {
        { null, [] },
        { """["src/*","tools/**"]""", ["src/*", "tools/**"] },
    };

    [Theory]
    [MemberData(nameof(TestsSettings))]
    public void ATestsSettingWithOrWithoutDeclarations_WhenParsedAtSchema020_ShouldReadItsFilesAndDeclarations(
        string tests,
        string[] declarations)
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0", ["tests"] = JsonNode.Parse(tests) };

        // When
        var structure = parser.Parse(manifest.ToJsonString());

        // Then
        structure.Tests.Should().NotBeNull();
        structure.Tests!.Files.Should().Equal("test/**/*.cs");
        structure.Tests.Declarations.Should().Equal(declarations);
    }

    [Fact]
    public void ATestsSetting_WhenParsedAtSchema010_ShouldBeRejectedAsAnUnknownKey()
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.1.0",
            ["tests"] = new JsonObject { ["files"] = new JsonArray("test/**/*.cs") },
        };

        // When
        var parse = () => parser.Parse(manifest.ToJsonString());

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain("does not know the key 'tests'");
    }

    [Theory]
    [MemberData(nameof(DeclarationFormsWithoutExactlyOneTestPlaceholder))]
    public void ADeclarationFormWithoutExactlyOneTestPlaceholder_WhenParsedAtSchema020_ShouldBeRejectedNamingTheEntry(string form)
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["tests"] = new JsonObject
            {
                ["files"] = new JsonArray("test/**/*.cs"),
                ["declarations"] = new JsonArray("void {test}(", form),
            },
        };

        // When
        var parse = () => parser.Parse(manifest.ToJsonString());

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain(form).And.NotContain("does not know the key");
    }

    [Theory]
    [MemberData(nameof(ScenarioTitles))]
    public void TheScenarioTitlesSetting_WhenParsedAtSchema020_ShouldReadAsDeclaredAndFalseWhenLeftOut(string? matrix, bool expected)
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0" };
        if (matrix is not null)
        {
            manifest["matrix"] = JsonNode.Parse(matrix);
        }

        // When
        var structure = parser.Parse(manifest.ToJsonString());

        // Then
        structure.MatrixScenarioTitles.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(StatusMarkers))]
    public void AStatusMarkerLeftOut_WhenParsedAtSchema020_ShouldReadAsItsDefault(string marker, string text)
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0", ["markers"] = new JsonObject { ["missing"] = "TBD" } };

        // When
        var structure = parser.Parse(manifest.ToJsonString());

        // Then
        structure.Markers[marker].Should().Be(text);
    }

    [Fact]
    public void AStatusMarkerTheManifestNames_WhenParsedAtSchema020_ShouldReplaceThatDefaultAlone()
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0", ["markers"] = new JsonObject { ["covered"] = "Done" } };

        // When
        var structure = parser.Parse(manifest.ToJsonString());

        // Then
        structure.Markers["covered"].Should().Be("Done");
        structure.Markers["partial"].Should().Be("Partial");
        structure.Markers["waived"].Should().Be("Waived");
        structure.Markers["withdrawn"].Should().Be("Withdrawn");
        structure.Markers["superseded"].Should().Be("Superseded");
    }

    [Fact]
    public void OwnersNamingListedSections_WhenParsedAtSchema020_ShouldReadEachTitlesRolesAsWritten()
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["owners"] = new JsonObject
            {
                ["3. Acceptance Criteria"] = new JsonArray("spec-author"),
                ["9. Traceability Matrix"] = new JsonArray("test-writer", "spec-reviewer"),
            },
        };

        // When
        var structure = parser.Parse(manifest.ToJsonString());

        // Then
        structure.Owners.Should().HaveCount(2);
        structure.Owners["3. Acceptance Criteria"].Should().Equal("spec-author");
        structure.Owners["9. Traceability Matrix"].Should().Equal("test-writer", "spec-reviewer");
    }

    [Theory]
    [MemberData(nameof(OwnersTheSectionsDoNotBearOut))]
    public void AnOwnersEntryTheSectionsDoNotBearOut_WhenParsedAtSchema020_ShouldBeRejectedNamingTheTitle(string title, string[] roles)
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["owners"] = new JsonObject { [title] = new JsonArray(roles.Select(static role => (JsonNode)JsonValue.Create(role)).ToArray()) },
        };

        // When
        var parse = () => parser.Parse(manifest.ToJsonString());

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain(title).And.NotContain("does not know the key");
    }

    [Fact]
    public void OwnersLeftOut_WhenParsedAtSchema020_ShouldGiveNoSectionAnOwner()
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0" };

        // When
        var structure = parser.Parse(manifest.ToJsonString());

        // Then
        structure.Owners.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(RequiredSpecifications))]
    public void TheRequiredSpecificationsSetting_WhenParsedAtSchema020_ShouldReadItsGlobsAndAnEmptyListWhenLeftOut(
        string? required,
        string[] expected)
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0" };
        if (required is not null)
        {
            manifest["requiredSpecifications"] = JsonNode.Parse(required);
        }

        // When
        var structure = parser.Parse(manifest.ToJsonString());

        // Then
        structure.RequiredSpecifications.Should().Equal(expected);
    }
}
