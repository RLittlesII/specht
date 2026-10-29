using System.IO.Abstractions;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class SchemaVersionNumberingIntegrationTests
{
    [Fact]
    public async Task TheRepositorysShippedVersions_WhenTheirNumbersAreVerified_ShouldReportNoFinding()
    {
        // Given
        var root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "specht.slnx")))
        {
            root = Path.GetDirectoryName(root.TrimEnd(Path.DirectorySeparatorChar))
                ?? throw new DirectoryNotFoundException("No repository root above the test assembly.");
        }

        var numbering = new SchemaVersionNumbering(new FileSystem(), root);

        // When
        var findings = await numbering.VerifyAsync();

        // Then
        findings.Should().BeEmpty();
    }
}
