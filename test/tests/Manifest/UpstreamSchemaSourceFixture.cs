using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Manifest;

namespace Specht.Tests.Manifest;

/// <summary>
/// Builds an <see cref="UpstreamSchemaSource"/>: a synthetic address and version label and no fetched file until a test
/// adds the ones it asserts on. Nothing is fetched from the address.
/// </summary>
[AutoFixture(typeof(UpstreamSchemaSource))]
internal sealed partial class UpstreamSchemaSourceFixture
{
    public UpstreamSchemaSourceFixture() => WithLocation("https://schemas.example/specht/").WithVersion("2026.10").WithFiles();

    /// <summary>Sets the file of each fetched kind.</summary>
    /// <remarks>The generator names the list's setter <c>WithList</c>, after its type; this names it after the source's.</remarks>
    /// <param name="files">The files.</param>
    /// <returns>The fixture.</returns>
    public UpstreamSchemaSourceFixture WithFiles(params IReadOnlyList<UpstreamSchemaFile> files) => WithList(files);
}
