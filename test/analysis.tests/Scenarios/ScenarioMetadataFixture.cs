using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Analysis.Scenarios;

namespace Specht.Analysis.Tests.Scenarios;

[AutoFixture(typeof(ScenarioMetadata))]
internal sealed partial class ScenarioMetadataFixture
{
    public ScenarioMetadataFixture() => WithPath("src/feature/.spec/tests.feature").WithScenarioId("scenario-1").WithLine(10)
        .WithFeatureScope(new ScenarioScope("0201", "F4")).WithScopeOverride(null).WithClaimTags(Array.Empty<string>())
        .WithRows(new[] { new ScenarioRow(null, "case-1", false) }).WithGeneratorInputPresent(true);

    public ScenarioMetadataFixture WithClaimTags(IReadOnlyList<string> claimTags) => WithList(claimTags);

    public ScenarioMetadataFixture WithRows(IReadOnlyList<ScenarioRow> rows) => WithList(rows);
}
