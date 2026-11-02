using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Discovery;

namespace Specht.Tests.Shared;

/// <summary>
/// Builds a <see cref="SpechtLocation"/>: a co-located specification at the root-relative <c>src/sample/.spec/README.md</c>
/// until a test overrides what it asserts on. The generated defaults are <see langword="null"/>, and a location with no
/// path is not one discovery gives.
/// </summary>
[AutoFixture(typeof(SpechtLocation))]
internal sealed partial class SpechtLocationFixture
{
    public SpechtLocationFixture() =>
        WithRelativePath("src/sample/.spec/README.md")
            .WithLayout(new SpechtLayout("features", "**/.spec/README.md"))
            .WithDirectory("src/sample/.spec");
}
