using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Specht.Tests;

/// <summary>
/// Builds a <see cref="FeatureSpec"/>: a <see cref="SpecLocationFixture"/> location and a <see cref="SpecDocumentFixture"/>
/// document, with no companion <c>.feature</c> file, until a test overrides what it asserts on. The generated location and
/// document are <see langword="null"/>, which discovery never gives.
/// </summary>
[AutoFixture(typeof(FeatureSpec))]
internal sealed partial class FeatureSpecFixture
{
    public FeatureSpecFixture() => WithLocation(new SpecLocationFixture()).WithDocument(new SpecDocumentFixture());

    /// <summary>Sets the companion <c>.feature</c> files.</summary>
    /// <remarks>The generator names the list's setter <c>WithList</c>, after its type; this names it after the Feature's.</remarks>
    /// <param name="featureFiles">The files.</param>
    /// <returns>The fixture.</returns>
    public FeatureSpecFixture WithFeatureFiles(params IReadOnlyList<string> featureFiles) => WithList(featureFiles);
}
