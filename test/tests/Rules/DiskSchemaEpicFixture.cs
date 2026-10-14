using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>Supplies a synthetic epic model for schema-evaluation tests.</summary>
[AutoFixture(typeof(EpicFile))]
internal sealed partial class DiskSchemaEpicFixture
{
    public DiskSchemaEpicFixture() => WithRelativePath("epics/0001-epic/epic.md").WithFrontmatter(new FrontmatterFixture());
}
