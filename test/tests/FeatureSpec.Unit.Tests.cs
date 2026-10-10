using System.Text.Json.Nodes;
using AwesomeAssertions;

namespace Specht.Tests;

/// <summary>
/// A co-located Feature's identity, read from its frontmatter <c>epic</c> and <c>id</c> wherever it sits
/// (<c>0001-F1</c> B-003).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class FeatureSpecUnitTests
{
    public static TheoryData<string> CoLocatedPaths { get; } = new()
    {
        "src/area/.spec/README.md",
        "src/unrelated/deeper/area/.spec/README.md",
        "lib/0002-F9/.spec/README.md",
    };

    [Theory]
    [MemberData(nameof(CoLocatedPaths))]
    public void ACoLocatedSpecificationAtAnyPath_WhenItsIdentityIsRead_ShouldBeItsFrontmatterEpicAndId(string path)
    {
        // Given
        FeatureSpec feature = new FeatureSpecFixture()
            .WithLocation(new SpecLocationFixture().WithRelativePath(path))
            .WithDocument(
                new SpecDocumentFixture()
                    .WithRelativePath(path)
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["epic"] = "0001", ["id"] = "F2" })));

        // When
        var identity = feature.Identity;

        // Then
        identity.Should().Be("0001-F2");
    }
}
