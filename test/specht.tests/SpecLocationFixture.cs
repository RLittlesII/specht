using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace specht.tests;

/// <summary>
/// Builds a <see cref="SpecLocation"/>: a co-located specification at <c>src/sample/.spec/README.md</c> under the root
/// <c>repo</c> until a test overrides what it asserts on. The generated defaults are <see langword="null"/>, and a location
/// with no path is not one discovery gives.
/// </summary>
[AutoFixture(typeof(SpecLocation))]
internal sealed partial class SpecLocationFixture
{
    public SpecLocationFixture() =>
        WithAbsolutePath(Path.Combine("repo", "src", "sample", ".spec", "README.md"))
            .WithRelativePath("src/sample/.spec/README.md")
            .WithLayout(new SpecLayout("features", "**/.spec/README.md"))
            .WithDirectory(Path.Combine("repo", "src", "sample", ".spec"));
}
