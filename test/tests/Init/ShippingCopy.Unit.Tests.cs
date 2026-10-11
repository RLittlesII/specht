using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Tool;

namespace Specht.Tests.Init;

/// <summary>
/// The shipping copy embedded in the tool (0001-F4 A-1, A-2, B-005): the newest version holds the eight files, and each
/// frontmatter schema it embeds names this repository and that version in its <c>$id</c>.
/// </summary>
[Trait("Tier", "Unit")]
public sealed partial class ShippingCopyUnitTests
{
    public static TheoryData<string> FrontmatterSchemas { get; } = new()
    {
        "feature-spec.frontmatter.schema.json",
        "task.frontmatter.schema.json",
        "epic.frontmatter.schema.json",
    };

    [Theory]
    [MemberData(nameof(FrontmatterSchemas))]
    public void TheNewestEmbeddedFrontmatterSchema_WhenRead_ShouldCarryAnIdUnderThisRepositorysSchemaAddressForItsVersion(string file)
    {
        // Given
        var version = NewestVersion();
        var name = $"schema/v{version}/{file}";

        // When
        using var stream = Tool.GetManifestResourceStream(name);

        // Then
        stream.Should().NotBeNull($"the tool embeds {name}");
        JsonNode.Parse(stream!)!["$id"]!.GetValue<string>()
            .Should()
            .Be($"https://github.com/rlittlesii/specht/schema/v{version}/{file}");
    }

    [Fact]
    public void TheNewestEmbeddedVersion_WhenListed_ShouldHoldExactlyTheEightFiles()
    {
        // Given
        var version = NewestVersion();

        // When
        var files = Tool.GetManifestResourceNames()
            .Where(name => name.StartsWith($"schema/v{version}/", StringComparison.Ordinal)
                           || name.StartsWith($"templates/v{version}/", StringComparison.Ordinal))
            .ToList();

        // Then
        files.Should()
            .BeEquivalentTo(
                $"schema/v{version}/spec-structure.schema.json",
                $"schema/v{version}/feature-spec.frontmatter.schema.json",
                $"schema/v{version}/task.frontmatter.schema.json",
                $"schema/v{version}/epic.frontmatter.schema.json",
                $"templates/v{version}/feature.md",
                $"templates/v{version}/decision.md",
                $"templates/v{version}/adr.md",
                $"templates/v{version}/lesson.md");
    }

    private static Assembly Tool => typeof(ExitCodes).Assembly;

    private static int NewestVersion()
    {
        var versions = Tool.GetManifestResourceNames()
            .Select(static name => SchemaVersion().Match(name))
            .Where(static match => match.Success)
            .Select(static match => int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture))
            .ToList();
        versions.Should().NotBeEmpty("the tool embeds at least one schema/v<n>/ resource");
        return versions.Max();
    }

    [GeneratedRegex("^schema/v([0-9]+)/")]
    private static partial Regex SchemaVersion();
}
