using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Unit")]
public sealed class SchemaVersionNumberingUnitTests
{
    [Theory]
    [InlineData("2.0.0", false)]
    [InlineData("1.1.0", true)]
    [InlineData("1.0.1", true)]
    public async Task ANewRuleIdFromOneZeroZeroOn_WhenTheNumbersAreVerified_ShouldBeReportedUnlessTheMajorIsHigher(string higher, bool reported)
    {
        // Given
        var fileSystem = new MockFileSystem();
        foreach (var version in new[] { "1.0.0", higher })
        {
            fileSystem.AddFile(
                $"repo/src/tool/schema/v{version}/frontmatter/feature.schema.json",
                new MockFileData($"{{\n  \"$id\": \"https://github.com/rlittlesii/specht/schema/v{version}/frontmatter/feature.schema.json\"\n}}\n"));
            fileSystem.AddFile($"repo/src/tool/manifest/v{version}/manifest.json", new MockFileData($"{{\n  \"schemaVersion\": \"{version}\"\n}}\n"));
            fileSystem.AddFile($"repo/src/tool/templates/v{version}/feature.md", new MockFileData("template\n"));
            fileSystem.AddFile($"repo/docs/rules/v{version}/SPEC001.md", new MockFileData($"# SPEC001\n\n| Schema version | {version} |\n"));
        }

        fileSystem.AddFile($"repo/docs/rules/v{higher}/SPEC002.md", new MockFileData($"# SPEC002\n\n| Schema version | {higher} |\n"));

        // When
        var findings = await new SchemaVersionNumbering(fileSystem, "repo").VerifyAsync();

        // Then
        findings.Should().Equal(
            reported ? [new SchemaVersionNumberingFinding("1.0.0", higher, SchemaVersionNumberingFault.NewRuleWithoutHigherMajor)] : []);
    }

    [Theory]
    [InlineData("0.2.0", false)]
    [InlineData("0.1.1", true)]
    public async Task ANewRuleIdBelowOneZeroZero_WhenTheNumbersAreVerified_ShouldBeReportedUnlessTheMinorIsHigher(string higher, bool reported)
    {
        // Given
        var fileSystem = new MockFileSystem();
        foreach (var version in new[] { "0.1.0", higher })
        {
            fileSystem.AddFile(
                $"repo/src/tool/schema/v{version}/frontmatter/feature.schema.json",
                new MockFileData($"{{\n  \"$id\": \"https://github.com/rlittlesii/specht/schema/v{version}/frontmatter/feature.schema.json\"\n}}\n"));
            fileSystem.AddFile($"repo/src/tool/manifest/v{version}/manifest.json", new MockFileData($"{{\n  \"schemaVersion\": \"{version}\"\n}}\n"));
            fileSystem.AddFile($"repo/src/tool/templates/v{version}/feature.md", new MockFileData("template\n"));
            fileSystem.AddFile($"repo/docs/rules/v{version}/SPEC001.md", new MockFileData($"# SPEC001\n\n| Schema version | {version} |\n"));
        }

        fileSystem.AddFile($"repo/docs/rules/v{higher}/SPEC002.md", new MockFileData($"# SPEC002\n\n| Schema version | {higher} |\n"));

        // When
        var findings = await new SchemaVersionNumbering(fileSystem, "repo").VerifyAsync();

        // Then
        findings.Should().Equal(
            reported ? [new SchemaVersionNumberingFinding("0.1.0", higher, SchemaVersionNumberingFault.NewRuleWithoutHigherMinor)] : []);
    }

    [Theory]
    [InlineData("src/tool/templates/v{0}/feature.md", "0.1.1", false)]
    [InlineData("src/tool/templates/v{0}/feature.md", "0.2.0", true)]
    [InlineData("docs/rules/v{0}/SPEC001.md", "0.1.1", false)]
    [InlineData("docs/rules/v{0}/SPEC001.md", "1.0.0", true)]
    public async Task NeighboursDifferingOnlyInATemplateOrARulePagesText_WhenTheNumbersAreVerified_ShouldBeReportedUnlessOnlyThePatchIsHigher(
        string changed, string higher, bool reported)
    {
        // Given
        var fileSystem = new MockFileSystem();
        foreach (var version in new[] { "0.1.0", higher })
        {
            fileSystem.AddFile(
                $"repo/src/tool/schema/v{version}/frontmatter/feature.schema.json",
                new MockFileData($"{{\n  \"$id\": \"https://github.com/rlittlesii/specht/schema/v{version}/frontmatter/feature.schema.json\"\n}}\n"));
            fileSystem.AddFile($"repo/src/tool/manifest/v{version}/manifest.json", new MockFileData($"{{\n  \"schemaVersion\": \"{version}\"\n}}\n"));
            fileSystem.AddFile($"repo/src/tool/templates/v{version}/feature.md", new MockFileData("template\n"));
            fileSystem.AddFile($"repo/docs/rules/v{version}/SPEC001.md", new MockFileData($"# SPEC001\n\n| Schema version | {version} |\n"));
        }

        var path = $"repo/{string.Format(CultureInfo.InvariantCulture, changed, higher)}";
        fileSystem.AddFile(path, new MockFileData(fileSystem.File.ReadAllText(path) + "Changed text.\n"));

        // When
        var findings = await new SchemaVersionNumbering(fileSystem, "repo").VerifyAsync();

        // Then
        findings.Should().Equal(
            reported ? [new SchemaVersionNumberingFinding("0.1.0", higher, SchemaVersionNumberingFault.TemplateOrRulePageChangeNotAPatch)] : []);
    }

    [Theory]
    [InlineData("src/tool/schema/v{0}/frontmatter/feature.schema.json")]
    [InlineData("src/tool/manifest/v{0}/manifest.json")]
    public async Task NeighboursDifferingInATemplateAndBeyondTheVersionInASchemaOrTheManifest_WhenTheNumbersAreVerified_ShouldReportNothingOnAMinorStep(
        string alsoChanged)
    {
        // Given
        var fileSystem = new MockFileSystem();
        foreach (var version in new[] { "0.1.0", "0.2.0" })
        {
            fileSystem.AddFile(
                $"repo/src/tool/schema/v{version}/frontmatter/feature.schema.json",
                new MockFileData($"{{\n  \"$id\": \"https://github.com/rlittlesii/specht/schema/v{version}/frontmatter/feature.schema.json\"\n}}\n"));
            fileSystem.AddFile($"repo/src/tool/manifest/v{version}/manifest.json", new MockFileData($"{{\n  \"schemaVersion\": \"{version}\"\n}}\n"));
            fileSystem.AddFile($"repo/src/tool/templates/v{version}/feature.md", new MockFileData("template\n"));
            fileSystem.AddFile($"repo/docs/rules/v{version}/SPEC001.md", new MockFileData($"# SPEC001\n\n| Schema version | {version} |\n"));
        }

        fileSystem.AddFile("repo/src/tool/templates/v0.2.0/feature.md", new MockFileData("template\nChanged text.\n"));
        var path = $"repo/{string.Format(CultureInfo.InvariantCulture, alsoChanged, "0.2.0")}";
        fileSystem.AddFile(path, new MockFileData(fileSystem.File.ReadAllText(path) + "Changed text.\n"));

        // When
        var findings = await new SchemaVersionNumbering(fileSystem, "repo").VerifyAsync();

        // Then
        findings.Should().BeEmpty();
    }
}
