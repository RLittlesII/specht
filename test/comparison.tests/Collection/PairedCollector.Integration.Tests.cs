using System.Collections.Immutable;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Specht.Comparison.Tests.Evidence;

namespace Specht.Comparison.Tests.Collection;

[Trait("Tier", "Integration")]
public sealed class PairedCollectorIntegrationTests : IDisposable
{
    public PairedCollectorIntegrationTests()
    {
        Directory.CreateDirectory(_root);
        _processes = new ScriptedRevisions(_root);
    }

    [Fact]
    public async Task PairedPlan_WhenCollecting_ShouldRunEachPinnedRevisionInTurnWithItsOwnHarness()
    {
        // Given
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        const string Previous = ".artifacts/benchmarks/comparison/session-1/previous/source";
        const string Current = ".artifacts/benchmarks/comparison/session-1/current/source";
        const string Build = "dotnet build .performance/benchmarks/benchmarks.csproj --configuration Release --framework net10.0 --disable-build-servers";
        const string Run = "dotnet run --project .performance/benchmarks/benchmarks.csproj --configuration Release --framework net10.0 --no-build --";
        const string Pack = "dotnet pack src/tool/tool.csproj --configuration Release --output .artifacts/nupkg --disable-build-servers";
        _processes.Requests.Select(static request => $"{request.WorkingDirectory}> {request.FileName} {string.Join(' ', request.Arguments)}")
            .Should().Equal(
                $".> git clone --quiet --no-checkout --shared . {Previous}",
                $"{Previous}> git checkout --quiet --detach 1111111111111111111111111111111111111111",
                $"{Previous}> git rev-parse HEAD",
                $"{Previous}> {Build}",
                $"{Previous}> {Run} --filter Specht.Benchmarks.* --artifacts ../results --job Short --list flat",
                $"{Previous}> {Pack}",
                $"{Previous}> {Run} --filter Specht.Benchmarks.* --artifacts ../results --job Short",
                $".> git clone --quiet --no-checkout --shared . {Current}",
                $"{Current}> git checkout --quiet --detach 2222222222222222222222222222222222222222",
                $"{Current}> git rev-parse HEAD",
                $"{Current}> {Build}",
                $"{Current}> {Run} --filter Specht.Benchmarks.* --artifacts ../results --job Short --list flat",
                $"{Current}> {Pack}",
                $"{Current}> {Run} --filter Specht.Benchmarks.* --artifacts ../results --job Short");
        _processes.MostConcurrent.Should().Be(1);
    }

    [Fact]
    public async Task PairedPlan_WhenCollected_ShouldRecordOneSessionWithEachRevisionsEnvironment()
    {
        // Given
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        result.Session.Should().BeEquivalentTo(new
        {
            FormatVersion = 1,
            PairDigest = new string('b', 64),
            ComparatorCommit = new string('c', 40),
            ComparatorVersion = "1",
            RunId = "run-1",
            Attempt = 1,
            SessionId = "session-1",
            RunnerIdentity = "runner-1",
        });
        result.Session.Previous.ProductCommit.Should().Be(new string('1', 40));
        result.Session.Current.ProductCommit.Should().Be(new string('2', 40));
        foreach (var side in new[] { result.Session.Previous, result.Session.Current })
        {
            side.Conditions.Should().Be(new MeasurementConditions("Synthetic OS 1.0", ".NET 10.0.0", "X64", "11.0.100-synthetic.1", "Job default",
                "ShortRun(Toolchain=.NET 10.0, IterationCount=3, LaunchCount=1, WarmupCount=3)"));
            side.ExporterVersion.Should().Be("0.15.8");
            side.Invocation.Should().Equal("--filter", "Specht.Benchmarks.*", "--artifacts", "../results", "--job", "Short");
            side.InventoryKnown.Should().BeFalse();
        }

        result.Previous.Side.Should().Be("previous");
        result.Previous.Status.Should().Be(CollectionStatus.Collected);
        result.Previous.Reason.Should().BeNull();
        result.Previous.ResultFiles.Should().Equal(
            ".artifacts/benchmarks/comparison/session-1/previous/results/results/Specht.Benchmarks.Engine.CheckBenchmarks-report-full.json",
            ".artifacts/benchmarks/comparison/session-1/previous/results/results/Specht.Benchmarks.Startup.ColdStartBenchmarks-report-full.json");
        result.Previous.Evidence.Should().BeEquivalentTo(result.Session.Previous);
        result.Current.Side.Should().Be("current");
        result.Current.Status.Should().Be(CollectionStatus.Collected);
        result.Current.ResultFiles.Should().Equal(
            ".artifacts/benchmarks/comparison/session-1/current/results/results/Specht.Benchmarks.Engine.CheckBenchmarks-report-full.json",
            ".artifacts/benchmarks/comparison/session-1/current/results/results/Specht.Benchmarks.Startup.ColdStartBenchmarks-report-full.json");
        result.Current.Evidence.Should().BeEquivalentTo(result.Session.Current);
        (await File.ReadAllTextAsync(Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/session-manifest.json"),
            TestContext.Current.CancellationToken)).Should().Be(ComparisonJson.Write(result.Session));
    }

