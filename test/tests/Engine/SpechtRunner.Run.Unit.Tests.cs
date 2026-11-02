using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Tool;

namespace Specht.Tests.Engine;

/// <summary>
/// The order of a run (<c>0001-F5</c> B-018; ADR-0005 (d)): the manifest is loaded and judged before any rule is
/// evaluated, so a rejected manifest ends the run with the rejection and no rule has run. The runner is resolved from the
/// engine's registrations over a file system held in memory, beside a rule that records whether it was evaluated, so the
/// evaluator the runner calls is the one the engine composes. Nothing touches the disk.
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
                [Path.Combine("repo", SpechtTree.ManifestPath)] = new("""{ "notAKeyTheEngineKnows": true }"""),
            });
        var rule = new RecordingRule();
        using var resolver = (TypeResolver)new TypeRegistrar(
            new ServiceCollection().AddSingleton<IFileSystem>(fileSystem).AddSpechtEngine().AddSingleton<ISpechtRule>(rule)).Build();
        var runner = (SpechtRunner)resolver.Resolve(typeof(SpechtRunner))!;

        // When
        var running = () => runner.Run(fileSystem.Path.GetFullPath("repo"));

        // Then
        running.Should().Throw<SpechtManifestException>();
        rule.Evaluated.Should().BeFalse();
    }

    private sealed class RecordingRule : ISpechtRule
    {
        public bool Evaluated { get; private set; }

        public string Id => "SPEC010";

        public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
        {
            Evaluated = true;
            return [];
        }
    }
}
