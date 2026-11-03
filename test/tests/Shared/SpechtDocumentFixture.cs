using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Model;

namespace Specht.Tests.Shared;

/// <summary>
/// Builds a <see cref="SpechtDocument"/>: <c>src/sample/.spec/README.md</c> with no frontmatter and no section until a test
/// overrides what it asserts on. The generated path and frontmatter are <see langword="null"/>, which no parse gives.
/// </summary>
[AutoFixture(typeof(SpechtDocument))]
internal sealed partial class SpechtDocumentFixture
{
    public SpechtDocumentFixture() => WithRelativePath("src/sample/.spec/README.md").WithFrontmatter(new FrontmatterFixture());

    /// <summary>Sets the document's sections.</summary>
    /// <remarks>The generator names the list's setter <c>WithList</c>, after its type; this names it after the document's.</remarks>
    /// <param name="sections">The sections.</param>
    /// <returns>The fixture.</returns>
    public SpechtDocumentFixture WithSections(params IReadOnlyList<SpechtSection> sections) => WithList(sections);
}
