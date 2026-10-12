using System.Security.Cryptography;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Tool;

namespace Specht.Tests.Check;

/// <summary>
/// <c>specht --version</c> (<c>0001-F2</c> B-040, B-041, B-042, B-043, B-044), through the built tool: the option is the
/// host's, configured in <c>Program.cs</c>, which Spectre's command tester does not run. With <c>--version</c> as the only
/// argument, stdout is the informational version the launched assembly's metadata states and nothing else, and the exit
/// code is <c>0</c>, in a root the check reports a violation in and in one holding no manifest, so neither is the check's
/// output or code; no file under the working directory is created, modified or deleted; and <c>--help</c> names the option.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class ToolVersionIntegrationTests : IDisposable
{
    /// <summary>Gets the working directories the version is asked for in, each holding a violation: whether it holds a manifest.</summary>
    public static TheoryData<string, bool> WorkingDirectories =>
        new()
        {
            { "a root holding a manifest and a specification with a violation", true },
            { "a directory with no manifest at the manifest path", false },
        };

    [Theory]
    [MemberData(nameof(WorkingDirectories))]
    public void VersionAsTheOnlyArgument_WhenTheToolRuns_ShouldPrintTheVersionItsAssemblyStatesAndNothingElse(string directory, bool manifest)
    {
        // Given
        Arrange(manifest);
        var version = AcceptanceTool.InformationalVersion();

        // When
        var (stdout, stderr, _) = AcceptanceTool.Launch(_tree.Root, "--version");

        // Then
        version.Should().NotBeNullOrEmpty();
        stdout.TrimEnd('\r', '\n').Should().Be(version, $"{directory}; {stderr}");
    }

    [Theory]
    [MemberData(nameof(WorkingDirectories))]
    public void VersionAsTheOnlyArgument_WhenTheToolRuns_ShouldExitWithZero(string directory, bool manifest)
    {
        // Given
        Arrange(manifest);

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Launch(_tree.Root, "--version");

        // Then
        exitCode.Should().Be(ExitCodes.Success, $"{directory}; {stdout}{stderr}");
    }

    [Fact]
    public void VersionAsTheOnlyArgument_WhenTheToolRuns_ShouldCreateModifyOrDeleteNoFileUnderTheWorkingDirectory()
    {
        // Given
        _tree.WriteFeature("0001", "F1", featureFile: null);
        var before = Snapshot();

        // When
        AcceptanceTool.Launch(_tree.Root, "--version");

        // Then
        Snapshot().Should().Equal(before);
    }

    [Fact]
    public void Help_WhenTheToolRuns_ShouldNameTheVersionOption()
    {
        // Given
        var workingDirectory = _tree.Root;

        // When
        var (stdout, stderr, _) = AcceptanceTool.Launch(workingDirectory, "--help");

        // Then
        stdout.Should().Contain("--version", stderr);
    }

    /// <inheritdoc/>
    public void Dispose() => _tree.Dispose();

    private void Arrange(bool manifest)
    {
        _tree.WriteFeature("0001", "F1", featureFile: null);
        if (!manifest)
        {
            File.Delete(Path.Combine(_tree.Root, SpecTree.ManifestPath));
        }
    }

    private SortedDictionary<string, string> Snapshot() =>
        new(
            Directory.EnumerateFiles(_tree.Root, "*", new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = 0 })
                .ToDictionary(
                    file => Path.GetRelativePath(_tree.Root, file).Replace(Path.DirectorySeparatorChar, '/'),
                    static file => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file)))),
            StringComparer.Ordinal);

    private readonly SpecTree _tree = new();
}
