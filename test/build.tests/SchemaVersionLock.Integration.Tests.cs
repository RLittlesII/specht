using System.IO.Abstractions;
using System.Text.Json;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class SchemaVersionLockIntegrationTests
{
    [Fact]
    public async Task NoLockFile_WhenTheNewestVersionIsLocked_ShouldCreateItsDigestAndLeaveTheVersionFilesAlone()
    {
        // Given
        var root = Path.Combine(".artifacts", "schema-lock-tests", Guid.NewGuid().ToString("N"));
        var schema = Path.Combine(root, "src/tool/schema/v0.1.0/frontmatter/feature.schema.json");
        Directory.CreateDirectory(Path.GetDirectoryName(schema)!);
        await File.WriteAllTextAsync(schema, "synthetic schema\r\n", TestContext.Current.CancellationToken);
        var before = await File.ReadAllBytesAsync(schema, TestContext.Current.CancellationToken);
        var versionLock = new SchemaVersionLock(new FileSystem(), root);
        try
        {
            // When
            await versionLock.LockNewestAsync();

            // Then
            var lockPath = Path.Combine(root, SchemaVersionLock.RelativePath);
            var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(await File.ReadAllTextAsync(lockPath, TestContext.Current.CancellationToken))!;
            entries.Should().ContainSingle().Which.Should().Be(
                new KeyValuePair<string, string>("0.1.0", await versionLock.ComputeDigestAsync("0.1.0")));
            (await File.ReadAllBytesAsync(schema, TestContext.Current.CancellationToken)).Should().Equal(before);
            Directory.GetFiles(root, "*", SearchOption.AllDirectories).Should().HaveCount(2);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
