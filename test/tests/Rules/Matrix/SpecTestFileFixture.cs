using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Discovery;

namespace Specht.Tests.Rules.Matrix;

/// <summary>A synthetic test file's text and root-relative path.</summary>
[AutoFixture(typeof(SpecTestFile))]
internal sealed partial class SpecTestFileFixture
{
    public SpecTestFileFixture() => WithRelativePath("test/sample.cs").WithText(string.Empty);
}
