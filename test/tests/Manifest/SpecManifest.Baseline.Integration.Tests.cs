using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Baseline;

namespace Specht.Tests.Manifest;

/// <summary>
/// The engine's verdicts on the baseline tree under the default manifest against the golden report (<c>0001-F5</c> B-016,
/// A-2; <c>0001-F1</c> B-004, C-9, C-10): once with every key declared, once with every value reached by omission.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpecManifestBaselineIntegrationTests
{
    /// <summary>Gets each way the default manifest reaches the engine, and the manifest written over the tree's copy, if any.</summary>
    public static TheoryData<string, string?> DefaultManifests { get; } = new()
    {
        { "the default manifest as the repository ships it", null },
        { "the default manifest's values reached by omission", "{\"$comment\": \"Every value is left out, so each is the default manifest's.\"}" },
    };

    [Theory]
    [MemberData(nameof(DefaultManifests))]
    public void TheBaselineTree_WhenCheckedUnderTheDefaultManifest_ShouldGiveTheGoldenReportsVerdictsFieldForFieldInOrder(
        string because,
        string? manifest)
    {
        // Given
        using var tree = new SpecTree();

        if (manifest is not null)
        {
            tree.WriteRaw(SpecManifest.RelativePath, manifest);
        }

        BaselineTree.Write(tree);
        var golden = GoldenReport.Read();

        // When
        var verdicts = GoldenReport.Of(tree.Run());

        // Then
        verdicts.Should().Equal(golden, because);
    }
}
