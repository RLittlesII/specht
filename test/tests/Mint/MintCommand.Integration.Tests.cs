using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Mint;

/// <summary>
/// The built tool's <c>mint</c> command as a process (<c>0001-F19</c> A-2, item <c>0415</c>): <c>specht mint &lt;type&gt;
/// [--feature &lt;dir&gt;] [--root &lt;dir&gt;]</c> over a synthetic root whose manifest the test writes whole. An opted-in type
/// draws from a one-character alphabet, so the id a request mints is known before it is made and no random source is
/// replaced. A refusal exits one of <c>2</c>, <c>3</c> and <c>4</c> where the claim names that set (OQ-11's default, C-10), and
/// exactly <c>3</c> where it names the invalid manifest (B-015, B-043).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class MintCommandIntegrationTests
{
    [Theory]
    [ClassData(typeof(TypesNotOptedInRequests))]
    public void ATypeTheManifestDoesNotOptIn_WhenRequested_ShouldWriteNothingOnStandardOutput(string manifest)
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw(SpechtTree.ManifestPath, manifest);

        // When
        var (Stdout, Stderr, _) = AcceptanceTool.Launch(tree.Root, "mint", "adr", "--root", ".");

        // Then
        Stdout.Should().BeEmpty(Stderr);
    }

    [Theory]
    [ClassData(typeof(TypesNotOptedInRequests))]
    public void ATypeTheManifestDoesNotOptIn_WhenRequested_ShouldExitWithAFailureCodeTheCheckAlreadyGives(string manifest)
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw(SpechtTree.ManifestPath, manifest);

        // When
        var (_, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "mint", "adr", "--root", ".");

        // Then
        ExitCode.Should().BeOneOf([2, 3, 4], Stderr);
    }

    [Theory]
    [ClassData(typeof(OptedInRepositoryWideTypes))]
    public void AnOptedInType_WhenRequested_ShouldWriteOneLineHoldingOnlyTheMintedId(string type)
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw(
            SpechtTree.ManifestPath,
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "@type@": "^z{3}$" },
              "mint": { "@type@": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """.Replace("@type@", type, StringComparison.Ordinal));

        // When
        var (Stdout, Stderr, _) = AcceptanceTool.Launch(tree.Root, "mint", type, "--root", ".");

        // Then
        Stdout.Should().MatchRegex(@"\Azzz(\r?\n)?\z", Stderr);
    }

    [Theory]
    [ClassData(typeof(OptedInRepositoryWideTypes))]
    public void AnOptedInType_WhenItsIdIsPrinted_ShouldExitZero(string type)
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw(
            SpechtTree.ManifestPath,
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "@type@": "^z{3}$" },
              "mint": { "@type@": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """.Replace("@type@", type, StringComparison.Ordinal));

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "mint", type, "--root", ".");

        // Then
        Stdout.Should().Contain("zzz", Stderr);
        ExitCode.Should().Be(0, Stderr);
    }

    [Theory]
    [ClassData(typeof(PinsBeforeMint))]
    public void IdSettingsUnderAPinBeforeZeroThreeOne_WhenRequested_ShouldExitAsAnInvalidManifest(string pin)
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw(SpechtTree.ManifestPath, """{"schemaVersion":"@pin@","identifiers":{"adr":"^z{3}$"}}""".Replace("@pin@", pin, StringComparison.Ordinal));
        var withoutSettings = AcceptanceTool.Launch(tree.Root, "--root", ".");
        tree.WriteRaw(
            SpechtTree.ManifestPath,
            """
            {
              "schemaVersion": "@pin@",
              "identifiers": { "adr": "^z{3}$" },
              "mint": { "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """.Replace("@pin@", pin, StringComparison.Ordinal));

        // When
        var (_, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "mint", "adr", "--root", ".");

        // Then
        withoutSettings.ExitCode.Should().NotBe(3, withoutSettings.Stderr);
        ExitCode.Should().Be(3, Stderr);
    }

    [Theory]
    [ClassData(typeof(FoldersNamedForRepositoryWideTypes))]
    public void ARepositoryWideTypeRequestedForAFeature_WhenRequested_ShouldWriteNothingOnStandardOutput(string type, string folder)
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteCoLocatedFeature("features/one", "0001", "F1");
        tree.WriteRaw(
            SpechtTree.ManifestPath,
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "@type@": "^z{3}$" },
              "mint": { "@type@": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """.Replace("@type@", type, StringComparison.Ordinal));

        // When
        var (Stdout, Stderr, _) = AcceptanceTool.Launch(tree.Root, "mint", type, "--feature", folder, "--root", ".");

        // Then
        Stdout.Should().BeEmpty(Stderr);
    }

    [Theory]
    [ClassData(typeof(ManifestsOptingInATypeWithNoGrammar))]
    public void AManifestOptingInATypeWithNoGrammar_WhenRequested_ShouldExitAsAnInvalidManifest(string manifest, string type)
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw(SpechtTree.ManifestPath, manifest);

        // When
        var (_, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "mint", type, "--root", ".");

        // Then
        ExitCode.Should().Be(3, Stderr);
    }

    [Theory]
    [ClassData(typeof(RefusedRequests))]
    public void ARefusedRequestUnderAnAbsoluteRoot_WhenRequested_ShouldNameNoAbsolutePath(string manifest, string type, string? feature)
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteCoLocatedFeature("features/one", "0001", "F1");
        tree.WriteRaw(SpechtTree.ManifestPath, manifest);
        string[] arguments = feature is null
            ? ["mint", type, "--root", tree.Root]
            : ["mint", type, "--feature", feature, "--root", tree.Root];

        // When
        var (Stdout, Stderr, _) = AcceptanceTool.Launch(tree.Root, arguments);

        // Then
        Stdout.Should().NotContain(tree.Root);
        Stderr.Should().NotContain(tree.Root);
    }
}
