using System.Collections.Immutable;
using AwesomeAssertions;
using Specht.Comparison.Tests.Evidence;

namespace Specht.Comparison.Tests.Collection;

[Trait("Tier", "Unit")]
public sealed class RevisionEvidenceUnitTests
{
    [Theory]
    [ClassData(typeof(Listings))]
    public void Listing_WhenSelecting_ShouldKeepTheHarnessNamesInListingOrder(string[] listing, string[] expected)
    {
        // Given
        ImmutableArray<string> lines = [.. listing];

        // When
        var selected = RevisionEvidence.Select(lines);

        // Then
        selected.Should().Equal(expected);
    }

    [Theory]
    [ClassData(typeof(ToolSelections))]
    public void Selection_WhenDecidingThePack_ShouldNeedTheToolOnlyForStartupNames(string[] selected, bool expected)
    {
        // Given
        ImmutableArray<string> names = [.. selected];

        // When
        var needed = RevisionEvidence.NeedsTool(names);

        // Then
        needed.Should().Be(expected);
    }

    [Fact]
    public void ParameterisedMappings_WhenTakingTheInventory_ShouldExpectEveryCaseOfEachSelectedNameInKeyOrder()
    {
        // Given
        ImmutableArray<IdentityMapping> mappings =
        [
            new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Startup.ColdStartBenchmarks.Start")
                .WithCase(new WorkloadCaseFixture().WithOperationId("start").WithParameters(ImmutableSortedDictionary<string, ParameterValue>.Empty)),
            new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 10)")
                .WithCase(new WorkloadCaseFixture().WithParameters(
                    ImmutableSortedDictionary<string, ParameterValue>.Empty.Add("Specifications", new ParameterValue("number", "10")))),
            new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 1)"),
            new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Engine.CheckBenchmarks.Unselected(Specifications: 1)")
                .WithCase(new WorkloadCaseFixture().WithOperationId("unselected")),
            new IdentityMappingFixture().WithProductCommit(new string('2', 40))
                .WithFullName("Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 100)")
                .WithCase(new WorkloadCaseFixture().WithParameters(
                    ImmutableSortedDictionary<string, ParameterValue>.Empty.Add("Specifications", new ParameterValue("number", "100")))),
        ];

        // When
        var inventory = RevisionEvidence.Inventory(mappings, new string('1', 40),
            ["Specht.Benchmarks.Engine.CheckBenchmarks.Check", "Specht.Benchmarks.Startup.ColdStartBenchmarks.Start"]);

        // Then
        inventory.Should().NotBeNull();
        inventory!.Value
            .Select(static workload => $"{workload.OperationId}({string.Join(',', workload.Parameters.Values.Select(static value => value.Value))})")
            .Should().Equal("check(1)", "check(10)", "start()");
    }

    [Theory]
    [ClassData(typeof(UnknownInventories))]
    public void MappingsWithoutAnAcceptableCaseForEachName_WhenTakingTheInventory_ShouldLeaveItUnknown(IdentityMapping[] mappings)
    {
        // Given
        ImmutableArray<IdentityMapping> provided = [.. mappings];

        // When
        var inventory = RevisionEvidence.Inventory(provided, new string('1', 40),
            ["Specht.Benchmarks.Engine.CheckBenchmarks.Check", "Specht.Benchmarks.Startup.ColdStartBenchmarks.Start"]);

        // Then
        inventory.Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(DisplayedJobs))]
    public void AgreeingExports_WhenRead_ShouldDeriveTheExporterAndTheSixConditions(string displayInfo, string job, string collection)
    {
        // Given
        var json = ExportData.CollectedFullJson
            .Replace(DisplayedOne, displayInfo, StringComparison.Ordinal)
            .Replace(DisplayedTen, displayInfo, StringComparison.Ordinal);

        // When
        var reading = RevisionEvidence.Read([json, json]);

        // Then
        reading.Should().Be(new ExportReading("0.15.8",
            new MeasurementConditions("Synthetic OS 1.0", ".NET 10.0.0", "X64", "11.0.100-synthetic.1", collection, job), null));
    }

    [Theory]
    [ClassData(typeof(UnusableExports))]
    public void UnusableExports_WhenRead_ShouldNameWhyTheyCannotBeUsed(string[] exports, string failure)
    {
        // Given
        ImmutableArray<string> jsons = [.. exports];

        // When
        var reading = RevisionEvidence.Read(jsons);

        // Then
        reading.Failure.Should().Be(failure);
    }

    [Fact]
    public void ParentWithCredentials_WhenBuildingTheChildEnvironment_ShouldKeepOnlyTheAllowlistAndTheSessionKeys()
    {
        // Given
        var parent = new Dictionary<string, string>
        {
            ["PATH"] = "synthetic-path",
            ["LANG"] = "en_US.UTF-8",
            ["LC_ALL"] = "C",
            ["SystemRoot"] = "synthetic-system-root",
            ["windir"] = "synthetic-windir",
            ["ComSpec"] = "synthetic-comspec",
            ["PATHEXT"] = ".COM;.EXE",
            ["DOTNET_ROOT"] = "synthetic-dotnet-root",
            ["DOTNET_ROOT_ARM64"] = "synthetic-dotnet-root-arm64",
            ["HOME"] = "synthetic-parent-home",
            ["USERPROFILE"] = "synthetic-parent-profile",
            ["TMPDIR"] = "synthetic-parent-temp",
            ["NUGET_PACKAGES"] = "synthetic-parent-packages",
            ["EDITOR"] = "vi",
            ["GITHUB_TOKEN"] = "ghs_synthetic-secret",
            ["GH_TOKEN"] = "gho_synthetic-secret",
            ["ACTIONS_RUNTIME_TOKEN"] = "runtime-synthetic-secret",
            ["ACTIONS_ID_TOKEN_REQUEST_TOKEN"] = "id-synthetic-secret",
            ["NuGetPackageSourceCredentials_specht"] = "Username=someone;Password=synthetic-secret",
        };

        // When
        var environment = RevisionEvidence.ChildEnvironment(parent, "session-1/previous/home", "session-1/previous/temp", "session-1/nuget");

        // Then
        environment.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["PATH"] = "synthetic-path",
            ["LANG"] = "en_US.UTF-8",
            ["LC_ALL"] = "C",
            ["SystemRoot"] = "synthetic-system-root",
            ["windir"] = "synthetic-windir",
            ["ComSpec"] = "synthetic-comspec",
            ["PATHEXT"] = ".COM;.EXE",
            ["DOTNET_ROOT"] = "synthetic-dotnet-root",
            ["DOTNET_ROOT_ARM64"] = "synthetic-dotnet-root-arm64",
            ["HOME"] = "session-1/previous/home",
            ["USERPROFILE"] = "session-1/previous/home",
            ["DOTNET_CLI_HOME"] = "session-1/previous/home",
            ["TMPDIR"] = "session-1/previous/temp",
            ["TMP"] = "session-1/previous/temp",
            ["TEMP"] = "session-1/previous/temp",
            ["NUGET_PACKAGES"] = "session-1/nuget",
            ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1",
            ["DOTNET_NOLOGO"] = "1",
        });
    }

    public sealed class Listings : TheoryData<string[], string[]>
    {
        public Listings()
        {
            Add(
                ["// Available benchmarks:", "Specht.Benchmarks.Startup.ColdStartBenchmarks.Start", "Specht.Benchmarks.Engine.CheckBenchmarks.Check"],
                ["Specht.Benchmarks.Startup.ColdStartBenchmarks.Start", "Specht.Benchmarks.Engine.CheckBenchmarks.Check"]);
            Add(
                ["Other.Benchmarks.Engine.CheckBenchmarks.Check", "Specht.Benchmarks.Engine.CheckBenchmarks.Check"],
                ["Specht.Benchmarks.Engine.CheckBenchmarks.Check"]);
            Add(["// Available benchmarks:"], []);
        }
    }

    public sealed class ToolSelections : TheoryData<string[], bool>
    {
        public ToolSelections()
        {
            Add(["Specht.Benchmarks.Engine.CheckBenchmarks.Check", "Specht.Benchmarks.Startup.ColdStartBenchmarks.Start"], true);
            Add(["Specht.Benchmarks.Engine.CheckBenchmarks.Check"], false);
            Add(["Specht.Benchmarks.StartupProbe.ProbeBenchmarks.Probe"], false);
            Add([], false);
        }
    }

    public sealed class UnknownInventories : TheoryData<IdentityMapping[]>
    {
        public UnknownInventories()
        {
            Add([new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 1)")]);
            Add(
            [
                new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 1)"),
                new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Startup.ColdStartBenchmarks.Start")
                    .WithCase(new WorkloadCaseFixture().WithOperationId("start"))
                    .WithProductCommit(new string('2', 40)),
            ]);
            Add(
            [
                new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 1)"),
                new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Startup.ColdStartBenchmarks.StartAll")
                    .WithCase(new WorkloadCaseFixture().WithOperationId("start")),
            ]);
            Add(
            [
                new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 1)"),
                new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Startup.ColdStartBenchmarks.Start")
                    .WithCase(new WorkloadCaseFixture().WithOperationId("start"))
                    .WithSourceFile("../ColdStartBenchmarks.cs"),
            ]);
            Add(
            [
                new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 1)"),
                new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Startup.ColdStartBenchmarks.Start")
                    .WithCase(new WorkloadCaseFixture().WithOperationId("start"))
                    .WithSourceDigest(new string('d', 63)),
            ]);
            Add(
            [
                new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 1)"),
                new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 10)"),
                new IdentityMappingFixture().WithFullName("Specht.Benchmarks.Startup.ColdStartBenchmarks.Start")
                    .WithCase(new WorkloadCaseFixture().WithOperationId("start")),
            ]);
        }
    }

    public sealed class DisplayedJobs : TheoryData<string, string, string>
    {
        public DisplayedJobs()
        {
            Add(DisplayedOne, "ShortRun(Toolchain=.NET 10.0, IterationCount=3, LaunchCount=1, WarmupCount=3)", "Job default");
            Add(
                "CheckBenchmarks.Check: Job-SRVGC(Server=True, Concurrent=False, HeapCount=4, Toolchain=.NET 10.0, IterationCount=3) [Specifications=1]",
                "Job-SRVGC(Server=True, Concurrent=False, HeapCount=4, Toolchain=.NET 10.0, IterationCount=3)",
                "Server=True, Concurrent=False, HeapCount=4");
            Add("CheckBenchmarks.Check: DefaultJob", "DefaultJob", "Job default");
        }
    }

    public sealed class UnusableExports : TheoryData<string[], string>
    {
        public UnusableExports()
        {
            Add(["{"], Malformed);
            Add(["[]"], Malformed);
            Add([ExportData.CollectedFullJson.Replace("\"BenchmarkDotNetVersion\": \"0.15.8\", ", string.Empty, StringComparison.Ordinal)], Malformed);
            Add([ExportData.CollectedFullJson.Replace("\"DotNetCliVersion\": \"11.0.100-synthetic.1\", ", string.Empty, StringComparison.Ordinal)], Malformed);
            Add([ExportData.CollectedFullJson.Replace(DisplayedOne, "CheckBenchmarks.Check", StringComparison.Ordinal)], Malformed);
            Add([ExportData.CollectedFullJson, ExportData.CollectedFullJson.Replace(".NET 10.0.0", ".NET 10.0.1", StringComparison.Ordinal)], Disagreeing);
            Add([ExportData.CollectedFullJson, ExportData.CollectedFullJson.Replace("\"X64\"", "\"Arm64\"", StringComparison.Ordinal)], Disagreeing);
            Add([ExportData.CollectedFullJson, ExportData.CollectedFullJson.Replace("11.0.100-synthetic.1", "11.0.100-synthetic.2", StringComparison.Ordinal)],
                Disagreeing);
            Add(
                [
                    ExportData.CollectedFullJson.Replace(DisplayedTen,
                        "CheckBenchmarks.Check: MediumRun(Toolchain=.NET 10.0, IterationCount=15, LaunchCount=2, WarmupCount=10) [Specifications=10]",
                        StringComparison.Ordinal),
                ],
                Disagreeing);
        }
    }

    private const string DisplayedOne =
        "CheckBenchmarks.Check: ShortRun(Toolchain=.NET 10.0, IterationCount=3, LaunchCount=1, WarmupCount=3) [Specifications=1]";

    private const string DisplayedTen =
        "CheckBenchmarks.Check: ShortRun(Toolchain=.NET 10.0, IterationCount=3, LaunchCount=1, WarmupCount=3) [Specifications=10]";

    private const string Malformed = "Benchmark export is malformed or names no exporter version.";
    private const string Disagreeing = "Benchmark exports disagree on their measurement conditions.";
}
