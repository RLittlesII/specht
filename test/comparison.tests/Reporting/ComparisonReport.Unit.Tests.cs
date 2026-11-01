using System.Text.Json;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Comparison.Tests.Evidence;
using Specht.Comparison.Tests.Selection;

namespace Specht.Comparison.Tests.Reporting;

[Trait("Tier", "Unit")]
public sealed class ComparisonReportUnitTests
{
    [Fact]
    public void CompletedComparison_WhenRendered_ShouldGiveBothFormsTheSameOrderedFindings()
    {
        // Given
        ReleasePair pair = ReportFixtures.Pair();
        ComparisonDocument document = new ComparisonDocumentFixture();

        // When
        var json = ComparisonReport.Json(pair, document);
        var markdown = ComparisonReport.Markdown(pair, document);

        // Then
        using var report = JsonDocument.Parse(json);
        report.RootElement.GetProperty("formatVersion").GetInt32().Should().Be(1);
        var comparison = report.RootElement.GetProperty("comparison");
        comparison.GetProperty("judgment").GetString().Should().Be("ReviewRequired");
        var cases = comparison.GetProperty("cases").EnumerateArray().ToArray();
        cases.Select(static c => c.GetProperty("key").GetString()).Should().Equal(SmallKey, LargeKey);
        cases.Select(static c => c.GetProperty("status").GetString()).Should().Equal("Comparable", "Added");
        cases.Select(static c => c.GetProperty("reason").GetString()).Should().Equal(null, null);
        cases[0].GetProperty("metrics").EnumerateArray().Select(static m => (m.GetProperty("metric").GetString(),
                m.GetProperty("judgment").GetString(), m.GetProperty("unit").GetString(), m.GetProperty("previous").GetDecimal(),
                m.GetProperty("current").GetDecimal(), m.GetProperty("absoluteChange").GetDecimal(), m.GetProperty("percentChange").GetDecimal(),
                m.GetProperty("reason").GetString()))
            .Should().Equal(("Time", "ReviewRequired", "ns", 100m, 120m, 20m, 20m, TimingReason),
                ("ManagedAllocation", "NoRegressionFlagged", "B", 32m, 32m, 0m, 0m, null));
        cases[1].GetProperty("metrics").GetArrayLength().Should().Be(0);
        comparison.GetProperty("gaps").EnumerateArray().Select(static g => (g.GetProperty("caseKey").GetString(),
                g.GetProperty("metric").ValueKind, g.GetProperty("reason").GetString()))
            .Should().Equal((LargeKey, JsonValueKind.Null, AddedReason));
        var lines = markdown.Split('\n');
        markdown.Should().Contain("Review required");
        markdown.IndexOf(SmallKey, StringComparison.Ordinal).Should().BeGreaterThan(-1)
            .And.BeLessThan(markdown.IndexOf(LargeKey, StringComparison.Ordinal));
        lines.Should().Contain(static line => line.Contains(SmallKey, StringComparison.Ordinal) && line.Contains("Comparable", StringComparison.Ordinal));
        lines.Should().Contain(static line => line.Contains(LargeKey, StringComparison.Ordinal) && line.Contains("Added", StringComparison.Ordinal));
        var timing = Array.FindIndex(lines, static line => line.Contains(TimingReason, StringComparison.Ordinal));
        var allocation = Array.FindIndex(lines, static line => line.Contains("No regression flagged", StringComparison.Ordinal));
        timing.Should().BeGreaterThan(-1).And.BeLessThan(allocation);
        lines[timing].Should().Contain("Review required").And.Contain("ns").And.Contain("100").And.Contain("120");
        lines[allocation].Should().MatchRegex(@"(^|[^\w])B([^\w]|$)").And.Contain("32");
        lines.Should().Contain(static line => line.Contains(LargeKey, StringComparison.Ordinal) && line.Contains(AddedReason, StringComparison.Ordinal));
    }

    [Fact]
    public void UndefinedPercentage_WhenRenderedAsMarkdown_ShouldReadNotDefined()
    {
        // Given
        ReleasePair pair = ReportFixtures.Pair();
        ComparisonDocument document = new ComparisonDocumentFixture();
        var flagged = document.Cases[0];
        document = document with
        {
            Cases = [flagged with { Metrics = [flagged.Metrics[0], flagged.Metrics[1] with { PercentChange = null }] }],
            Gaps = [],
        };

        // When
        var markdown = ComparisonReport.Markdown(pair, document);
        var json = ComparisonReport.Json(pair, document);

        // Then
        var lines = markdown.Split('\n');
        lines.Single(static line => line.Contains("No regression flagged", StringComparison.Ordinal)).Should().Contain("Not defined");
        lines.Single(static line => line.Contains(TimingReason, StringComparison.Ordinal)).Should().NotContain("Not defined");
        using var report = JsonDocument.Parse(json);
        report.RootElement.GetProperty("comparison").GetProperty("cases")[0].GetProperty("metrics")[1].GetProperty("percentChange").ValueKind
            .Should().Be(JsonValueKind.Null);
    }

