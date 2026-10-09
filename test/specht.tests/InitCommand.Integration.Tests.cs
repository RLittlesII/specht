using AwesomeAssertions;
using specht.acceptance;

namespace specht.tests;

/// <summary>
/// <c>specht init</c> through the built tool (<c>0001-F4</c> B-006, C-4): with any one of the eight files already under the
/// root, stdout names each of the eight once, relative to the root with <c>/</c> separators, that one as skipped and every
/// other as written. The scenario holds the feature template alone; this pins the decision for each file. The tool is
/// launched as a process because no seam beneath the command exists yet (§ 8).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class InitCommandIntegrationTests : IDisposable
{
    public static TheoryData<string> EightFiles => new(Files);

    [Theory]
    [MemberData(nameof(EightFiles))]
    public void OneOfTheEightFilesPresent_WhenInitRuns_ShouldListItSkippedAndEveryOtherWrittenRelativeToTheRoot(string present)
    {
        // Given
        var root = Path.Combine(_sandbox, "repo");
        var path = Path.Combine([root, .. present.Split('/')]);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.Copy(Path.Combine([AppContext.BaseDirectory, .. present.Split('/')]), path);

        // When
        var (stdout, stderr, _) = Tool.Launch(_sandbox, "init", "--root", "repo");

        // Then
        var lines = stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var file in Files)
        {
            var (marker, other) = file == present ? ("skipped", "written") : ("written", "skipped");
            lines.Where(line => line.Contains(file, StringComparison.Ordinal))
                .Should()
                .ContainSingle($"stdout lists {file} once; {stdout}{stderr}")
                .Which.Should()
                .Contain(marker)
                .And.NotContain(other);
        }

        stdout.Should().NotContain(_sandbox).And.NotContain("\\");
    }

    public void Dispose()
    {
        if (Directory.Exists(_sandbox))
        {
            Directory.Delete(_sandbox, recursive: true);
        }
    }

    private static readonly string[] Files =
    [
        ".spec/schema/spec-structure.schema.json",
        ".spec/schema/feature-spec.frontmatter.schema.json",
        ".spec/schema/task.frontmatter.schema.json",
        ".spec/schema/epic.frontmatter.schema.json",
        ".spec/templates/feature.md",
        ".spec/templates/decision.md",
        ".spec/templates/adr.md",
        ".spec/templates/lesson.md",
    ];

    private readonly string _sandbox = Path.Combine(Path.GetTempPath(), "specht-init-" + Guid.NewGuid().ToString("N"));
}
