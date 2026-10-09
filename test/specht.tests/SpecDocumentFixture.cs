using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace specht.tests;

/// <summary>
/// Builds a <see cref="SpecDocument"/>: <c>src/sample/.spec/README.md</c> with no frontmatter and no section until a test
/// overrides what it asserts on. The generated path and frontmatter are <see langword="null"/>, which no parse gives.
/// </summary>
[AutoFixture(typeof(SpecDocument))]
internal sealed partial class SpecDocumentFixture
{
    public SpecDocumentFixture() => WithRelativePath("src/sample/.spec/README.md").WithFrontmatter(new FrontmatterFixture());
}