    [Fact]
    public async Task PairedPlan_WhenCollected_ShouldRecordOnlyRepositoryRelativePaths()
    {
        // Given
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        var recorded = _processes.Requests.SelectMany(static request => request.Arguments.Append(request.WorkingDirectory))
            .Concat(result.Previous.ResultFiles).Concat(result.Current.ResultFiles)
            .Concat(result.Session.Previous.Invocation).Concat(result.Session.Current.Invocation)
            .ToArray();
        recorded.Should().NotBeEmpty().And.NotContain(static value => Path.IsPathRooted(value));
        recorded.Should().NotContain(value => value.Contains(_root, StringComparison.Ordinal));
        (await File.ReadAllTextAsync(Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/session-manifest.json"),
            TestContext.Current.CancellationToken)).Should().NotContain(_root).And.NotContain(Path.GetTempPath());
    }

    [Theory]
    [ClassData(typeof(PackSelections))]
    public async Task SelectedCases_WhenCollecting_ShouldPackTheToolOncePerRevisionOnlyForStartupCases(string[] listed, int packs)
    {
        // Given
        _processes.Listed["previous"] = listed;
        _processes.Listed["current"] = listed;
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        var packed = _processes.Requests.Where(static request => request.Arguments.FirstOrDefault() == "pack").ToArray();
        packed.Count(static request => ScriptedRevisions.SideOf(request) == "previous").Should().Be(packs);
        packed.Count(static request => ScriptedRevisions.SideOf(request) == "current").Should().Be(packs);
    }

    [Fact]
    public async Task PlanWithoutJob_WhenCollecting_ShouldOmitTheJobArgument()
    {
        // Given
        CollectionPlan plan = new CollectionPlanFixture().WithJob(null);

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        _processes.Requests.Should().NotBeEmpty().And.NotContain(static request => request.Arguments.Contains("--job"));
        result.Session.Previous.Invocation.Should().Equal("--filter", "Specht.Benchmarks.*", "--artifacts", "../results");
        result.Session.Current.Invocation.Should().Equal("--filter", "Specht.Benchmarks.*", "--artifacts", "../results");
    }

