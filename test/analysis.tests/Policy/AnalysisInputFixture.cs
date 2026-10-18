using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Tests.Policy;

[AutoFixture(typeof(AnalysisInput))]
internal sealed partial class AnalysisInputFixture
{
    public AnalysisInputFixture() => WithKind("feature").WithRelativePath("src/sample/.spec/README.md").WithText(string.Empty);
}
