using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Versioning;
using Spectre.Console.Testing;

namespace Specht.Tests.FrontmatterCommand;

/// <summary>
/// <c>specht frontmatter</c> stays on the pin under a floating pin policy, like the commands <c>0001-F7</c> OQ-73 keeps there
/// (decision 0021): over a set holding the embedded <c>0.3.0</c> with a task schema accepting any document and a <c>0.3.1</c>
/// copied from it whose task schema requires a key the document lacks, a root pinning <c>0.3.0</c> with <c>latest-patch</c>
/// validates the document with the pinned version's schema, so it passes where the version in use would fail it.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class FrontmatterCommandPinPolicyIntegrationTests
{
    [Fact]
    public async Task ARootPinnedWithLatestPatchBesideAPatchWhoseSchemaDiffers_WhenADocumentIsChecked_ShouldValidateWithThePinnedVersionsSchema()
    {
        // Given
        var pinned = EngineServices.Versions().Find(new SemanticVersion(0, 3, 0))! with { ItemSchema = "{}" };
        var patch = pinned with
        {
            Number = new SemanticVersion(0, 3, 1),
            ItemSchema = """{"type":"object","required":["in_use_only"]}""",
        };
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/.specht/manifest.json"] = new("""{"schemaVersion":"0.3.0","pinPolicy":"latest-patch"}"""),
            ["drafts/issue.md"] = new("---\ntitle: valid\n---\n"),
        });
        using var input = new StringReader(string.Empty);
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var app = EngineServices.FrontmatterApp(files, new TestConsole(), input, errors, new SchemaVersions([pinned, patch]));

        // When
        var result = await app.RunAsync(
            ["frontmatter", "--kind", "task", "drafts/issue.md", "--root", "repo"], TestContext.Current.CancellationToken);

        // Then
        result.ExitCode.Should().Be(0, errors.ToString() + result.Output);
        result.Output.TrimEnd().Should().Be("errors: 0, warnings: 0");
        errors.ToString().Should().BeEmpty();
    }
}
