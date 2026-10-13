using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Tool;
using Specht.Versioning;

namespace Specht.Tests.Init;

/// <summary>
/// The shipping copy embedded in the tool (0001-F4 A-1, A-2, B-005): the newest version holds the eight files - the
/// manifest, the three frontmatter schemas in their <c>frontmatter/</c> folder and the four templates - and each frontmatter
/// schema it embeds names this repository, that version, the <c>frontmatter</c> folder and its own file name in its
/// <c>$id</c> (<c>0001-F5</c> decision 0011).
/// </summary>
[Trait("Tier", "Unit")]
public sealed partial class ShippingCopyUnitTests
{
    public static TheoryData<string> FrontmatterSchemas { get; } = new()
    {
        "feature.schema.json",
        "task.schema.json",
        "epic.schema.json",
    };

    [Theory]
    [MemberData(nameof(FrontmatterSchemas))]
    public void TheNewestEmbeddedFrontmatterSchema_WhenRead_ShouldCarryAnIdUnderThisRepositorysSchemaAddressForItsVersion(string file)
    {
        // Given
        var version = NewestVersion();
        var name = $"schema/v{version}/frontmatter/{file}";

        // When
        using var stream = Tool.GetManifestResourceStream(name);

        // Then
        stream.Should().NotBeNull($"the tool embeds {name}");
        JsonNode.Parse(stream!)!["$id"]!.GetValue<string>()
            .Should()
            .Be($"https://github.com/rlittlesii/specht/schema/v{version}/frontmatter/{file}");
    }

    [Fact]
    public void TheNewestEmbeddedVersion_WhenListed_ShouldHoldExactlyTheEightFiles()
    {
        // Given
        var version = NewestVersion();

        // When
        var files = Tool.GetManifestResourceNames()
            .Where(name => name.StartsWith($"manifest/v{version}/", StringComparison.Ordinal)
                           || name.StartsWith($"schema/v{version}/", StringComparison.Ordinal)
                           || name.StartsWith($"templates/v{version}/", StringComparison.Ordinal))
            .ToList();

        // Then
        files.Should()
            .BeEquivalentTo(
                $"manifest/v{version}/manifest.json",
                $"schema/v{version}/frontmatter/feature.schema.json",
                $"schema/v{version}/frontmatter/task.schema.json",
                $"schema/v{version}/frontmatter/epic.schema.json",
                $"templates/v{version}/feature.md",
                $"templates/v{version}/decision.md",
                $"templates/v{version}/adr.md",
                $"templates/v{version}/lesson.md");
    }

    private static Assembly Tool => typeof(ExitCodes).Assembly;

    private static SemanticVersion NewestVersion()
    {
        var versions = Tool.GetManifestResourceNames()
            .Select(static name => SchemaVersion().Match(name))
            .Where(static match => match.Success)
            .Select(static match => SemanticVersion.TryParse(match.Groups[1].Value, out var version)
                ? version
                : throw new InvalidOperationException("Invalid version folder."))
            .ToList();
        versions.Should().NotBeEmpty("the tool embeds at least one schema/v<major.minor.patch>/ resource");
        return versions.Max();
    }

    [GeneratedRegex("^schema/v([0-9]+\\.[0-9]+\\.[0-9]+)/")]
    private static partial Regex SchemaVersion();
}
