using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Model;

namespace Specht.Tests;

/// <summary>
/// Builds a <see cref="ChildItem"/>: the item <c>0001-01-task.md</c> beside the co-located specification at
/// <c>src/sample/.spec/</c> under the root <c>repo</c>, with no frontmatter, until a test overrides what it asserts on.
/// The generated defaults are <see langword="null"/>, and an item with no path is not one discovery gives.
/// </summary>
[AutoFixture(typeof(ChildItem))]
internal sealed partial class ChildItemFixture
{
    public ChildItemFixture() =>
        WithRelativePath("src/sample/.spec/0001-01-task.md")
            .WithFileName("0001-01-task.md")
            .WithFrontmatter(new FrontmatterFixture())
            .WithParentDirectory(Path.Combine("repo", "src", "sample", ".spec"));
}
