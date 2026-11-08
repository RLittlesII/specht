using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.Policy;
using Specht.Tool.Features.Evidence;

namespace Specht.Analysis.Tests.Gherkin;

[Trait("Tier", "Unit")]
public sealed class ClaimFileSetParityUnitTests
{
    [Theory]
    [ClassData(typeof(ClaimFileSetParityCases))]
    public void AClaimFileSetGlobSpelling_WhenReadByTheAnalyzerAndTheEngine_ShouldCatalogueTheClaimFileOrRejectTheGlobInBoth(
        string glob, string claimFile, bool? read)
    {
        // Given
        var sets = new JsonArray(new JsonObject { ["glob"] = glob, ["feature"] = "arithmetic" });
        var patch = new JsonObject { ["codeAnalysis"] = new JsonObject { ["records"] = new JsonObject { ["claimFileSets"] = sets } } };
        var inputs = new GherkinInputs().Merge(patch.ToJsonString()).Replace("scenario", claimFile, GherkinInputs.Arithmetic);
        var paths = new RepositoryPaths();

        // When
        var snapshot = new AnalysisInputReader().Read(inputs.Inputs, TestContext.Current.CancellationToken);
        bool? analyzer = snapshot.Available
            ? snapshot.Catalog!.Features.SelectMany(static feature => feature.Claims).Any(claim => claim.RelativePath == claimFile)
            : null;
        bool? engine;
        try
        {
            engine = paths.Matches(claimFile, paths.Normalize(glob));
        }
        catch (ArgumentException)
        {
            engine = null;
        }

        // Then
        analyzer.Should().Be(read);
        engine.Should().Be(read);
        snapshot.Failures.Select(static failure => (failure.RelativePath, failure.Message)).Should().Equal(read is null
            ? [(GherkinInputs.ConfigurationPath, "codeAnalysis.records.claimFileSets is missing or invalid.")]
            : []);
    }
}
