using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;

namespace Specht.Tests.Engine;

/// <summary>
/// The order of a run (<c>0001-F5</c> B-018; ADR-0005 (d)): the manifest is loaded and judged before any rule is
/// evaluated, so a rejected manifest ends the run with the rejection and no rule has run. The runner is constructed by
/// hand over a file system held in memory and a rule that records whether it was evaluated. Nothing touches the disk.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtRunnerRunUnitTests
{
    [Fact]
    public void AManifestTheEngineRejects_WhenRun_ShouldThrowTheRejectionAndEvaluateNoRule()
    {
        // Given
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine("repo", ".spec", "schema", "spec-structure.schema.json")] = new("""{ "notAKeyTheEngineKnows": true }"""),
            });
        var rule = new RecordingRule();
        var runner = new SpechtRunner(
            new SpecModelLoader(
                fileSystem,
                new FrontmatterReader(fileSystem),
                new SpecDiscovery(fileSystem),
                new SpecSchemasLoader(fileSystem)),
            [rule],
            fileSystem);

        // When
        var running = () => runner.Run(fileSystem.Path.GetFullPath("repo"));

        // Then
        running.Should().Throw<SpechtManifestException>();
        rule.Evaluated.Should().BeFalse();
    }

    private sealed class RecordingRule : ISpecRule
    {
        public bool Evaluated { get; private set; }

        public string Id => "SPEC010";

        public IReadOnlyList<string> ReportedIds => ["SPEC010"];

        public IEnumerable<SpecViolation> Evaluate(SpecModel model)
        {
            Evaluated = true;
            return [];
        }
    }
}
