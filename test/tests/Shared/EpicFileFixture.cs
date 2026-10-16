using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Model;

namespace Specht.Tests.Shared;

/// <summary>
/// Builds an <see cref="EpicFile"/>: <c>epics/0001-epic/epic.md</c> with no frontmatter, until a test overrides what it
/// asserts on.
/// </summary>
[AutoFixture(typeof(EpicFile))]
internal sealed partial class EpicFileFixture
{
    public EpicFileFixture() => WithRelativePath("epics/0001-epic/epic.md").WithFrontmatter(new FrontmatterFixture());
}
