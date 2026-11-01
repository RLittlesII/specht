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
}