    [Fact]
    public async Task ParentWithCredentials_WhenCollecting_ShouldGiveEachChildOnlyTheAllowlistAndItsRevisionWorkspace()
    {
        // Given
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        _processes.Requests.Should().NotBeEmpty();
        foreach (var request in _processes.Requests)
        {
            var workspace = $".artifacts/benchmarks/comparison/session-1/{ScriptedRevisions.SideOf(request)}";
            request.Environment.Keys.Should().BeEquivalentTo(
                "DOTNET_CLI_HOME", "DOTNET_CLI_TELEMETRY_OPTOUT", "DOTNET_NOLOGO", "DOTNET_ROOT", "DOTNET_ROOT_ARM64", "HOME", "LANG", "LC_ALL",
                "NUGET_PACKAGES", "PATH", "SystemRoot", "TEMP", "TMP", "TMPDIR", "USERPROFILE");
            request.Environment.Should().Contain(new Dictionary<string, string>
            {
                ["PATH"] = "synthetic-path",
                ["DOTNET_ROOT"] = "synthetic-dotnet-root",
                ["DOTNET_ROOT_ARM64"] = "synthetic-dotnet-root-arm64",
                ["LANG"] = "en_US.UTF-8",
                ["LC_ALL"] = "C",
                ["SystemRoot"] = "synthetic-system-root",
                ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1",
                ["DOTNET_NOLOGO"] = "1",
            });
            foreach (var key in new[] { "HOME", "USERPROFILE", "DOTNET_CLI_HOME" })
            {
                Path.GetFullPath(request.Environment[key], _root).Should().Be(Path.GetFullPath($"{workspace}/home", _root));
            }

            foreach (var key in new[] { "TMPDIR", "TMP", "TEMP" })
            {
                Path.GetFullPath(request.Environment[key], _root).Should().Be(Path.GetFullPath($"{workspace}/temp", _root));
            }

            Path.GetFullPath(request.Environment["NUGET_PACKAGES"], _root)
                .Should().Be(Path.GetFullPath(".artifacts/benchmarks/comparison/session-1/nuget", _root));
            request.Environment.Values.Should().NotContain(static value => value.Contains("secret", StringComparison.Ordinal));
        }
    }

