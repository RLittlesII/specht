using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Tests.Policy;

[AutoFixture(typeof(AnalysisDeclaration))]
internal sealed partial class AnalysisDeclarationFixture
{
    public AnalysisDeclarationFixture() => WithProject("test/tests.csproj").WithTargetFramework("net10.0").WithRelativePath("test/Run.cs")
        .WithKind("method").WithSymbolId("M:Run").WithGenerated(false).WithRecognizedTest(false);
}
