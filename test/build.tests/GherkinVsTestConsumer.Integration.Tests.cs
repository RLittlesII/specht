using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Model;
using Specht.Tool.Features.Evidence;

namespace Specht.Build.Tests;

[Collection("Repository package builds")]
[Trait("Tier", "Integration")]
public sealed partial class GherkinVsTestConsumerIntegrationTests(GherkinVsTestConsumerIntegrationTests.ConsumerRun run)
    : IClassFixture<GherkinVsTestConsumerIntegrationTests.ConsumerRun>
{
    [Theory]
    [ClassData(typeof(ConsumerProjects))]
    public async Task ClaimFilesInACentralSpecFeaturesTree_WhenEachProjectIsEvaluated_ShouldReachItAsInputsWithNoBuildProperty(string project)
    {
        // Given
        var evidence = await Collected();
        evidence.Consumer.Authored().Select(evidence.Consumer.Text).Should()
            .NotContain(static text => text.Contains("SpechtScenarioInputs", StringComparison.Ordinal)
                || text.Contains("SpechtFeatureInputs", StringComparison.Ordinal) || text.Contains("SpechtRoot", StringComparison.Ordinal),
                evidence.Because);

        // When
        var inputs = evidence.Inputs[project];

        // Then
        inputs.Should().Contain(["configuration|.specht/manifest.json", "scenario|" + ClaimFile, "feature|" + Record,
            "schema:feature|.specht/schema/frontmatter/feature.schema.json"], evidence.Because);
    }

    [Fact]
    public async Task CleanCheckoutWithMandatoryEnforcementAndWarningsAsErrors_WhenBothProjectsBuildBeforeAnyEvidence_ShouldSucceedWithNoSpctDiagnostic()
    {
        // Given
        var evidence = await Collected();
        var manifest = evidence.Consumer.Text(".specht/manifest.json");
        manifest.Should().Contain("\"role\": \"implements\"", evidence.Because).And.Contain("\"role\": \"verifies\"", evidence.Because);
        evidence.Consumer.Text("Directory.Build.props").Should().Contain("<TreatWarningsAsErrors>true</TreatWarningsAsErrors>", evidence.Because);
        evidence.Consumer.Text(Record).Should().NotContain("|", evidence.Because);

        // When
        var build = evidence.Builds["clean"];

        // Then
        build.ExitCode.Should().Be(0, evidence.Because);
        build.Output.Should().NotContain("SPCT", evidence.Because);
    }

    [Fact]
    public async Task FixtureSources_WhenSearched_ShouldCarryNoSuppressionOfAnyKind()
    {
        // Given
        var evidence = await Collected();
        evidence.Consumer.Text(Manifest).Should().Contain($"\"exclude\": [\"{PendingSource}\"]", evidence.Because);

        // When
        var authored = evidence.Consumer.Authored().ToArray();

        // Then
        authored.Should().NotBeEmpty(evidence.Because).And.NotContain(static file => file.EndsWith(".editorconfig", StringComparison.Ordinal),
            evidence.Because);
        authored.Select(evidence.Consumer.Text).Should().NotContain(static text => text.Contains("NoWarn", StringComparison.Ordinal)
            || text.Contains("#pragma", StringComparison.Ordinal) || text.Contains("SuppressMessage", StringComparison.Ordinal)
            || text.Contains("dotnet_diagnostic", StringComparison.Ordinal) || text.Contains("WarningsNotAsErrors", StringComparison.Ordinal),
            evidence.Because);
    }

    [Fact]
    public async Task FixtureAndItsEvidence_WhenSearched_ShouldCarryNoReqnrollPackageGeneratedTestOrScenarioRecord()
    {
        // Given
        var evidence = await Collected();

        // When
        var executed = evidence.Artifact("run");

        // Then
        evidence.Consumer.Authored().Select(evidence.Consumer.Text).Should()
            .NotContain(static text => text.Contains("Reqnroll", StringComparison.OrdinalIgnoreCase), evidence.Because);
        executed.GetProperty("discovery").GetProperty("cases").EnumerateArray()
            .Select(static record => record.GetProperty("methodSymbolId").GetString()).Should()
            .OnlyContain(static symbol => symbol!.StartsWith("M:Consumer.Ledger.Tests.", StringComparison.Ordinal), evidence.Because);
        (executed.TryGetProperty("scenarios", out var scenarios) && scenarios.GetArrayLength() > 0).Should().BeFalse(evidence.Because);
    }

    [Fact]
    public async Task CleanCheckout_WhenReconciledBeforeAnyEvidence_ShouldBeIncompleteNamingTheTestProjectAndTheProducingSteps()
    {
        // Given
        var evidence = await Collected();

        // When
        var result = evidence.Reconciliations["empty"];

        // Then
        result.ExitCode.Should().Be(2, evidence.Because);
        result.Report["inputState"]!["state"]!.GetValue<string>().Should().Be("incomplete", evidence.Because);
        result.Report["findings"]!.AsArray().Should().Contain(static finding => finding!["kind"]!.GetValue<string>() == "missingCompilation"
            && finding["path"]!.GetValue<string>() == TestProject
            && finding["message"]!.GetValue<string>().Contains("dotnet test --list-tests", StringComparison.Ordinal), evidence.Because);
    }

    [Theory]
    [ClassData(typeof(CaseOutcomes))]
    public async Task FactOrTheoryRow_WhenListedThenRunUnderVsTest_ShouldKeepItsOwnCaseAndOutcome(string symbolId, string[] outcomes)
    {
        // Given
        var evidence = await Collected();

        // When
        var discovery = evidence.Artifact("list");
        var executed = evidence.Artifact("run");

        // Then
        discovery.GetProperty("discovery").GetProperty("state").GetString().Should().Be("complete", evidence.Because);
        var discovered = CaseIds(discovery, symbolId);
        discovered.Should().HaveCount(outcomes.Length, evidence.Because).And.OnlyHaveUniqueItems(evidence.Because);
        CaseIds(executed, symbolId).Should().BeEquivalentTo(discovered, evidence.Because);
        discovered.Select(caseId => Outcome(executed, caseId)).Should().BeEquivalentTo(outcomes, evidence.Because);
    }

    [Fact]
    public async Task TestMethodCompiledFromOutsideTheTestProject_WhenDiscoveredUnderVsTest_ShouldResolveToItsRepositoryRelativeSource()
    {
        // Given
        var evidence = await Collected();

        // When
        var discovery = evidence.Artifact("list");

        // Then
        CaseIds(discovery, ReversalFact).Should().ContainSingle(evidence.Because);
        discovery.GetProperty("discovery").GetProperty("methods").EnumerateArray()
            .Should().ContainSingle(static method => method.GetProperty("symbolId").GetString() == ReversalFact, evidence.Because).Subject
            .GetProperty("location").GetProperty("path").GetString().Should().Be("test/shared/ReversalTests.cs", evidence.Because);
    }

    [Theory]
    [ClassData(typeof(ScopedOwnership))]
    public async Task RequiredCaseScopeAsACategoryTraitExpression_WhenTheRunIsRecorded_ShouldOwnExactlyTheCasesItsExpressionHolds(
        string symbolId, string[] categories, bool owned)
    {
        // Given
        var evidence = await Collected();
        evidence.Consumer.Text(".specht/manifest.json").Should().Contain("\"requiredCaseScope\": \"not @pending\"", evidence.Because);

        // When
        var executed = evidence.Artifact("run");

        // Then
        var records = Cases(executed, symbolId);
        records.Should().NotBeEmpty(evidence.Because);
        records.Should().AllSatisfy(record =>
        {
            record.GetProperty("categories").EnumerateArray().Select(static category => category.GetString()).Should().Equal(categories, evidence.Because);
            record.GetProperty("owned").GetBoolean().Should().Be(owned, evidence.Because);
        });
    }

    [Fact]
    public async Task TestProjectWithNoRequiredCaseScope_WhenRebuiltAndRun_ShouldOwnEveryCase()
    {
        // Given
        var evidence = await Collected();

        // When
        var executed = evidence.Artifact("unscoped-run");

        // Then
        var records = executed.GetProperty("discovery").GetProperty("cases").EnumerateArray().ToArray();
        records.Should().HaveCount(7, evidence.Because);
        records.Should().OnlyContain(static record => record.GetProperty("owned").GetBoolean(), evidence.Because);
    }

    [Fact]
    public async Task NativeClaimsInTheClaimFileSet_WhenTheGateReconcilesAnArtifactCarryingNoCatalogue_ShouldReadEachClaimFromItsClaimFile()
    {
        // Given
        var evidence = await Collected();
        var executed = evidence.Artifact("run");
        executed.TryGetProperty("catalog", out _).Should().BeFalse(evidence.Because);

        // When
        var claims = evidence.Reconciliations["run"].Report["claims"]!.AsArray();

        // Then
        claims.Select(static claim => (claim!["epic"]?.GetValue<string>(), claim["feature"]!.GetValue<string>(), claim["id"]!.GetValue<string>(),
                claim["path"]!.GetValue<string>(), claim["links"]!["test"]!["state"]!.GetValue<string>()))
            .Should().Equal([((string?) null, "ledger", "REQ-LED-001", ClaimFile, "satisfied"), (null, "ledger", "REQ-LED-002", ClaimFile, "satisfied")],
                evidence.Because);
        claims[0]!["cases"]!.AsArray().Select(static item => item!.GetValue<string>()).Should()
            .Contain(CaseIds(executed, PostingFact).Concat(CaseIds(executed, PostingTheory)), evidence.Because);
        claims[1]!["cases"]!.AsArray().Select(static item => item!.GetValue<string>()).Should()
            .Contain(CaseIds(executed, ReversalFact), evidence.Because);
    }

    [Fact]
    public async Task ProductionProjectNamedOnlyByImplementsEnforcement_WhenReconciledWithAndWithoutTestEvidence_ShouldNeedNoExportAndNeverBeMissing()
    {
        // Given
        var evidence = await Collected();
        evidence.Artifact("run").GetProperty("project").GetProperty("path").GetString().Should().Be(TestProject, evidence.Because);

        // When
        var (empty, run) = (evidence.Reconciliations["empty"], evidence.Reconciliations["run"]);

        // Then
        empty.Report["findings"]!.AsArray().Should().NotContain(static finding => finding!["kind"]!.GetValue<string>() == "missingCompilation"
            && finding["path"]!.GetValue<string>() == ProductionProject, evidence.Because);
        run.Report["inputState"]!["state"]!.GetValue<string>().Should().Be("complete", evidence.Because);
        run.Report["findings"]!.AsArray().Should().NotContain(static finding => finding!["path"]!.GetValue<string>() == ProductionProject,
            evidence.Because);
    }

    [Fact]
    public async Task NativeClaimsInACentralTree_WhenBothProjectsBind_ShouldResolveTheSameClaimsForEach()
    {
        // Given
        var evidence = await Collected();
        evidence.Consumer.Text("src/Ledger/Postings.cs").Should().Contain("<implements feature=\"ledger\" claim=\"REQ-LED-002\" />", evidence.Because);

        // When
        var bindings = evidence.Artifact("run").GetProperty("bindings").EnumerateArray().ToArray();

        // Then
        evidence.Builds["clean"].Output.Should().Contain("Ledger.dll", evidence.Because).And.NotContain("SPCT", evidence.Because);
        bindings.Should().HaveCount(3, evidence.Because);
        bindings.Should().OnlyContain(static binding => binding.GetProperty("status").GetString() == "resolved"
            && binding.GetProperty("isEligible").GetBoolean(), evidence.Because);
        bindings.Select(static binding => binding.GetProperty("target")).Select(static target => target.GetProperty("epic").ValueKind
                + " " + target.GetProperty("feature").GetString() + " " + target.GetProperty("id").GetString()
                + " " + target.GetProperty("relativePath").GetString()).Distinct()
            .Should().BeEquivalentTo(["Null ledger REQ-LED-001 " + ClaimFile, "Null ledger REQ-LED-002 " + ClaimFile], evidence.Because);
    }

    [Fact]
    public async Task DocumentedSequence_WhenFollowedOnACleanCheckout_ShouldPassTheDefaultGateWithCurrentEvidence()
    {
        // Given
        var evidence = await Collected();
        evidence.Artifact("list").GetProperty("results").GetProperty("state").GetString().Should().Be("absent", evidence.Because);
        evidence.Artifact("run").GetProperty("results").GetProperty("state").GetString().Should().Be("available", evidence.Because);

        // When
        var result = evidence.Reconciliations["run"];

        // Then
        result.ExitCode.Should().Be(0, evidence.Because);
        result.Report["inputState"]!["state"]!.GetValue<string>().Should().Be("complete", evidence.Because);
        result.Report["findings"]!.AsArray().Select(static finding => finding!["kind"]!.GetValue<string>()).Should()
            .NotContain(["missingCompilation", "staleEvidence", "invalidInput", "deferredCases", "missingRequiredLink", "missingResult"],
                evidence.Because);
    }

    [Fact]
    public async Task FixtureWithoutItsRequiredCaseScopeAndWithItsPendingMethodsBound_WhenReconciled_ShouldFailTheDefaultGateForTheFailedAndSkippedCases()
    {
        // Given
        var evidence = await Collected();
        var executed = evidence.Artifact("unscoped-run");
        var notPassed = new[] { SkippedFact, FailingFact }.SelectMany(symbolId => CaseIds(executed, symbolId)).ToArray();
        notPassed.Should().HaveCount(2, evidence.Because);

        // When
        var result = evidence.Reconciliations["unscoped"];

        // Then
        result.ExitCode.Should().Be(1, evidence.Because);
        result.Report["findings"]!.AsArray().Where(static finding => finding!["kind"]!.GetValue<string>() == "missingResult")
            .Select(static finding => finding!["message"]!.GetValue<string>())
            .Should().BeEquivalentTo(notPassed.Select(static caseId => $"Required case {caseId} did not pass."), evidence.Because);
    }

    [Fact]
    public async Task ArtifactSuppliedThroughSpechtTestEvidence_WhenTheTestProjectRebuilds_ShouldBuildWithNoSpctDiagnostic()
    {
        // Given
        var evidence = await Collected();
        evidence.Consumer.Text(TestProject).Should().Contain("<SpechtTestEvidence Include=", evidence.Because);

        // When
        var rebuild = evidence.Builds["rebuild"];

        // Then
        rebuild.ExitCode.Should().Be(0, evidence.Because);
        rebuild.Output.Should().NotContain("SPCT", evidence.Because);
    }

    [Theory]
    [ClassData(typeof(MissingRelationships))]
    public async Task MandatoryRelationshipRemoved_WhenBuiltInItsEnforcementScope_ShouldFailWithTheMissingRelationshipDiagnostic(
        string step, string declaration)
    {
        // Given
        var evidence = await Collected();

        // When
        var build = evidence.Builds[step];

        // Then
        build.ExitCode.Should().NotBe(0, evidence.Because);
        Diagnostics(build.Output).Should().Contain(static diagnostic => diagnostic.StartsWith("SPCT0007|", StringComparison.Ordinal), evidence.Because);
        build.Output.Should().MatchRegex($"error SPCT0007: [^\\r\\n]*{declaration}", evidence.Because);
    }

    [Fact]
    public async Task TestMethodMissingVerifiesWithOnlyStaleEvidence_WhenBuilt_ShouldSucceedWhileRecognitionIsProvisional()
    {
        // Given
        var evidence = await Collected();

        // When
        var build = evidence.Builds["missing-verifies-build"];

        // Then
        build.ExitCode.Should().Be(0, evidence.Because);
        build.Output.Should().NotContain("SPCT", evidence.Because);
    }

    [Fact]
    public async Task RelationshipNamingAClaimAbsentFromTheNativeCatalogue_WhenBuilt_ShouldFailWithTheAbsentTargetDiagnostic()
    {
        // Given
        var evidence = await Collected();

        // When
        var build = evidence.Builds["unknown-claim"];

        // Then
        build.ExitCode.Should().NotBe(0, evidence.Because);
        build.Output.Should().MatchRegex("error SPCT0004: [^\\r\\n]*REQ-LED-009", evidence.Because);
    }

    [Theory]
    [ClassData(typeof(MissingInputs))]
    public async Task RequiredInputRemoved_WhenBuilt_ShouldFailWithTheInputFailureDiagnostic(string step)
    {
        // Given
        var evidence = await Collected();

        // When
        var build = evidence.Builds[step];

        // Then
        build.ExitCode.Should().NotBe(0, evidence.Because);
        build.Output.Should().Contain("error SPCT0001", evidence.Because);
    }

    [Fact]
    public async Task SourceEditedAfterTheRun_WhenReconciledAndRebuilt_ShouldReportStaleEvidenceExcludedFromTheGateAndStillBuild()
    {
        // Given
        var evidence = await Collected();

        // When
        var report = evidence.Reconciliations["stale"].Report;

        // Then
        var stale = report["findings"]!.AsArray().Where(static finding => finding!["kind"]!.GetValue<string>() == "staleEvidence").ToArray();
        stale.Select(static finding => finding!["path"]!.GetValue<string>()).Should().Contain("test/Ledger.Tests/PostingTests.cs", evidence.Because);
        stale.Select(static finding => finding!["message"]!.GetValue<string>()).Should()
            .OnlyContain(static message => message.Contains("rebuild", StringComparison.OrdinalIgnoreCase), evidence.Because);
        report["bindings"]!.AsArray().Should().NotBeEmpty(evidence.Because)
            .And.OnlyContain(static binding => !binding!["current"]!.GetValue<bool>(), evidence.Because);
        report["cases"]!.AsArray().Should().NotBeEmpty(evidence.Because)
            .And.OnlyContain(static row => row!["outcome"]!.GetValue<string>() == "absent", evidence.Because);
        evidence.Builds["stale"].ExitCode.Should().Be(0, evidence.Because);
        evidence.Builds["stale"].Output.Should().NotContain("SPCT", evidence.Because);
    }

    [Theory]
    [ClassData(typeof(ContinuousIntegrationPairs))]
    public async Task SameConfiguration_WhenBuiltLocallyAndWithContinuousIntegrationProperties_ShouldGiveTheSameVerdict(string local, string ci)
    {
        // Given
        var evidence = await Collected();
        evidence.Consumer.Authored().Select(evidence.Consumer.Text).Should()
            .NotContain(static text => text.Contains("ContinuousIntegrationBuild", StringComparison.Ordinal)
                || text.Contains("$(CI)", StringComparison.Ordinal) || text.Contains("GITHUB_ACTIONS", StringComparison.Ordinal), evidence.Because);

        // When
        var (localBuild, ciBuild) = (evidence.Builds[local], evidence.Builds[ci]);

        // Then
        (ciBuild.ExitCode == 0).Should().Be(localBuild.ExitCode == 0, evidence.Because);
        Diagnostics(ciBuild.Output).Should().Equal(Diagnostics(localBuild.Output), evidence.Because);
    }

    public sealed class ConsumerProjects : TheoryData<string>
    {
        public ConsumerProjects()
        {
            Add(ProductionProject);
            Add(TestProject);
        }
    }

    public sealed class CaseOutcomes : TheoryData<string, string[]>
    {
        public CaseOutcomes()
        {
            Add(PostingFact, ["passed"]);
            Add(PostingTheory, ["passed", "passed", "passed"]);
            Add(SkippedFact, ["skipped"]);
            Add(ReversalFact, ["passed"]);
            Add(FailingFact, ["failed"]);
        }
    }

    public sealed class ScopedOwnership : TheoryData<string, string[], bool>
    {
        public ScopedOwnership()
        {
            Add(PostingFact, [], true);
            Add(PostingTheory, [], true);
            Add(ReversalFact, [], true);
            Add(SkippedFact, ["pending"], false);
            Add(FailingFact, ["pending"], false);
        }
    }

    public sealed class MissingRelationships : TheoryData<string, string>
    {
        public MissingRelationships()
        {
            Add("missing-implements", "Postings.Post");
            Add("missing-verifies-rebuild", "PostingTests.PostingAddsTheAmount");
        }
    }

    public sealed class MissingInputs : TheoryData<string>
    {
        public MissingInputs()
        {
            Add("missing-manifest");
            Add("missing-claim-file");
        }
    }

    public sealed class ContinuousIntegrationPairs : TheoryData<string, string>
    {
        public ContinuousIntegrationPairs()
        {
            Add("clean", "clean-ci");
            Add("missing-implements", "missing-implements-ci");
        }
    }

    public sealed record BuildStep(int ExitCode, string Output);

    public sealed record ConsumerEvidence(IReadOnlyDictionary<string, JsonElement?> Artifacts, IReadOnlyDictionary<string, BuildStep> Builds,
        IReadOnlyDictionary<string, EvidenceResult> Reconciliations, IReadOnlyDictionary<string, string[]> Inputs, ConsumerFiles Consumer,
        string Because)
    {
        public JsonElement Artifact(string step)
        {
            var artifact = Artifacts.GetValueOrDefault(step);
            artifact.Should().NotBeNull(Because);
            return artifact!.Value;
        }
    }

    public sealed record ConsumerFiles(string Root)
    {
        public string Text(string relative) => File.ReadAllText(Path.Combine(Root, relative));

        public IEnumerable<string> Authored() => Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories)
            .Where(file => !Path.GetRelativePath(Root, file).Split(Path.DirectorySeparatorChar)
                .Any(static part => part is "bin" or "obj" or "packages" or ".probe"))
            .Select(file => Path.GetRelativePath(Root, file).Replace('\\', '/'));
    }

    public sealed class ConsumerRun : IAsyncDisposable
    {
        public ConsumerRun()
        {
            var work = Path.Combine(_root, ".artifacts", "gherkin-vstest", Guid.NewGuid().ToString("N"));
            _probe = Path.Combine(work, "probe.txt");
            _feed = Path.Combine(work, "feed");
            _evidence = new Lazy<Task<ConsumerEvidence>>(Collect);
        }

        public Task<ConsumerEvidence> Evidence => _evidence.Value;

        public ValueTask DisposeAsync()
        {
            foreach (var directory in new[] { _consumer, _feed })
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }

            return ValueTask.CompletedTask;
        }

        private static async Task<int> Run(StringBuilder log, string directory, IReadOnlyDictionary<string, string>? environment,
            params string[] arguments)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            foreach (var argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            foreach (var (name, value) in environment ?? new Dictionary<string, string>())
            {
                process.StartInfo.Environment[name] = value;
            }

            process.Start();
            var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            log.AppendLine($"== dotnet {string.Join(' ', arguments)} exited {process.ExitCode}").AppendLine(await stdout + await stderr);
            return process.ExitCode;
        }

        private static string FindRepositoryRoot()
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "specht.slnx")))
                {
                    return directory.FullName;
                }
            }

            throw new DirectoryNotFoundException("Could not locate the repository root.");
        }

        private async Task<ConsumerEvidence> Collect()
        {
            var log = new StringBuilder();
            var artifacts = new Dictionary<string, JsonElement?>(StringComparer.Ordinal);
            var builds = new Dictionary<string, BuildStep>(StringComparer.Ordinal);
            var reconciliations = new Dictionary<string, EvidenceResult>(StringComparer.Ordinal);
            var inputs = new Dictionary<string, string[]>(StringComparer.Ordinal);
            try
            {
                await Steps(log, artifacts, builds, reconciliations, inputs);
            }
            finally
            {
                foreach (var (name, result) in reconciliations)
                {
                    log.AppendLine($"== reconciliation {name}: exit {result.ExitCode}").AppendLine(result.Report.ToJsonString());
                }

                Directory.CreateDirectory(Path.GetDirectoryName(_probe)!);
                await File.WriteAllTextAsync(_probe, Relativized(log.ToString()), TestContext.Current.CancellationToken);
            }

            var because = "the Gherkin VSTest consumer run is recorded in " + Path.GetRelativePath(_root, _probe).Replace('\\', '/');
            return new ConsumerEvidence(artifacts, builds, reconciliations, inputs, new ConsumerFiles(_consumer), because);
        }

        private async Task Steps(StringBuilder log, Dictionary<string, JsonElement?> artifacts, Dictionary<string, BuildStep> builds,
            Dictionary<string, EvidenceResult> reconciliations, Dictionary<string, string[]> inputs)
        {
            Directory.CreateDirectory(_feed);
            var pack = await Run(log, _root, null, "pack", "src/codeanalysis/codeanalysis.csproj", "--configuration", "Debug", "--output", _feed,
                "--no-restore");
            pack.Should().Be(0, "the analyzer package must build before the consumer is arranged");
            var package = Directory.GetFiles(_feed, "Specht.CodeAnalysis.*.nupkg").Should().ContainSingle().Subject;
            await Arrange(log, Path.GetFileNameWithoutExtension(package)["Specht.CodeAnalysis.".Length..]);

            await Run(log, _consumer, null, "restore", TestProject, "--packages", Path.Combine(_consumer, "packages"));
            foreach (var project in new[] { ProductionProject, TestProject })
            {
                inputs[project] = await Inputs(log, project);
            }

            builds["clean-ci"] = await Step(log, ContinuousIntegration, "build", TestProject, "--no-restore", "-p:ContinuousIntegrationBuild=true");
            builds["clean"] = await Step(log, null, "build", TestProject, "--no-restore");
            reconciliations["empty"] = Reconcile([]);
            artifacts["list"] = await Discover(log);
            artifacts["run"] = await Execute(log);
            reconciliations["run"] = Reconcile([EvidenceFolder]);
            builds["rebuild"] = await Step(log, null, "build", TestProject, "--no-restore");

            await Mutate(PostingSource, static text => text + "\n", async () =>
            {
                reconciliations["stale"] = Reconcile([EvidenceFolder]);
                builds["stale"] = await Step(log, null, "build", TestProject, "--no-restore");
            });

            await Mutate(Manifest, static text => text.Replace(", \"requiredCaseScope\": \"not @pending\"", "", StringComparison.Ordinal), () =>
                Mutate(PendingSource, static text => text
                    .Replace("    [Fact(Skip", "    /// <verifies feature=\"ledger\" claim=\"REQ-LED-001\" />\n    [Fact(Skip", StringComparison.Ordinal)
                    .Replace("    [Fact]\n", "    /// <verifies feature=\"ledger\" claim=\"REQ-LED-002\" />\n    [Fact]\n", StringComparison.Ordinal),
                async () =>
                {
                    await Step(log, null, "build", TestProject, "--no-restore");
                    await Discover(log);
                    artifacts["unscoped-run"] = await Execute(log);
                    reconciliations["unscoped"] = Reconcile([EvidenceFolder]);
                }));

            await Mutate(PostingSource, static text => Regex.Replace(text,
                "    /// <verifies feature=\"ledger\" claim=\"REQ-LED-001\" />\\n(    \\[Fact\\]\\n    public void PostingAddsTheAmount)", "$1"), async () =>
            {
                builds["missing-verifies-build"] = await Step(log, null, "build", TestProject, "--no-restore");
                await Discover(log);
                await Execute(log);
                builds["missing-verifies-rebuild"] = await Step(log, null, "build", TestProject, "--no-restore");
            });

            await Mutate(ProductionSource, static text => text.Replace(
                "    /// <implements feature=\"ledger\" claim=\"REQ-LED-001\" />\n    public static int Post", "    public static int Post",
                StringComparison.Ordinal), async () =>
            {
                builds["missing-implements"] = await Step(log, null, "build", ProductionProject, "--no-restore");
                builds["missing-implements-ci"] = await Step(log, ContinuousIntegration, "build", ProductionProject, "--no-restore",
                    "-p:ContinuousIntegrationBuild=true");
            });

            await Mutate(ProductionSource, static text => text.Replace("claim=\"REQ-LED-002\"", "claim=\"REQ-LED-009\"", StringComparison.Ordinal),
                async () => builds["unknown-claim"] = await Step(log, null, "build", ProductionProject, "--no-restore"));
            await Mutate(ClaimFile, null, async () => builds["missing-claim-file"] = await Step(log, null, "build", ProductionProject, "--no-restore"));
            await Mutate(Manifest, null, async () => builds["missing-manifest"] = await Step(log, null, "build", ProductionProject, "--no-restore",
                $"-p:SpechtRoot={_consumer}"));
        }

        private async Task Arrange(StringBuilder log, string version)
        {
            var fixture = Path.Combine(_root, "test", "gherkin.vstest.fixture");
            foreach (var file in Directory.GetFiles(fixture, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(fixture, file);
                if (relative.Split(Path.DirectorySeparatorChar).Any(static part => part is "bin" or "obj"))
                {
                    continue;
                }

                var target = Path.Combine(_consumer, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target);
            }

            var central = await File.ReadAllTextAsync(Path.Combine(_root, "Directory.Packages.props"), TestContext.Current.CancellationToken);
            var versions = string.Join("\n", new[] { "Microsoft.NET.Test.Sdk", "xunit", "xunit.runner.visualstudio" }
                .Select(package => (Package: package, Match: Regex.Match(central, $"<PackageVersion Include=\"{Regex.Escape(package)}\" Version=\"([^\"]+)\"")))
                .Select(static pair => $"    <PackageVersion Include=\"{pair.Package}\" Version=\"{pair.Match.Groups[1].Value}\" />"));
            await Write("Directory.Packages.props", $"""
                <Project>
                  <PropertyGroup>
                    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                  </PropertyGroup>
                  <ItemGroup>
                    <GlobalPackageReference Include="Specht.CodeAnalysis" Version="$(SpechtCodeAnalysisPackageVersion)" />
                {versions}
                  </ItemGroup>
                </Project>
                """);
            using var sdk = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(_root, "global.json")));
            await Write("global.json", $$"""
                {
                  "sdk": {{sdk.RootElement.GetProperty("sdk").GetRawText()}},
                  "test": { "runner": "VSTest" }
                }
                """);
            await Write("nuget.config", $"""
                <?xml version="1.0" encoding="utf-8"?>
                <configuration>
                  <packageSources>
                    <clear />
                    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
                    <add key="fixture" value="{_feed}" />
                  </packageSources>
                  <packageSourceMapping>
                    <clear />
                    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
                    <packageSource key="fixture"><package pattern="Specht.CodeAnalysis" /></packageSource>
                  </packageSourceMapping>
                </configuration>
                """);
            await Write("Directory.Build.props", $"""
                <Project>
                  <PropertyGroup>
                    <SpechtCodeAnalysisPackageVersion>{version}</SpechtCodeAnalysisPackageVersion>
                    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
                  </PropertyGroup>
                </Project>
                """);
            var result = Path.Combine(_consumer, ".probe", "bundled-framework.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(result)!);
            await Run(log, _consumer, null, "msbuild", ProductionProject, "-getProperty:BundledNETCoreAppTargetFrameworkVersion",
                $"-getResultOutputFile:{result}", "-nologo");
            var framework = "net" + (await File.ReadAllTextAsync(result, TestContext.Current.CancellationToken)).Trim();
            await Write(Manifest, $$"""
                {
                  "revision": "ledger-r1",
                  "grammars": { "feature": "^[a-z]+$", "claim": "^REQ-LED-[0-9]{3}$" },
                  "lifecycle": { "approved": "approved", "withdrawn": "Withdrawn", "superseded": "Superseded" },
                  "claim": { "tag": "@{claim}", "tagIgnoreCase": false },
                  "codeAnalysis": {
                    "projects": [
                      { "path": "{{ProductionProject}}", "targetFrameworks": ["{{framework}}"] },
                      { "path": "{{TestProject}}", "targetFrameworks": ["{{framework}}"], "requiredCaseScope": "not @pending" }
                    ],
                    "features": [{ "feature": "ledger" }],
                    "requiredLinks": { "test": true },
                    "enforcement": [
                      { "project": "{{ProductionProject}}", "include": ["src/Ledger/**/*.cs"], "exclude": [],
                        "role": "implements", "declarationKinds": ["type", "constructor", "method"] },
                      { "project": "{{TestProject}}", "include": ["test/**/*.cs"], "exclude": ["{{PendingSource}}"],
                        "role": "verifies", "declarationKinds": ["testMethod"] }
                    ],
                    "records": {
                      "claimSource": "gherkin",
                      "claimFileSets": [{ "glob": ".spec/features/ledger/*.feature", "feature": "ledger", "record": "{{Record}}" }]
                    },
                    "mappings": {
                      "/schemaVersion": "/revision",
                      "/identifiers": "/grammars",
                      "/markers": "/lifecycle",
                      "/claim": "/claim"
                    }
                  }
                }
                """);
            await Write(".specht/schema/frontmatter/feature.schema.json", """
                {"type":"object","required":["id","spec_status"],"properties":{"id":{"type":"string"},"spec_status":{"type":"string"}}}
                """);
            await Write(Record, """
                ---
                id: ledger
                spec_status: approved
                ---
                # Ledger

                The ledger's claims are its scenarios, under features/ledger.
                """);
            await Write(ClaimFile, """
                Feature: Ledger

                  @REQ-LED-001
                  Scenario: A posting adds its amount to the balance
                    Given a balance of 1
                    When 2 is posted
                    Then the balance is 3

                  @REQ-LED-002
                  Scenario Outline: A reversal subtracts its amount from the balance
                    Given a balance of <balance>
                    When <amount> is reversed
                    Then the balance is <result>

                    Examples:
                      | balance | amount | result |
                      | 3       | 1      | 2      |
                      | 5       | 5      | 0      |
                """);
        }

        private async Task<string[]> Inputs(StringBuilder log, string project)
        {
            var result = Path.Combine(_consumer, ".probe", Path.GetFileNameWithoutExtension(project) + "-inputs.json");
            await Run(log, _consumer, null, "msbuild", project, "-getItem:AdditionalFiles", $"-getResultOutputFile:{result}", "-nologo");
            var text = File.Exists(result) ? await File.ReadAllTextAsync(result, TestContext.Current.CancellationToken) : "{}";
            log.AppendLine($"== AdditionalFiles of {project}").AppendLine(text);
            using var document = JsonDocument.Parse(text);
            return document.RootElement.TryGetProperty("Items", out var items) && items.TryGetProperty("AdditionalFiles", out var files)
                ? files.EnumerateArray().Where(static file => file.TryGetProperty("SpechtInputKind", out _))
                    .Select(static file => file.GetProperty("SpechtInputKind").GetString() + "|" + file.GetProperty("SpechtRelativePath").GetString())
                    .ToArray()
                : [];
        }

        private async Task<BuildStep> Step(StringBuilder log, IReadOnlyDictionary<string, string>? environment, params string[] arguments)
        {
            var before = log.Length;
            var exit = await Run(log, _consumer, environment, arguments);
            return new BuildStep(exit, log.ToString(before, log.Length - before));
        }

        private async Task<JsonElement?> Discover(StringBuilder log)
        {
            await Run(log, _consumer, null, "test", TestProject, "--no-build", "--list-tests");
            return Artifact(log);
        }

        private async Task<JsonElement?> Execute(StringBuilder log)
        {
            await Run(log, _consumer, null, "test", TestProject, "--no-build");
            return Artifact(log);
        }

        private EvidenceResult Reconcile(IReadOnlyList<string> paths) =>
            new EvidenceReconciler(new SpechtDocumentParser(new FrontmatterReader())).Reconcile(_consumer, paths, null);

        private async Task Mutate(string relative, Func<string, string>? change, Func<Task> steps)
        {
            var path = Path.Combine(_consumer, relative);
            var original = await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);
            if (change is null)
            {
                File.Delete(path);
            }
            else
            {
                var changed = change(original);
                changed.Should().NotBe(original, "the mutation of {0} must change it", relative);
                await File.WriteAllTextAsync(path, changed, TestContext.Current.CancellationToken);
            }

            try
            {
                await steps();
            }
            finally
            {
                await File.WriteAllTextAsync(path, original, TestContext.Current.CancellationToken);
            }
        }

        private async Task Write(string relative, string content)
        {
            var path = Path.Combine(_consumer, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path, content, TestContext.Current.CancellationToken);
        }

        private JsonElement? Artifact(StringBuilder log)
        {
            var folder = Path.Combine(_consumer, EvidenceFolder);
            var files = Directory.Exists(folder) ? Directory.GetFiles(folder, "*.json") : [];
            log.AppendLine($"== artifact: {files.Length} file(s) under {EvidenceFolder}");
            if (files.Length != 1)
            {
                return null;
            }

            var text = File.ReadAllText(files[0]);
            log.AppendLine(text);
            using var document = JsonDocument.Parse(text);
            return document.RootElement.Clone();
        }

        private string Relativized(string text)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            foreach (var (path, placeholder) in new[] { (_consumer, "<consumer>"), (_root, "<repository>"), (home, "<home>") })
            {
                foreach (var form in new[] { "/private" + path, path, path.Replace('\\', '/') })
                {
                    text = text.Replace(form, placeholder, StringComparison.Ordinal);
                }
            }

            return text;
        }

        private readonly string _root = FindRepositoryRoot();
        private readonly string _consumer = Path.Combine(Path.GetTempPath(), "specht-gherkin-consumer-" + Guid.NewGuid().ToString("N"));
        private readonly string _probe;
        private readonly string _feed;
        private readonly Lazy<Task<ConsumerEvidence>> _evidence;
    }

    private static string[] CaseIds(JsonElement artifact, string symbolId) =>
        Cases(artifact, symbolId).Select(static record => record.GetProperty("caseId").GetString()!).ToArray();

    private static JsonElement[] Cases(JsonElement artifact, string symbolId) =>
        artifact.GetProperty("discovery").GetProperty("cases").EnumerateArray()
            .Where(record => record.GetProperty("methodSymbolId").GetString() == symbolId).ToArray();

    private static string? Outcome(JsonElement artifact, string caseId) =>
        artifact.GetProperty("results").GetProperty("cases").EnumerateArray()
            .Where(result => result.GetProperty("caseId").GetString() == caseId)
            .Select(static result => result.GetProperty("outcome").GetString())
            .SingleOrDefault();

    private static string[] Diagnostics(string output) => DiagnosticPattern().Matches(output)
        .Select(static match => match.Groups["id"].Value + "|" + Path.GetFileName(match.Groups["file"].Value) + "|" + match.Groups["line"].Value)
        .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();

    [GeneratedRegex("(?:(?<file>[^\\s()]+)\\((?<line>\\d+),\\d+(?:,\\d+,\\d+)?\\))?\\s*:\\s*(?:error|warning) (?<id>SPCT\\d{4})")]
    private static partial Regex DiagnosticPattern();

    private async Task<ConsumerEvidence> Collected()
    {
        if (!AppContext.TargetFrameworkName!.EndsWith("v10.0", StringComparison.Ordinal))
        {
            Assert.Skip("The Gherkin VSTest consumer fixture runs once against the repository's .NET 10 consumer runtime.");
        }

        return await run.Evidence;
    }

    private const string ProductionProject = "src/Ledger/Ledger.csproj";
    private const string TestProject = "test/Ledger.Tests/Ledger.Tests.csproj";
    private const string ProductionSource = "src/Ledger/Postings.cs";
    private const string PostingSource = "test/Ledger.Tests/PostingTests.cs";
    private const string PendingSource = "test/shared/PendingTests.cs";
    private const string Manifest = ".specht/manifest.json";
    private const string Record = ".spec/README.md";
    private const string ClaimFile = ".spec/features/ledger/ledger.feature";
    private const string EvidenceFolder = "test/Ledger.Tests/obj/specht-test-evidence";
    private const string PostingFact = "M:Consumer.Ledger.Tests.PostingTests.PostingAddsTheAmount";
    private const string PostingTheory = "M:Consumer.Ledger.Tests.PostingTests.PostingAddsEachRowsAmount(System.Int32,System.Int32,System.Int32)";
    private const string SkippedFact = "M:Consumer.Ledger.Tests.PendingTests.PostingOfAForeignAmountIsPending";
    private const string ReversalFact = "M:Consumer.Ledger.Tests.ReversalTests.ReversalSubtractsTheAmount";
    private const string FailingFact = "M:Consumer.Ledger.Tests.PendingTests.ReversalBelowZeroIsPending";

    private static readonly IReadOnlyDictionary<string, string> ContinuousIntegration = new Dictionary<string, string> { ["CI"] = "true" };
}
