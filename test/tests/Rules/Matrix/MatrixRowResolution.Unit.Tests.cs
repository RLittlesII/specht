using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Rules.Matrix;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules.Matrix;

/// <summary>
/// Whether a traceability row's tests are skipped (<c>0001-F13</c> B-019): its trimmed Status is the waived, withdrawn or
/// superseded marker of the manifest it is read under, compared ordinally, and a marker the manifest lacks matches nothing.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class MatrixRowResolutionUnitTests
{
    [Theory]
    [ClassData(typeof(MatrixRowSkipStatuses))]
    public void ARowStatus_WhenItsSkipIsRead_ShouldSkipOnlyTheTrimmedWaivedWithdrawnOrSupersededMarkerOrdinally(
        string schemaVersion, string? waived, string status, bool skips)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = schemaVersion };
        if (waived is not null)
        {
            manifest["markers"] = new JsonObject { ["waived"] = waived };
        }

        var markers = EngineServices.Parser().Parse(manifest.ToJsonString()).Markers;
        MatrixRowResolution row = new MatrixRowResolutionFixture().WithStatus(status);

        // When
        var skipped = row.Skips(markers);

        // Then
        skipped.Should().Be(skips);
    }
}