    [Theory]
    [ClassData(typeof(UnusablePreviousRevisions))]
    public async Task UnusablePreviousRevision_WhenCollecting_ShouldRecordItsReasonAndStillCollectTheCurrent(string failure, string reason)
    {
        // Given
        Break(failure);
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        result.Previous.Status.Should().Be(CollectionStatus.Unavailable);
        result.Previous.Reason.Should().Be(reason);
        result.Previous.Evidence.ProductCommit.Should().Be(new string('1', 40));
        result.Previous.Evidence.Conditions.Should().Be(new MeasurementConditions(null, null, null, null, null, null));
        result.Current.Status.Should().Be(CollectionStatus.Collected);
        _processes.Requests.Where(static request => request.Arguments.FirstOrDefault() == "checkout")
            .Select(static request => request.Arguments[^1]).Should().Equal(new string('1', 40), new string('2', 40));
        Directory.Exists(Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/previous/source")).Should().BeFalse();
    }

    [Theory]
    [ClassData(typeof(FailingPreviousSteps))]
    public async Task FailingPreviousStep_WhenCollecting_ShouldRecordASanitisedReasonAndStillCollectTheCurrent(string failure, string[] checkouts)
    {
        // Given
        Break(failure);
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        result.Previous.Status.Should().Be(CollectionStatus.Unavailable);
        result.Previous.Reason.Should().NotBeNullOrWhiteSpace().And.NotContain("secret-detail").And.NotContain(_root);
        result.Previous.Evidence.ProductCommit.Should().Be(new string('1', 40));
        result.Previous.Evidence.Conditions.Should().Be(new MeasurementConditions(null, null, null, null, null, null));
        result.Current.Status.Should().Be(CollectionStatus.Collected);
        _processes.Requests.Where(static request => request.Arguments.FirstOrDefault() == "checkout")
            .Select(static request => request.Arguments[^1]).Should().Equal(checkouts);
        Directory.Exists(Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/previous/source")).Should().BeFalse();
    }

    [Fact]
    public async Task RevisionWithoutHarness_WhenCollecting_ShouldNeverBuildOrRunThatRevision()
    {
        // Given
        _processes.WithoutHarness.Add("previous");
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        _processes.Requests.Where(static request => ScriptedRevisions.SideOf(request) == "previous")
            .Should().NotContain(static request => request.FileName == "dotnet");
        File.Exists(Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/previous/source/.performance/benchmarks/benchmarks.csproj"))
            .Should().BeFalse();
    }

    [Fact]
    public async Task CollectedPair_WhenFinished_ShouldRemoveSourcesAndCachesAndKeepResults()
    {
        // Given
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        foreach (var side in new[] { "previous", "current" })
        {
            var workspace = Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1", side);
            Directory.Exists(Path.Combine(workspace, "source")).Should().BeFalse();
            Directory.Exists(Path.Combine(workspace, "home")).Should().BeFalse();
            Directory.Exists(Path.Combine(workspace, "temp")).Should().BeFalse();
            File.Exists(Path.Combine(workspace, "results/results/Specht.Benchmarks.Engine.CheckBenchmarks-report-full.json")).Should().BeTrue();
        }
    }

    [Fact]
    public async Task ParameterisedIdentityMappings_WhenCollecting_ShouldKnowARevisionsInventoryOnlyWhenEachSelectedNameHasAMapping()
    {
        // Given
        _processes.Declarations["previous"] = [(CheckFile, SyntheticBenchmarks.Check), (StartupFile, SyntheticBenchmarks.Startup)];
        _processes.Declarations["current"] = [(CheckFile, SyntheticBenchmarks.Check)];
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        result.Session.Previous.InventoryKnown.Should().BeTrue();
        result.Previous.InventoryReason.Should().BeNull();
        result.Session.Previous.ExpectedCases
            .Select(static workload => $"{workload.OperationId}({string.Join(',', workload.Parameters.Values.Select(static value => value.Value))})")
            .Should().Equal("check(1)", "check(10)", "start()");
        result.Session.Current.InventoryKnown.Should().BeFalse();
        result.Current.InventoryReason.Should().Be("Revision benchmarks do not declare one workload identity for every selected benchmark.");
        result.Session.Current.ExpectedCases.Should().BeEmpty();
    }

    [Fact]
    public async Task RevisionsDeclaringTheirWorkloads_WhenCollecting_ShouldTakeEachRevisionsMappingsFromItsOwnDeclarations()
    {
        // Given
        var revised = SyntheticBenchmarks.Check.Replace("\"check\", \"1\"", "\"check\", \"2\"", StringComparison.Ordinal);
        _processes.Declarations["previous"] = [(CheckFile, SyntheticBenchmarks.Check), (StartupFile, SyntheticBenchmarks.Startup)];
        _processes.Declarations["current"] = [(CheckFile, revised), (StartupFile, SyntheticBenchmarks.Startup)];
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        var previousCheck = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(SyntheticBenchmarks.Check)));
        var currentCheck = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(revised)));
        result.Previous.Mappings.Should().HaveCount(3).And.OnlyContain(static mapping => mapping.ProductCommit == new string('1', 40));
        result.Current.Mappings.Should().HaveCount(3).And.OnlyContain(static mapping => mapping.ProductCommit == new string('2', 40));
        result.Previous.Mappings.Where(static mapping => mapping.Case.OperationId == "check")
            .Select(static mapping => (mapping.SourceFile, mapping.SourceDigest, mapping.Case.OperationRevision))
            .Should().AllBeEquivalentTo((CheckFile, previousCheck, "1"));
        result.Current.Mappings.Where(static mapping => mapping.Case.OperationId == "check")
            .Select(static mapping => (mapping.SourceFile, mapping.SourceDigest, mapping.Case.OperationRevision))
            .Should().AllBeEquivalentTo((CheckFile, currentCheck, "2"));
        result.Session.Previous.ExpectedCases.Where(static workload => workload.OperationId == "check")
            .Select(static workload => workload.OperationRevision).Should().Equal("1", "1");
        result.Session.Current.ExpectedCases.Where(static workload => workload.OperationId == "check")
            .Select(static workload => workload.OperationRevision).Should().Equal("2", "2");
    }

    [Fact]
    public async Task PreviousRevisionDeclaringNoWorkloadIdentity_WhenCollecting_ShouldLeaveItsInventoryUnknownWithoutTheCurrentRevisionsIdentities()
    {
        // Given
        _processes.Declarations["previous"] = [(CheckFile, SyntheticBenchmarks.Undeclared)];
        _processes.Declarations["current"] = [(CheckFile, SyntheticBenchmarks.Check), (StartupFile, SyntheticBenchmarks.Startup)];
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        result.Previous.Status.Should().Be(CollectionStatus.Collected);
        result.Previous.Mappings.Should().BeEmpty();
        result.Previous.InventoryReason.Should().Be("Revision benchmarks declare no workload identity.");
        result.Session.Previous.InventoryKnown.Should().BeFalse();
        result.Session.Previous.ExpectedCases.Should().BeEmpty();
        result.Current.Mappings.Should().NotBeEmpty().And.OnlyContain(static mapping => mapping.ProductCommit == new string('2', 40));
        result.Session.Current.InventoryKnown.Should().BeTrue();
    }

    [Fact]
    public async Task WorkloadIdentitiesWrittenOutsideTheRevision_WhenCollecting_ShouldNotBeUsed()
    {
        // Given
        _processes.Declarations["previous"] = [(CheckFile, SyntheticBenchmarks.Undeclared)];
        var output = Path.Combine(_root, ".performance", "benchmarks", "bin", "Release", "net10.0");
        Directory.CreateDirectory(output);
        Directory.CreateDirectory(Path.Combine(_root, ".performance", "benchmarks", "Check"));
        Directory.CreateDirectory(Path.Combine(_root, ".performance", "benchmarks", "Startup"));
        await File.WriteAllTextAsync(Path.Combine(_root, CheckFile), SyntheticBenchmarks.Check, TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(_root, StartupFile), SyntheticBenchmarks.Startup, TestContext.Current.CancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(output, "specht.benchmarks.dll"), SyntheticBenchmarks.Compile(
            (Path.Combine(_root, CheckFile), SyntheticBenchmarks.Check), (Path.Combine(_root, StartupFile), SyntheticBenchmarks.Startup)),
            TestContext.Current.CancellationToken);
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        result.Previous.Mappings.Should().BeEmpty();
        result.Previous.InventoryReason.Should().Be("Revision benchmarks declare no workload identity.");
        result.Session.Previous.InventoryKnown.Should().BeFalse();
        result.Current.Mappings.Should().BeEmpty();
        result.Session.Current.InventoryKnown.Should().BeFalse();
    }

    [Theory]
    [ClassData(typeof(UnexpectedFailures))]
    public async Task UnexpectedFailureInThePreviousRevision_WhenCollecting_ShouldRecordASanitisedReasonAndStillCollectTheCurrent(Exception failure)
    {
        // Given
        _processes.During = (side, step, _) => side == "previous" && step == "build" ? Task.FromException(failure) : Task.CompletedTask;
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        result.Previous.Status.Should().Be(CollectionStatus.Unavailable);
        result.Previous.Reason.Should().Be("Revision collection failed unexpectedly.");
        result.Current.Status.Should().Be(CollectionStatus.Collected);
        (await File.ReadAllTextAsync(Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/session-manifest.json"),
            TestContext.Current.CancellationToken)).Should().Be(ComparisonJson.Write(result.Session));
    }

    [Fact]
    public async Task CleanupFailure_WhenAFolderCannotBeRemoved_ShouldKeepTheCollectedResults()
    {
        // Given
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("The cleanup failure is arranged with a Unix file mode.");
        }

        _processes.During = (side, step, _) =>
        {
            if (OperatingSystem.IsWindows())
            {
                return Task.CompletedTask;
            }

            if (side != "previous" || step != "run")
            {
                return Task.CompletedTask;
            }

            var locked = Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/previous/source/locked");
            Directory.CreateDirectory(locked);
            File.WriteAllText(Path.Combine(locked, "held.txt"), "held");
            File.SetUnixFileMode(locked, UnixFileMode.UserRead | UnixFileMode.UserExecute);
            return Task.CompletedTask;
        };
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        result.Previous.Status.Should().Be(CollectionStatus.Collected);
        result.Current.Status.Should().Be(CollectionStatus.Collected);
        (await File.ReadAllTextAsync(Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/session-manifest.json"),
            TestContext.Current.CancellationToken)).Should().Be(ComparisonJson.Write(result.Session));
    }

    [Fact]
    public async Task LeftoverPreviousWorkspace_WhenCollecting_ShouldMarkItUnavailableWithoutRunningIt()
    {
        // Given
        var leftover = Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/previous/results/results");
        Directory.CreateDirectory(leftover);
        await File.WriteAllTextAsync(Path.Combine(leftover, "Specht.Benchmarks.Engine.CheckBenchmarks-report-full.json"), ExportData.CollectedFullJson,
            TestContext.Current.CancellationToken);
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        result.Previous.Status.Should().Be(CollectionStatus.Unavailable);
        result.Previous.Reason.Should().Be("Revision workspace already exists.");
        result.Previous.ResultFiles.Should().BeEmpty();
        _processes.Requests.Should().NotContain(static request => ScriptedRevisions.SideOf(request) == "previous");
        result.Current.Status.Should().Be(CollectionStatus.Collected);
    }

    [Fact]
    public async Task EarlierAttemptsManifest_WhenCollectingAgain_ShouldLeaveItUnchangedAndStillReturnTheResult()
    {
        // Given
        var manifest = Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/session-manifest.json");
        Directory.CreateDirectory(Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/previous/results"));
        await File.WriteAllTextAsync(manifest, "{ \"earlier\": \"attempt\" }\n", TestContext.Current.CancellationToken);
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        (await File.ReadAllTextAsync(manifest, TestContext.Current.CancellationToken)).Should().Be("{ \"earlier\": \"attempt\" }\n");
        result.Session.SessionId.Should().Be("session-1");
        result.Previous.Status.Should().Be(CollectionStatus.Unavailable);
        result.Current.Status.Should().Be(CollectionStatus.Collected);
    }

    [Fact]
    public async Task UnavailableRevision_WhenTheWrittenManifestIsRead_ShouldRoundTrip()
    {
        // Given
        Break("build exit");
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        result.Previous.Status.Should().Be(CollectionStatus.Unavailable);
        ComparisonJson.ReadSession(await File.ReadAllTextAsync(Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/session-manifest.json"),
            TestContext.Current.CancellationToken)).Should().BeEquivalentTo(result.Session);
    }

    [Fact(Timeout = 10_000)]
    public async Task HungPreviousRevision_WhenItsBoundExpires_ShouldMarkItUnavailableAndStillCollectTheCurrent()
    {
        // Given
        _processes.During = (side, step, token) =>
            side == "previous" && step == "run" ? Hang(TimeSpan.FromMinutes(26), token) : Task.CompletedTask;
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        result.Previous.Status.Should().Be(CollectionStatus.Unavailable);
        result.Previous.Reason.Should().Be("Revision collection exceeded its 25-minute bound.");
        result.Current.Status.Should().Be(CollectionStatus.Collected);
        Directory.Exists(Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/previous/source")).Should().BeFalse();
    }

    [Fact(Timeout = 10_000)]
    public async Task PairOverrunningItsTotalBound_WhenTheCurrentRevisionRuns_ShouldMarkTheCurrentUnavailable()
    {
        // Given
        _processes.During = (side, step, token) => (side, step) switch
        {
            ("previous", "run") => Overrun(TimeSpan.FromMinutes(40), token),
            ("current", "run") => Hang(TimeSpan.FromMinutes(21), token),
            _ => Task.CompletedTask,
        };
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, TestContext.Current.CancellationToken);

        // Then
        result.Previous.Status.Should().Be(CollectionStatus.Unavailable);
        result.Previous.Reason.Should().Be("Revision collection exceeded its 25-minute bound.");
        result.Current.Status.Should().Be(CollectionStatus.Unavailable);
        result.Current.Reason.Should().Be("Paired collection exceeded its 60-minute bound.");
        result.Current.Evidence.ProductCommit.Should().Be(new string('2', 40));
    }

    [Fact(Timeout = 10_000)]
    public async Task CallerCancellation_WhenCollecting_ShouldCancelTheChildAndReturnUnavailableRevisions()
    {
        // Given
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var observed = CancellationToken.None;
        _processes.During = (side, step, token) =>
        {
            if (side != "previous" || step != "run")
            {
                return Task.CompletedTask;
            }

            observed = token;
            caller.Cancel();
            return Hang(TimeSpan.Zero, token);
        };
        CollectionPlan plan = new CollectionPlanFixture();

        // When
        var result = await Collector().CollectAsync(plan, caller.Token);

        // Then
        observed.IsCancellationRequested.Should().BeTrue();
        result.Previous.Status.Should().Be(CollectionStatus.Unavailable);
        result.Previous.Reason.Should().Be("Collection was cancelled.");
        result.Current.Status.Should().Be(CollectionStatus.Unavailable);
        result.Current.Reason.Should().Be("Collection was cancelled.");
        var workspace = Path.Combine(_root, ".artifacts/benchmarks/comparison/session-1/previous");
        Directory.Exists(Path.Combine(workspace, "source")).Should().BeFalse();
        Directory.Exists(Path.Combine(workspace, "home")).Should().BeFalse();
        Directory.Exists(Path.Combine(workspace, "temp")).Should().BeFalse();
    }

    public void Dispose()
    {
        if (!Directory.Exists(_root))
        {
            return;
        }

        if (!OperatingSystem.IsWindows())
        {
            foreach (var directory in Directory.EnumerateDirectories(_root, "*", SearchOption.AllDirectories))
            {
                File.SetUnixFileMode(directory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }
        }

        Directory.Delete(_root, true);
    }

    public sealed class PackSelections : TheoryData<string[], int>
    {
        public PackSelections()
        {
            Add(["Specht.Benchmarks.Startup.ColdStartBenchmarks.Start", "Specht.Benchmarks.Engine.CheckBenchmarks.Check"], 1);
            Add(["Specht.Benchmarks.Engine.CheckBenchmarks.Check"], 0);
        }
    }

    public sealed class UnusablePreviousRevisions : TheoryData<string, string>
    {
        public UnusablePreviousRevisions()
        {
            Add("checkout mismatch", "Checked-out revision does not match the pinned commit.");
            Add("missing harness", "Revision has no native benchmark harness.");
            Add("build exit", "Benchmark build failed with exit code 1.");
            Add("empty selection", "No benchmarks selected for the filter.");
            Add("disagreeing runtimes", "Benchmark exports disagree on their measurement conditions.");
            Add("missing toolchain", "Benchmark export is malformed or names no exporter version.");
        }
    }

    public sealed class UnexpectedFailures : TheoryData<Exception>
    {
        public UnexpectedFailures()
        {
            Add(new Win32Exception(2, "secret-detail: dotnet was not found"));
            Add(new IOException("secret-detail: the disk is full"));
        }
    }

    public sealed class FailingPreviousSteps : TheoryData<string, string[]>
    {
        public FailingPreviousSteps()
        {
            Add("clone exit", ["2222222222222222222222222222222222222222"]);
            Add("checkout exit", ["1111111111111111111111111111111111111111", "2222222222222222222222222222222222222222"]);
            Add("list exit", ["1111111111111111111111111111111111111111", "2222222222222222222222222222222222222222"]);
            Add("pack exit", ["1111111111111111111111111111111111111111", "2222222222222222222222222222222222222222"]);
            Add("run exit", ["1111111111111111111111111111111111111111", "2222222222222222222222222222222222222222"]);
            Add("no exports", ["1111111111111111111111111111111111111111", "2222222222222222222222222222222222222222"]);
        }
    }

    private PairedCollector Collector() => new(new FileSystem(), _time, _processes.RunAsync, _root, Parent);

    private void Break(string failure)
    {
        switch (failure)
        {
            case "checkout mismatch":
                _processes.Heads["previous"] = new string('9', 40);
                break;
            case "missing harness":
                _processes.WithoutHarness.Add("previous");
                break;
            case "empty selection":
                _processes.Listed["previous"] = ["Other.Benchmarks.Engine.CheckBenchmarks.Check"];
                break;
            case "no exports":
                _processes.Exports["previous"] = [];
                break;
            case "disagreeing runtimes":
                Disagree(".NET 10.0.0", ".NET 10.0.1");
                break;
            case "missing toolchain":
                _processes.Exports["previous"] =
                [
                    ("Specht.Benchmarks.Engine.CheckBenchmarks-report-full.json",
                        ExportData.CollectedFullJson.Replace("\"DotNetCliVersion\": \"11.0.100-synthetic.1\", ", string.Empty, StringComparison.Ordinal)),
                ];
                break;
            default:
                var step = failure.Split(' ')[0];
                _processes.ExitCodes[("previous", step)] = step == "clone" ? 128 : 1;
                break;
        }
    }

    private void Disagree(string value, string other) => _processes.Exports["previous"] =
    [
        ("Specht.Benchmarks.Engine.CheckBenchmarks-report-full.json", ExportData.CollectedFullJson),
        ("Specht.Benchmarks.Startup.ColdStartBenchmarks-report-full.json", ExportData.CollectedFullJson.Replace(value, other, StringComparison.Ordinal)),
    ];

    private async Task Hang(TimeSpan elapsed, CancellationToken token)
    {
        _time.Advance(elapsed);
        var killed = new TaskCompletionSource();
        await using var registration = token.Register(() => killed.TrySetCanceled(token));
        await killed.Task;
    }

    private Task Overrun(TimeSpan elapsed, CancellationToken token)
    {
        _time.Advance(elapsed);
        token.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    private const string CheckFile = ".performance/benchmarks/Check/CheckBenchmarks.cs";
    private const string StartupFile = ".performance/benchmarks/Startup/ColdStartBenchmarks.cs";

    private static readonly Dictionary<string, string> Parent = new()
    {
        ["PATH"] = "synthetic-path",
        ["DOTNET_ROOT"] = "synthetic-dotnet-root",
        ["DOTNET_ROOT_ARM64"] = "synthetic-dotnet-root-arm64",
        ["LANG"] = "en_US.UTF-8",
        ["LC_ALL"] = "C",
        ["SystemRoot"] = "synthetic-system-root",
        ["HOME"] = "synthetic-parent-home",
        ["DOTNET_CLI_HOME"] = "synthetic-parent-cli-home",
        ["NUGET_PACKAGES"] = "synthetic-parent-packages",
        ["EDITOR"] = "vi",
        ["GITHUB_TOKEN"] = "ghs_synthetic-secret",
        ["GH_TOKEN"] = "gho_synthetic-secret",
        ["ACTIONS_RUNTIME_TOKEN"] = "runtime-synthetic-secret",
        ["ACTIONS_ID_TOKEN_REQUEST_TOKEN"] = "id-synthetic-secret",
        ["NuGetPackageSourceCredentials_specht"] = "Username=someone;Password=synthetic-secret",
    };

    private readonly string _root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    private readonly FakeTimeProvider _time = new();
    private readonly ScriptedRevisions _processes;
}