    [Theory]
    [ClassData(typeof(HostileTextRows))]
    public void HostileDisplayText_WhenRenderedAsMarkdown_ShouldNotBreakTheTableOrInjectHtml(string hostile, string forbidden, string visible)
    {
        // Given
        ReleasePair pair = ReportFixtures.Pair();
        ComparisonDocument document = new ComparisonDocumentFixture();
        var flagged = document.Cases[0];
        document = document with
        {
            Cases = [flagged with { Key = hostile, Reason = hostile, Metrics = [flagged.Metrics[0] with { Reason = hostile }, flagged.Metrics[1]] },
                document.Cases[1]],
            Gaps = [document.Gaps[0] with { CaseKey = hostile, Reason = hostile }],
        };

        // When
        var markdown = ComparisonReport.Markdown(pair, document);

        // Then
        markdown.Should().NotContain(forbidden).And.Contain(visible);
        int? width = null;
        foreach (var line in markdown.Split('\n').Select(static line => line.TrimEnd('\r')))
        {
            if (!line.StartsWith('|'))
            {
                width = null;
                continue;
            }
            var pipes = Regex.Count(line, @"(?<!\\)\|");
            (width ??= pipes).Should().Be(pipes, $"every row of a table keeps its columns: {line}");
        }
    }

    [Fact]
    public void CompletedComparison_WhenRendered_ShouldIdentifyThePairSessionPolicyWorkloadsAndRawEvidence()
    {
        // Given
        ReleasePair pair = ReportFixtures.Pair()
            .WithCurrent(new ReleaseRecordFixture().WithId(731).WithTag("v2.0.0").WithPublishedAt(new DateTimeOffset(2026, 1, 4, 0, 0, 0, TimeSpan.Zero)))
            .WithPrevious(new ReleaseRecordFixture().WithId(529).WithTag("v1.9.0").WithCommit(new string('e', 40)));
        ComparisonDocument document = new ComparisonDocumentFixture();
        document = document with
        {
            Session = new SessionManifestFixture().WithPairDigest(ComparisonJson.Digest(pair)).WithComparatorVersion("comparator-1.2.3")
                .WithRunId("run-7").WithAttempt(37).WithSessionId("session-9").WithRunnerIdentity("runner-x")
                .WithPrevious(new SessionSideFixture().WithInvocation(["--filter", "previous-filter"]).WithConditions(new MeasurementConditionsFixture()
                    .WithOperatingSystem("os-previous").WithRuntime("runtime-previous").WithArchitecture("architecture-previous")
                    .WithToolchain("toolchain-previous").WithGarbageCollection("gc-previous").WithJob("job-previous")))
                .WithCurrent(new SessionSideFixture().WithInvocation(["--filter", "current-filter"]).WithConditions(new MeasurementConditionsFixture()
                    .WithOperatingSystem("os-current").WithRuntime("runtime-current").WithArchitecture("architecture-current")
                    .WithToolchain("toolchain-current").WithGarbageCollection("gc-current").WithJob("job-current"))),
        };
        string[] identities =
        [
            "v2.0.0", "v1.9.0", new string('a', 40), new string('e', 40), ComparisonJson.Digest(pair), new string('c', 40), "comparator-1.2.3",
            "run-7", "session-9", "runner-x", "os-previous", "runtime-previous", "architecture-previous", "toolchain-previous", "gc-previous",
            "job-previous", "os-current", "runtime-current", "architecture-current", "toolchain-current", "gc-current", "job-current", "0.15.8",
            "previous-filter", "current-filter", "check", "operation-revision-2", "synthetic-tree", "fixture-revision-3", "engine-run",
            "Specifications", "raw/previous/Synthetic-report-full.json", "raw/current/Synthetic-report-full.json", "/Benchmarks/0/Statistics",
            "/Benchmarks/0/Memory", "/Benchmarks/1/Statistics", "/Benchmarks/1/Memory",
        ];

        // When
        var json = ComparisonReport.Json(pair, document);
        var markdown = ComparisonReport.Markdown(pair, document);

        // Then
        using var report = JsonDocument.Parse(json);
        var strings = new List<string>();
        var pending = new Stack<JsonElement>([report.RootElement]);
        while (pending.TryPop(out var element))
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    strings.Add(property.Name);
                    pending.Push(property.Value);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array) foreach (var item in element.EnumerateArray()) pending.Push(item);
            else if (element.ValueKind == JsonValueKind.String) strings.Add(element.GetString()!);
        }
        strings.Should().Contain(identities);
        ComparisonJson.ReadPair(report.RootElement.GetProperty("pair").GetRawText(), pair.Current).Should().BeEquivalentTo(pair);
        var comparison = report.RootElement.GetProperty("comparison");
        comparison.GetProperty("session").GetProperty("attempt").GetInt32().Should().Be(37);
        var policy = comparison.GetProperty("policy");
        (policy.GetProperty("version").GetInt32(), policy.GetProperty("timingIncreasePercent").GetDecimal(),
            policy.GetProperty("allocationIncreasePercent").GetDecimal()).Should().Be((1, 10m, 10m));
        markdown.Should().ContainAll(identities).And.ContainAll("731", "529", "37", "10%");
    }

    public sealed class HostileTextRows : TheoryData<string, string, string>
    {
        public HostileTextRows()
        {
            Add("pipe | injected", "pipe | injected", "injected");
            Add("line\nbreak", "line\nbreak", "break");
            Add("tick ` code", "tick ` code", "code");
            Add("<script>alert(1)</script>", "<script>", "alert(1)");
            Add("<b>bold</b>", "<b>", "bold");
        }
    }

    private const string SmallKey = """{"operation":"check","parameters":[{"kind":"number","name":"Specifications","value":"1"}]}""";
    private const string LargeKey = """{"operation":"check","parameters":[{"kind":"number","name":"Specifications","value":"10"}]}""";
    private const string TimingReason =
        "Mean time rose more than 10% and the current confidence interval lies strictly above the previous interval.";
    private const string AddedReason = "Workload is only in the current release; it has no regression judgment.";
}
