using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Model;

namespace Specht.Tests.Shared;

/// <summary>Builds a companion snapshot with a relative path and no tags unless overridden.</summary>
[AutoFixture(typeof(FeatureFile))]
internal sealed partial class FeatureFileFixture
{
    public FeatureFileFixture() => WithRelativePath("src/sample/.spec/sample.feature");

    /// <summary>Sets the parsed companion tags.</summary>
    /// <param name="tags">The tags.</param>
    /// <returns>The fixture.</returns>
    public FeatureFileFixture WithTags(params IReadOnlyList<FeatureTag> tags) => WithList(tags);
}
