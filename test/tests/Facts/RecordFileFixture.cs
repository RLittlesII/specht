using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Facts;

/// <summary>
/// Builds a <see cref="RecordFile"/>: the ADR <c>src/sample/.spec/adr/0001-sample.md</c> with no frontmatter and no line
/// under a <c>## Status</c> heading, until a test overrides what it asserts on.
/// </summary>
[AutoFixture(typeof(RecordFile))]
internal sealed partial class RecordFileFixture
{
    public RecordFileFixture() =>
        WithRelativePath("src/sample/.spec/adr/0001-sample.md").WithKind("adr").WithFrontmatter(new FrontmatterFixture()).WithStatusLine(null);
}
