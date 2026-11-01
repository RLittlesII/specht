using System.Collections.Immutable;
using System.Globalization;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Reqnroll;
using Specht.Comparison;

namespace Specht.Acceptance.Comparison;

[Binding]
[Scope(Feature = "Release baseline comparison")]
public sealed class ComparisonSteps : IDisposable
{
    public ComparisonSteps()
    {
        _session = ComparisonJson.ReadSession(SessionJson);
        _mapping = JsonSerializer.Deserialize<IdentityMapping>(MappingJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } })!;
        _previous = BenchmarkEvidenceReader.Read(ExportJson, "previous/full.json", _session.Previous, [_mapping]);
        _current = BenchmarkEvidenceReader.Read(ExportJson, "current/full.json", _session.Current, [_mapping]);
    }

    [Given("earlier published stable releases, published prereleases, draft releases and unpublished tags exist")]
    public void GivenMixedReleases()
    {
        _catalog = new ReleaseCatalog(
        [
            _release with { Id = 10, Tag = "v99", PublishedAt = _release.PublishedAt!.Value.AddDays(-3) },
            _release with { Id = 15, Tag = "v2-preview", PublishedAt = _release.PublishedAt!.Value.AddDays(-1), Prerelease = true },
            _release with { Id = 18, Tag = "draft", Draft = true },
            _release with { Id = 19, Tag = "bare-tag", PublishedAt = null },
        ], true, null);
    }

    [When("a release comparison selects its baseline")]
    public void WhenSelectingBaseline()
    {
        try { _pair = ReleaseSelection.Select(_release, _catalog, "run-1"); }
        catch (InvalidDataException exception) { _selectionFailure = exception; }
    }

    [Then("it selects the immediately preceding published release, including prereleases and excluding drafts")]
    public void ThenSelectsPredecessor() => _pair!.Previous!.Tag.Should().Be("v2-preview");

    [Then("it records both release tags and their resolved commits")]
    public void ThenRecordsRevisions()
    {
        _pair!.Current.Tag.Should().Be("v2");
        _pair.Previous!.Tag.Should().Be("v2-preview");
        _pair.Current.Commit.Should().Be(new string('a', 40));
        _pair.Previous.Commit.Should().Be(new string('a', 40));
    }

    [Given("the release catalog could not be read completely")]
    public void GivenIncompleteCatalog() => _catalog = new ReleaseCatalog([], false, "Page unavailable");

    [Then("no predecessor is selected from the incomplete catalog")]
    public void ThenNoPartialSelection()
    {
        _pair.Should().BeNull();
        _selectionFailure.Should().BeOfType<InvalidDataException>();
    }

    [Given("a release comparison has selected its predecessor")]
    public async Task GivenSavedPair()
    {
        GivenMixedReleases();
        _original = ReleaseSelection.Select(_release, _catalog, "run-1");
        _pair = await new ReleasePairStore(new FileSystem(), _directory).GetOrCreateAsync(_release,
            _ => Task.FromResult(_original), CancellationToken.None);
    }

    [Given("a newer release has since been published")]
    public void GivenNewPublication() => _catalog = _catalog with
    {
        Releases = _catalog.Releases.Add(_release with { Id = 30, Tag = "v3", PublishedAt = _release.PublishedAt!.Value.AddDays(1) }),
    };

    [When("the original release comparison is rerun")]
    public async Task WhenRerunning()
    {
        try
        {
            _pair = await new ReleasePairStore(new FileSystem(), _directory)
                .GetOrCreateAsync(_release, static _ => throw new InvalidOperationException("Saved pair must bypass selection"), CancellationToken.None);
        }
        catch (InvalidDataException exception) { _selectionFailure = exception; }
    }

    [Then("it compares the original release pair")]
    public void ThenPreservesPair() => _pair.Should().BeEquivalentTo(_original);

    [Given("two earlier eligible releases have the same publication timestamp")]
    public void GivenPublicationTie() => _catalog = new ReleaseCatalog(
        [_release with { Id = 10, Tag = "lower" }, _release with { Id = 19, Tag = "higher" }], true, null);

    [Then("the release with the higher immutable release identifier is the predecessor")]
    public void ThenBreaksTie() => _pair!.Previous!.Id.Should().Be(19);

    [Given("both releases provide measurements of the same workload")]
    [Given("both releases have a measurement with the same name")]
    public void GivenSameWorkload() => _previous.Cases.Should().NotBeEmpty();

    [Given("their inputs, measurement boundaries, operating system, runtime version, architecture, toolchain, "
        + "garbage-collection mode, benchmark job and timing confidence level match")]
    public void GivenMatchingConditions() => _session.Current.Conditions.Should().Be(_session.Previous.Conditions);

    [Given("their (.*) differ")]
    public void GivenChangedCondition(string condition)
    {
        _condition = condition;
        var evidence = _current.Cases[0];
        var conditions = _session.Current.Conditions;
        conditions = condition switch
        {
            "runtimes" => conditions with { Runtime = "other" },
            "operating systems" => conditions with { OperatingSystem = "other" },
            "architectures" => conditions with { Architecture = "other" },
            "toolchains" => conditions with { Toolchain = "other" },
            "garbage-collection modes" => conditions with { GarbageCollection = "other" },
            "benchmark jobs" => conditions with { Job = "other" },
            _ => conditions,
        };
        evidence = condition switch
        {
            "workload definitions" => evidence with { Case = evidence.Case with { FixtureRevision = "2" } },
            "input parameters" => evidence with
            {
                Case = evidence.Case with
                { Parameters = evidence.Case.Parameters.SetItem("Specifications", new ParameterValue("number", "10")) }
            },
            "measurement boundaries" => evidence with { Case = evidence.Case with { Boundary = "other" } },
            "timing confidence levels" => evidence with
            {
                Observations = [evidence.Observations[0] with
                { Interval = new TimingInterval(98, 102, 0.95m) }, evidence.Observations[1]]
            },
            _ => evidence,
        };
        _current = _current with { Cases = [evidence] };
        _session = _session with { Current = _session.Current with { Conditions = conditions } };
    }

    [Given("their workload and measurement-condition equivalence cannot be established")]
    public void GivenUnknownIdentity()
    {
        _previous = BenchmarkEvidenceReader.Read(ExportJson, "previous/full.json", _session.Previous, []);
        _current = BenchmarkEvidenceReader.Read(ExportJson, "current/full.json", _session.Current, []);
    }

    [When("the comparison is generated")]
    [When("the comparison is summarised")]
    public void WhenComparing()
    {
        _pairs = CasePairing.Pair(_previous, _current, _session);
        _document = ComparisonEvaluation.Evaluate(_session, _pairs);
    }

    [Then("those measurements are paired as comparable")]
    public void ThenComparable() => _pairs.Should().ContainSingle().Which.Status.Should().Be(PairingStatus.Comparable);

    [Then("that pair is marked \"Not comparable\" with the differing condition")]
    [Then("the pair is marked \"Not comparable\" with the reason")]
    public void ThenNotComparable()
    {
        _pairs.Should().NotBeEmpty().And.OnlyContain(static pair => pair.Status == PairingStatus.NotComparable);
        var reasons = string.Join(" ", _pairs.SelectMany(static pair => pair.Metrics.Select(static metric => metric.Reason).Append(pair.Reason)));
        reasons.Should().NotBeNullOrWhiteSpace();
        if (_condition is not null)
            reasons.Should().Contain(_condition switch
            {
                "workload definitions" => "fixture",
                "input parameters" => "input parameters",
                "measurement boundaries" => "boundary",
                "runtimes" => "runtime",
                "operating systems" => "operating system",
                "architectures" => "architecture",
                "toolchains" => "toolchain",
                "garbage-collection modes" => "garbage collection",
                "benchmark jobs" => "job",
                _ => "confidence",
            });
    }

    [Then("the pair receives no regression judgment")]
    public void ThenNoJudgment() => _pairs.Should().OnlyContain(static pair => pair.Status != PairingStatus.Comparable);

    [Given("a benchmark exists only in the current release's measurement set")]
    public void GivenAdded() => _previous = _previous with { Cases = [] };

    [Given("a benchmark exists only in the previous release's measurement set")]
    public void GivenRemoved() => _current = _current with { Cases = [] };

    [Given("the current release also provides another input size")]
    public void GivenAdditionalCurrentSize()
    {
        var original = _current.Cases[0];
        _current = _current with
        {
            Cases = _current.Cases.Add(original with
            {
                Case = original.Case with
                { Parameters = original.Case.Parameters.SetItem("Specifications", new ParameterValue("number", "10")) }
            })
        };
    }

    [Given("the previous release also provides another input size")]
    public void GivenAdditionalPreviousSize()
    {
        GivenAdditionalCurrentSize();
        (_previous, _current) = (_current, _previous);
    }

    [Then("the common input size remains comparable")]
    public void ThenCommonSize() => _pairs.Should().ContainSingle(static pair => pair.Status == PairingStatus.Comparable);

    [Then("the additional input size is reported as {string}")]
    public void ThenAdditionalSize(string status) => _pairs.Should().ContainSingle(pair => pair.Status.ToString() == status);

    [Then("it is reported as {string} without a regression judgment")]
    public void ThenOneSided(string status) => _pairs.Should().ContainSingle().Which.Status.ToString().Should().Be(status);

    [Given("the releases have renamed records without audited identities")]
    public void GivenUnidentifiedRenames()
    {
        _previous = BenchmarkEvidenceReader.Read(ExportJson, "previous/full.json", _session.Previous, []);
        var renamed = ExportJson.Replace("Invented.Check(1)", "Renamed.Check(1)", StringComparison.Ordinal);
        _current = BenchmarkEvidenceReader.Read(renamed, "current/full.json", _session.Current, []);
    }

    [Then("every unidentified row is marked {string} with an identity reason")]
    public void ThenUnknownIdentityRows(string status)
    {
        status.Should().Be("Not comparable");
        _pairs.Should().HaveCount(2).And.OnlyContain(static pair => pair.Status == PairingStatus.NotComparable);
        _pairs.Should().OnlyContain(static pair => pair.Reason!.Contains("identity", StringComparison.OrdinalIgnoreCase));
    }

    [Then("no unidentified row is classified as added or removed")]
    public void ThenNoInventedCoverage() => _pairs.Should().NotContain(static pair =>
        pair.Status == PairingStatus.Added || pair.Status == PairingStatus.Removed);

    [Given("the session expects timing and allocation but both observed contracts provide timing only")]
    public void GivenExpectedAllocation()
    {
        var expected = _current.Cases[0].Case;
        _previous = _previous with
        {
            Cases = [_previous.Cases[0] with
            { Case = expected with { RequiredMetrics = [MetricKind.Time] }, Observations = [_previous.Cases[0].Observations[0]] }]
        };
        _current = _current with
        {
            Cases = [_current.Cases[0] with
            { Case = expected with { RequiredMetrics = [MetricKind.Time] }, Observations = [_current.Cases[0].Observations[0]] }]
        };
        _session = _session with
        {
            Previous = _session.Previous with { ExpectedCases = [expected] },
            Current = _session.Current with { ExpectedCases = [expected] },
        };
    }

    [Then("the required allocation remains visible as insufficient evidence")]
    public void ThenMissingAllocation() => _pairs.Single().Metrics.Single(static metric => metric.Metric == MetricKind.ManagedAllocation)
        .Status.Should().Be(PairingStatus.InsufficientEvidence);

    [Then("the valid timing remains comparable")]
    public void ThenIndependentTiming() => _pairs.Single().Metrics.Single(static metric => metric.Metric == MetricKind.Time)
        .Status.Should().Be(PairingStatus.Comparable);

    [Given("the current expected contract has a different (.*)")]
    public void GivenExpectedContractConflict(string field)
    {
        var original = _current.Cases[0].Case;
        var expected = field switch
        {
            "operation revision" => original with { OperationRevision = "2" },
            "fixture revision" => original with { FixtureRevision = "2" },
            _ => original with { Boundary = "other" },
        };
        _session = _session with { Current = _session.Current with { ExpectedCases = [expected] } };
    }

    [Given("comparable measurements with a positive previous value")]
    public void GivenPositivePreviousValues()
    {
        _current = Change(_current, MetricKind.Time, static o => o with { Value = 110, Interval = o.Interval! with { Lower = 108, Upper = 112 } });
        _current = Change(_current, MetricKind.ManagedAllocation, static o => o with { Value = 40 });
    }

    [Then("each metric shows both values, its unit, its absolute change and its percentage change")]
    public void ThenEachMetricShowsItsChange()
    {
        var metrics = _document!.Cases.Single().Metrics;
        metrics.Single(static m => m.Metric == MetricKind.Time).Should().Match<MetricComparison>(static m =>
            m.Previous == 100 && m.Current == 110 && m.Unit == "ns" && m.AbsoluteChange == 10 && m.PercentChange == 10);
        metrics.Single(static m => m.Metric == MetricKind.ManagedAllocation).Should().Match<MetricComparison>(static m =>
            m.Previous == 32 && m.Current == 40 && m.Unit == "B" && m.AbsoluteChange == 8 && m.PercentChange == 25);
    }

    [Given("a previous comparable timing of 1 millisecond")]
    public void GivenPreviousMillisecond() => _previous = Change(_previous, MetricKind.Time,
        static o => o with { Value = 1, Unit = "ms", Interval = o.Interval! with { Lower = 0.98m, Upper = 1.02m } });

    [Given("a current comparable timing of 1000000 nanoseconds")]
    public void GivenCurrentNanoseconds() => _current = Change(_current, MetricKind.Time,
        static o => o with { Value = 1000000, Unit = "ns", Interval = o.Interval! with { Lower = 980000, Upper = 1020000 } });

    [Then("the timing change is zero with a shared time unit")]
    public void ThenTimingChangeIsZero() => Metric(MetricKind.Time).Should().Match<MetricComparison>(static m =>
        m.Unit == "ns" && m.Previous == 1000000 && m.Current == 1000000 && m.AbsoluteChange == 0 && m.PercentChange == 0);

    [Given("the previous release allocated zero bytes for a comparable workload")]
    public void GivenPreviousZeroAllocation() => _previous = Change(_previous, MetricKind.ManagedAllocation, static o => o with { Value = 0 });

    [Given("the current release allocates a positive number of bytes")]
    public void GivenCurrentPositiveAllocation() => _current = Change(_current, MetricKind.ManagedAllocation, static o => o with { Value = 64 });

    [Given("both releases allocated zero bytes for a comparable workload")]
    public void GivenBothZeroAllocation()
    {
        GivenPreviousZeroAllocation();
        _current = Change(_current, MetricKind.ManagedAllocation, static o => o with { Value = 0 });
    }

    [Then("the allocation percentage change is marked \"Not defined\"")]
    public void ThenAllocationPercentNotDefined() => Metric(MetricKind.ManagedAllocation).PercentChange.Should().BeNull();

    [Then("both allocation values and the absolute increase remain visible")]
    public void ThenAllocationValuesVisible() => Metric(MetricKind.ManagedAllocation).Should().Match<MetricComparison>(static m =>
        m.Previous == 0 && m.Current == 64 && m.AbsoluteChange == 64);

    [Then("the allocation is reported as unchanged without a review flag")]
    public void ThenAllocationUnchanged() => Metric(MetricKind.ManagedAllocation).Should().Match<MetricComparison>(static m =>
        m.Judgment == MetricJudgment.NoRegressionFlagged && m.AbsoluteChange == 0);

    [Given(@"a previous comparable mean time of (\d+(?:\.\d+)?) milliseconds with confidence interval half-width (\d+(?:\.\d+)?) milliseconds")]
    public void GivenPreviousMeanTime(decimal mean, decimal halfWidth) => _previous = Change(_previous, MetricKind.Time,
        o => o with { Value = mean, Unit = "ms", Interval = o.Interval! with { Lower = mean - halfWidth, Upper = mean + halfWidth } });

    [Given(@"a current comparable mean time of (\d+(?:\.\d+)?) milliseconds with confidence interval half-width (\d+(?:\.\d+)?) milliseconds")]
    public void GivenCurrentMeanTime(decimal mean, decimal halfWidth) => _current = Change(_current, MetricKind.Time,
        o => o with { Value = mean, Unit = "ms", Interval = o.Interval! with { Lower = mean - halfWidth, Upper = mean + halfWidth } });

    [Given(@"a previous comparable allocation of (\d+) bytes per operation")]
    public void GivenPreviousAllocation(decimal bytes) => _previous = Change(_previous, MetricKind.ManagedAllocation, o => o with { Value = bytes });

    [Given(@"a current comparable allocation of (\d+) bytes per operation")]
    public void GivenCurrentAllocation(decimal bytes) => _current = Change(_current, MetricKind.ManagedAllocation, o => o with { Value = bytes });

    [Then("the timing judgment is {string}")]
    [Then("its timing judgment is marked {string}")]
    public void ThenTimingJudgment(string judgment) => ThenJudgment(MetricKind.Time, judgment);

    [Then("the allocation judgment is {string}")]
    [Then("its allocation judgment is {string}")]
    public void ThenAllocationJudgment(string judgment) => ThenJudgment(MetricKind.ManagedAllocation, judgment);

    [Given("a comparable timing pair lacks valid uncertainty measurements")]
    public void GivenMissingUncertainty() => _current = BenchmarkEvidenceReader.Read(
        ExportJson.Replace(",\"ConfidenceInterval\":{\"Level\":12,\"Lower\":98,\"Upper\":102}", "", StringComparison.Ordinal),
        "current/full.json", _session.Current, [_mapping]);

    [Given("a timing pair has (.*)")]
    public void GivenInvalidTiming(string invalid)
    {
        var statistics = invalid switch
        {
            "a zero previous mean" => """{"Mean":0,"ConfidenceInterval":{"Level":12,"Lower":-2,"Upper":2}}""",
            "a negative mean" => """{"Mean":-100,"ConfidenceInterval":{"Level":12,"Lower":-102,"Upper":-98}}""",
            "a non-finite mean" => """{"Mean":"NaN","ConfidenceInterval":{"Level":12,"Lower":98,"Upper":102}}""",
            "a negative interval half-width" => """{"Mean":100,"ConfidenceInterval":{"Level":12,"Lower":102,"Upper":98}}""",
            "an unknown timing confidence level" => """{"Mean":100,"ConfidenceInterval":{"Level":99,"Lower":98,"Upper":102}}""",
            _ => throw new ArgumentOutOfRangeException(nameof(invalid), invalid, "Unbound invalid timing example."),
        };
        var json = ExportJson.Replace(ValidStatistics, statistics, StringComparison.Ordinal);
        if (invalid == "a zero previous mean") _previous = BenchmarkEvidenceReader.Read(json, "previous/full.json", _session.Previous, [_mapping]);
        else _current = BenchmarkEvidenceReader.Read(json, "current/full.json", _session.Current, [_mapping]);
    }

    [Given("some comparable metrics need no review")]
    [Given("no metric meets its regression policy")]
    public void GivenUnflaggedMetrics() => _current.Cases.SelectMany(static c => c.Observations).Should().OnlyContain(o =>
        _previous.Cases.SelectMany(static c => c.Observations).Any(p => p.Metric == o.Metric && p.Value == o.Value && p.Unit == o.Unit));

    [Given("another comparable metric is flagged {string}")]
    public void GivenFlaggedMetric(string judgment)
    {
        judgment.Should().Be("Review required");
        _current = Change(_current, MetricKind.ManagedAllocation, static o => o with { Value = 64 });
    }

    [Given("no metric is flagged {string}")]
    [Given("no other metric is flagged {string}")]
    public void GivenNoFlag(string judgment)
    {
        judgment.Should().Be("Review required");
        GivenUnflaggedMetrics();
    }

    [Given("a required metric is missing, not comparable or has insufficient evidence")]
    public void GivenRequiredMetricGap() => _current = _current with
    {
        Cases = [_current.Cases[0] with { Observations = [.. _current.Cases[0].Observations.Where(static o => o.Metric != MetricKind.ManagedAllocation)] }],
    };

    [Given("the release pair has no measurable workload cases")]
    public void GivenEmptyInventory()
    {
        _previous = _previous with { Cases = [] };
        _current = _current with { Cases = [] };
    }

    [Given("a workload case has more than one result for the same release and comparison session")]
    public void GivenDuplicateResult()
    {
        var evidence = _current.Cases[0];
        var timing = evidence.Observations.Single(static o => o.Metric == MetricKind.Time);
        _current = _current with { Cases = [evidence with { Observations = evidence.Observations.Add(timing with { Locator = "/Benchmarks/1/Statistics" }) }] };
    }

    [Then("the duplicate case is reported as invalid evidence")]
    public void ThenDuplicateInvalid()
    {
        var timing = Metric(MetricKind.Time);
        timing.Judgment.Should().Be(MetricJudgment.InsufficientEvidence);
        timing.Evidence.Current.Select(static o => o.Locator).Should().Equal("/Benchmarks/0/Statistics", "/Benchmarks/1/Statistics");
        _document!.Gaps.Should().Contain(gap => gap.CaseKey == _document.Cases[0].Key && gap.Metric == MetricKind.Time);
    }

    [Given("the release comparison requires at least one workload case")]
    public void GivenRequiredCase() => _previous.Cases.Should().NotBeEmpty();

    [Given("every metric required for the release comparison has a valid comparable result")]
    public void GivenValidRequiredMetrics() => _previous.Cases.Concat(_current.Cases).SelectMany(static c => c.Observations)
        .Should().OnlyContain(static o => o.InvalidReason == null);

    [Then("the overall judgment is {string}")]
    public void ThenOverallJudgment(string judgment) => _document!.Judgment.Should().Be(
        Enum.Parse<ComparisonJudgment>(judgment.Replace(" ", "", StringComparison.Ordinal), ignoreCase: true));

    [Given("a saved pair names the current release as its own predecessor")]
    public async Task GivenSavedSelfPair()
    {
        var pair = ReleaseSelection.Select(_release, new ReleaseCatalog([], true, null), "run-1");
        pair = pair with { Previous = _release with { PublishedAt = _release.PublishedAt!.Value.AddDays(-1) } };
        _invalidSavedJson = ComparisonJson.Write(pair);
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(Path.Combine(_directory, "comparison-pair-v1.json"), _invalidSavedJson, CancellationToken.None);
    }

    [Then("the saved pair is rejected without reselection or overwrite")]
    public async Task ThenSavedSelfPairRejected()
    {
        _selectionFailure.Should().BeOfType<InvalidDataException>();
        _pair.Should().BeNull();
        (await File.ReadAllTextAsync(Path.Combine(_directory, "comparison-pair-v1.json"), CancellationToken.None)).Should().Be(_invalidSavedJson);
    }

    [Given("a release comparison has completed")]
    public void GivenCompletedComparison()
    {
        _reportPair = ReleaseSelection.Select(_release, new ReleaseCatalog([_release with
        {
            Id = 10, Tag = "v1", PublishedAt = _release.PublishedAt!.Value.AddDays(-3), Commit = new string('e', 40),
        }], true, null), "run-1");
        _session = _session with { PairDigest = ComparisonJson.Digest(_reportPair) };
        _reportMappings = [_mapping, _mapping with
        {
            FullName = "Invented.Check(10)",
            Case = _mapping.Case with { Parameters = _mapping.Case.Parameters.SetItem("Specifications", new ParameterValue("number", "10")) },
        }];
        Directory.CreateDirectory(Path.Combine(_directory, ".artifacts", "benchmarks", "previous"));
        Directory.CreateDirectory(Path.Combine(_directory, ".artifacts", "benchmarks", "current"));
        File.WriteAllText(Path.Combine(_directory, PreviousReportExport), ExportJson);
        File.WriteAllText(Path.Combine(_directory, CurrentReportExport), CurrentReportJson);
        _previous = BenchmarkEvidenceReader.Read(ExportJson, PreviousReportExport, _session.Previous, _reportMappings);
        _current = BenchmarkEvidenceReader.Read(CurrentReportJson, CurrentReportExport, _session.Current, _reportMappings);
        WhenComparing();
    }

    [Given("a release comparison report was published")]
    public async Task GivenReportPublished()
    {
        GivenCompletedComparison();
        await WhenReportPublished();
    }

    [When("its report is published")]
    public async Task WhenReportPublished()
    {
        _bundle = new ReportBundler(new FileSystem(), _directory).Create(_reportPair!, _document!, _reportMappings,
            [new RawExport("previous", PreviousReportExport), new RawExport("current", CurrentReportExport)], ".artifacts/benchmarks/comparison");
        _bundleZip = await File.ReadAllBytesAsync(Path.Combine(_directory, _bundle.Path), CancellationToken.None);
        _published = await new ReportBundleAssetStore(_releaseClient).PublishAsync(_release, _bundle.Name, _bundleZip, CancellationToken.None);
    }

    [When("its transient workflow artifacts expire")]
    public void WhenArtifactsExpire()
    {
        File.Delete(Path.Combine(_directory, _bundle!.Path));
        Directory.Delete(_directory, true);
    }

    [Then("human-readable and machine-readable reports describe the same findings")]
    public void ThenSameFindings()
    {
        var markdown = Entry("report.md");
        using var report = JsonDocument.Parse(Entry("report.json"));
        var comparison = report.RootElement.GetProperty("comparison");
        comparison.GetProperty("judgment").GetString().Should().Be(_document!.Judgment.ToString());
        comparison.GetProperty("cases").EnumerateArray().Select(static c => c.GetProperty("key").GetString())
            .Should().Equal(_document.Cases.Select(static c => c.Key));
        _document.Cases.Select(c => markdown.IndexOf(c.Key, StringComparison.Ordinal)).Should().OnlyContain(static p => p >= 0)
            .And.BeInAscendingOrder();
        var reasons = _document.Cases.SelectMany(static c => c.Metrics.Select(static m => m.Reason).Append(c.Reason))
            .Concat(_document.Gaps.Select(static g => g.Reason)).OfType<string>().Distinct().ToArray();
        reasons.Should().NotBeEmpty();
        NamesAndStrings(report.RootElement).Should().Contain(reasons);
        markdown.Should().ContainAll(reasons).And.Contain("Review required");
    }

    [Then("it records the release pair, measurement environments, workload identities, comparison policy and raw result references")]
    public void ThenRecordsEvidenceIdentity()
    {
        ComparisonJson.ReadPair(Entry("pair.json"), _release).Should().BeEquivalentTo(_reportPair);
        ComparisonJson.ReadSession(Entry("session.json")).Should().BeEquivalentTo(_session);
        string[] identities =
        [
            "v2", "v1", new string('a', 40), new string('e', 40), ComparisonJson.Digest(_reportPair), new string('c', 40), "run-1", "session-1",
            "runner-1", "Linux", "10.0.0", "x64", "out-of-process", "workstation", "Short", "0.15.8", "check", "synthetic-tree", "engine-run",
            "Specifications", "raw/previous/Invented-report-full.json", "raw/current/Invented-report-full.json", "/Benchmarks/0/Statistics",
            "/Benchmarks/0/Memory", "/Benchmarks/1/Statistics", "/Benchmarks/1/Memory",
        ];
        using var report = JsonDocument.Parse(Entry("report.json"));
        NamesAndStrings(report.RootElement).Should().Contain(identities);
        var policy = report.RootElement.GetProperty("comparison").GetProperty("policy");
        (policy.GetProperty("version").GetInt32(), policy.GetProperty("timingIncreasePercent").GetDecimal(),
            policy.GetProperty("allocationIncreasePercent").GetDecimal()).Should().Be((1, 10m, 10m));
        Entry("report.md").Should().ContainAll(identities).And.Contain("10%");
        _bundle!.Manifest.Files.Where(static f => f.Role == "raw-export").Select(static f => (f.Path, f.Side, f.Source, f.SourceSha256))
            .Should().Equal(
                ("raw/current/Invented-report-full.json", "current", CurrentReportExport,
                    Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(CurrentReportJson)))),
                ("raw/previous/Invented-report-full.json", "previous", PreviousReportExport,
                    Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(ExportJson)))));
    }

    [Then("its report and the raw evidence it references remain available with that release")]
    public async Task ThenReportRetained()
    {
        Directory.Exists(_directory).Should().BeFalse();
        using var listing = await _releaseClient.GetAsync(
            new Uri("https://api.github.com/repos/example/tool/releases/20/assets?per_page=100&page=1"), CancellationToken.None);
        using var assets = JsonDocument.Parse(await listing.Content.ReadAsStringAsync(CancellationToken.None));
        var id = assets.RootElement.EnumerateArray().Single(asset => asset.GetProperty("name").GetString() == _bundle!.Name).GetProperty("id").GetInt64();
        id.Should().Be(_published!.AssetId);
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://api.github.com/repos/example/tool/releases/assets/{id}"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
        using var response = await _releaseClient.SendAsync(request, CancellationToken.None);
        var zip = await response.Content.ReadAsByteArrayAsync(CancellationToken.None);
        Convert.ToHexStringLower(SHA256.HashData(zip)).Should().Be(_published.Sha256);
        var manifest = ReportBundler.Validate(new MemoryStream(zip));
        ZipFile.ExtractToDirectory(new MemoryStream(zip), _extracted);
        foreach (var file in manifest.Files)
        {
            var content = await File.ReadAllBytesAsync(Path.Combine(_extracted, file.Path), CancellationToken.None);
            (content.LongLength, Convert.ToHexStringLower(SHA256.HashData(content))).Should().Be((file.Length, file.Sha256), file.Path);
        }
        var json = await File.ReadAllTextAsync(Path.Combine(_extracted, "report.json"), CancellationToken.None);
        var markdown = await File.ReadAllTextAsync(Path.Combine(_extracted, "report.md"), CancellationToken.None);
        var cited = _document!.Cases.SelectMany(static c => new[] { ("previous", c.Evidence.Previous), ("current", c.Evidence.Current) })
            .SelectMany(static side => side.Item2?.Observations.Select(o => (Side: side.Item1, Observation: o)) ?? []).ToArray();
        cited.Should().NotBeEmpty();
        foreach (var (side, observation) in cited)
        {
            var path = $"raw/{side}/{Path.GetFileName(observation.SourceFile)}";
            manifest.Files.Should().Contain(file => file.Path == path);
            json.Should().Contain(path).And.Contain(observation.Locator);
            markdown.Should().Contain(path).And.Contain(observation.Locator);
            using var raw = JsonDocument.Parse(await File.ReadAllBytesAsync(Path.Combine(_extracted, path), CancellationToken.None));
            var element = raw.RootElement;
            foreach (var segment in observation.Locator.Split('/').Skip(1))
                element = element.ValueKind == JsonValueKind.Array ? element[int.Parse(segment, CultureInfo.InvariantCulture)] : element.GetProperty(segment);
            element.ValueKind.Should().Be(JsonValueKind.Object, observation.Locator);
        }
    }

    [Given("release comparison evidence whose (.*) contains \"(.*)\"")]
    public void GivenRecordedValue(string source, string value)
    {
        GivenCompletedComparison();
        _recordedSource = source;
        _recordedValue = value;
    }

    [Given("raw measurement evidence holding the value {string}")]
    public void GivenRawMeasurementValue(string value) => GivenRecordedValue("raw measurement export", value);

    [When("a report bundle holding it is (.*)")]
    public void WhenRecordedValueBundled(string action)
    {
        _bundleAction = action;
        try
        {
            _bundle = new ReportBundler(new FileSystem(), _directory).Create(_reportPair!, Recorded(action == "validated" ? Placeholder : _recordedValue!),
                _reportMappings, [new RawExport("previous", PreviousReportExport), new RawExport("current", CurrentReportExport)], ReportWorkspace);
            _bundleZip = File.ReadAllBytes(Path.Combine(_directory, _bundle.Path));
            if (action == "validated") _bundleZip = Substituted(_bundleZip, _bundle.Manifest);
            if (action != "built") _validated = ReportBundler.Validate(new MemoryStream(_bundleZip));
        }
        catch (InvalidDataException exception)
        {
            _rejection = exception;
        }
    }

    [Then("the bundle is rejected")]
    public void ThenBundleRejected()
    {
        _rejection.Should().NotBeNull();
        _validated.Should().BeNull();
        var workspace = Path.Combine(_directory, ReportWorkspace);
        if (_bundleAction == "built") (Directory.Exists(workspace) ? Directory.GetFiles(workspace) : []).Should().BeEmpty();
    }

    [Then("no recorded value is rewritten to make it pass")]
    public void ThenNoRecordedValueRewritten()
    {
        if (_bundleAction == "built") _bundleZip.Should().BeNull();
        else
        {
            using var archive = new ZipArchive(new MemoryStream(_bundleZip!));
            var recorded = JsonSerializer.Serialize(_recordedValue)[1..^1];
            archive.Entries.Select(static entry =>
            {
                using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
                return reader.ReadToEnd();
            }).Should().Contain(text => text.Contains(recorded, StringComparison.Ordinal) && !text.Contains("[path removed]", StringComparison.Ordinal));
        }
    }

    [Then("the bundle is accepted with {string} unchanged")]
    public void ThenBundleAccepted(string value)
    {
        _rejection.Should().BeNull();
        _validated.Should().BeEquivalentTo(_bundle!.Manifest);
        Entry(_recordedSource switch
        {
            "session invocation" => "session.json",
            "raw measurement export" => "raw/current/Invented-report-full.json",
            _ => "report.json",
        }).Should().Contain(JsonSerializer.Serialize(value));
    }

    [Then("that value is replaced with a removal marker")]
    public void ThenValueRemoved()
    {
        _rejection.Should().BeNull();
        Entry("raw/current/Invented-report-full.json").Should().Contain("\"Title\":\"[path removed]\"").And.NotContain(_recordedValue!);
    }

    [Then("the bundle records that absolute paths were removed from that evidence")]
    public void ThenRemovalRecorded() => _bundle!.Manifest.Files.Single(static file => file.Path == "raw/current/Invented-report-full.json")
        .Transformation.Should().Be("absolute-paths-removed");

    [Given("a previous release has been selected")]
    public void GivenSelectedPredecessor() => _pair = ReleaseSelection.Select(_release, new ReleaseCatalog(
        [_release with { Id = 10, Tag = "v1", PublishedAt = _release.PublishedAt!.Value.AddDays(-1), Commit = new string('1', 40) }], true, null),
        "run-1");

    [When("their measurements are collected")]
    public async Task WhenCollecting()
    {
        var plan = CollectionPlan.For(_pair!, new string('c', 40), "1", "run-1", 1, "session-1", "runner-1", "Specht.Benchmarks.*", null, []);
        Directory.CreateDirectory(_directory);
        _collection = await new PairedCollector(new FileSystem(), new FakeTimeProvider(), RunRevisionAsync, _directory,
                new Dictionary<string, string> { ["PATH"] = "synthetic-path", ["GITHUB_TOKEN"] = "synthetic-secret" })
            .CollectAsync(plan!, CancellationToken.None);
    }

    [Then("both revisions are measured in the same comparison session on the same runner")]
    public async Task ThenOneSession()
    {
        _collection!.Previous.Status.Should().Be(CollectionStatus.Collected);
        _collection.Current.Status.Should().Be(CollectionStatus.Collected);
        _collection.Session.SessionId.Should().Be("session-1");
        _collection.Session.RunnerIdentity.Should().Be("runner-1");
        _collection.Session.Previous.ProductCommit.Should().Be(new string('1', 40));
        _collection.Session.Current.ProductCommit.Should().Be(new string('a', 40));
        _checkouts.Should().Equal(new string('1', 40), new string('a', 40));
        (await File.ReadAllTextAsync(Path.Combine(_directory, ".artifacts/benchmarks/comparison/session-1/session-manifest.json"),
            CancellationToken.None)).Should().Be(ComparisonJson.Write(_collection.Session));
    }

    [Then("the report identifies the environment of each measurement")]
    public void ThenEachEnvironment()
    {
        foreach (var side in new[] { _collection!.Session.Previous, _collection.Session.Current })
            side.Conditions.Should().Be(new MeasurementConditions("Invented OS 1.0", ".NET 10.0.0", "X64", "11.0.100-invented.1", "Job default",
                "ShortRun(Toolchain=.NET 10.0, IterationCount=3, LaunchCount=1, WarmupCount=3)"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        if (Directory.Exists(_extracted)) Directory.Delete(_extracted, true);
        _releaseClient.Dispose();
    }

    private static MeasurementInventory Change(MeasurementInventory inventory, MetricKind metric, Func<Observation, Observation> change) =>
        inventory with
        {
            Cases = [inventory.Cases[0] with { Observations = [.. inventory.Cases[0].Observations.Select(o => o.Metric == metric ? change(o) : o)] }],
        };

    private static IEnumerable<string> NamesAndStrings(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => element.EnumerateObject().SelectMany(static property => NamesAndStrings(property.Value).Prepend(property.Name)),
        JsonValueKind.Array => element.EnumerateArray().SelectMany(NamesAndStrings),
        JsonValueKind.String => [element.GetString()!],
        _ => [],
    };

    private async Task<ProcessOutcome> RunRevisionAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        var side = string.Join(' ', request.Arguments.Append(request.WorkingDirectory)).Contains("/previous/", StringComparison.Ordinal)
            ? "previous" : "current";
        var workspace = Path.Combine(_directory, ".artifacts", "benchmarks", "comparison", "session-1", side);
        switch (request.Arguments[0])
        {
            case "clone":
                Directory.CreateDirectory(Path.Combine(workspace, "source", ".performance", "benchmarks"));
                await File.WriteAllTextAsync(Path.Combine(workspace, "source", ".performance", "benchmarks", "benchmarks.csproj"), "<Project />",
                    cancellationToken);
                break;
            case "checkout":
                _checkouts.Add(request.Arguments[^1]);
                break;
            case "rev-parse":
                return new ProcessOutcome(0, [_checkouts[^1]]);
            case "run" when request.Arguments.Contains("--list"):
                return new ProcessOutcome(0, ["Specht.Benchmarks.Engine.CheckBenchmarks.Check"]);
            case "run":
                Directory.CreateDirectory(Path.Combine(workspace, "results", "results"));
                await File.WriteAllTextAsync(Path.Combine(workspace, "results", "results", "Specht.Benchmarks.Engine.CheckBenchmarks-report-full.json"),
                    CollectedExportJson, cancellationToken);
                break;
        }

        return new ProcessOutcome(0, []);
    }

    private void ThenJudgment(MetricKind metric, string judgment)
    {
        var result = Metric(metric);
        result.Judgment.Should().Be(Enum.Parse<MetricJudgment>(judgment.Replace(" ", "", StringComparison.Ordinal), ignoreCase: true));
        if (result.Judgment is MetricJudgment.ReviewRequired or MetricJudgment.InsufficientEvidence)
            result.Reason.Should().NotBeNullOrWhiteSpace();
    }

    private MetricComparison Metric(MetricKind metric) => _document!.Cases.Single().Metrics.Single(m => m.Metric == metric);

    private ComparisonDocument Recorded(string value)
    {
        var document = _document!;
        var flagged = document.Cases[0];
        var evidence = flagged.Evidence.Current!;
        if (_recordedSource == "session invocation")
        {
            return document with
            {
                Session = document.Session with { Previous = document.Session.Previous with { Invocation = [.. document.Session.Previous.Invocation, value] } },
            };
        }
        if (_recordedSource == "raw measurement export")
        {
            File.WriteAllText(Path.Combine(_directory, CurrentReportExport),
                CurrentReportJson.Replace("\"Title\":\"Invented\"", $"\"Title\":{JsonSerializer.Serialize(value)}", StringComparison.Ordinal));
            return document;
        }
        if (_recordedSource == "workload parameter")
        {
            flagged = flagged with
            {
                Evidence = flagged.Evidence with
                {
                    Current = evidence with
                    {
                        Case = evidence.Case with { Parameters = evidence.Case.Parameters.SetItem("Root", new ParameterValue("string", value)) },
                    },
                },
            };
        }
        else if (_recordedSource == "case reason") flagged = flagged with { Reason = value };
        else flagged = flagged with { Metrics = [flagged.Metrics[0] with { Unit = value }, .. flagged.Metrics.Skip(1)] };
        return document with { Cases = [flagged, .. document.Cases.Skip(1)] };
    }

    private byte[] Substituted(byte[] zip, EvidenceManifest manifest)
    {
        var replacement = JsonSerializer.Serialize(_recordedValue)[1..^1];
        var entries = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        using (var archive = new ZipArchive(new MemoryStream(zip)))
        {
            foreach (var entry in archive.Entries)
            {
                using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
                entries.Add(entry.FullName, Encoding.UTF8.GetBytes(reader.ReadToEnd().Replace(Placeholder, replacement, StringComparison.Ordinal)));
            }
        }
        manifest = manifest with
        {
            Files = [.. manifest.Files.Select(file => file with
            {
                Sha256 = Convert.ToHexStringLower(SHA256.HashData(entries[file.Path])), Length = entries[file.Path].LongLength,
            })],
        };
        entries["evidence-manifest.json"] = Encoding.UTF8.GetBytes(ComparisonJson.Write(manifest));
        using var buffer = new MemoryStream();
        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            foreach (var (name, content) in entries)
            {
                using var stream = archive.CreateEntry(name).Open();
                stream.Write(content);
            }
        }
        return buffer.ToArray();
    }

    private string Entry(string name)
    {
        using var archive = new ZipArchive(new MemoryStream(_bundleZip!));
        using var reader = new StreamReader(archive.GetEntry(name)!.Open(), Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private const string ValidStatistics = """{"Mean":100,"ConfidenceInterval":{"Level":12,"Lower":98,"Upper":102}}""";

    private const string ExportJson = """
        {"Title":"Invented","HostEnvironmentInfo":{"BenchmarkDotNetVersion":"0.15.8","Configuration":"Release","HasAttachedDebugger":false},
         "Benchmarks":[{"FullName":"Invented.Check(1)","Statistics":{"Mean":100,"ConfidenceInterval":{"Level":12,"Lower":98,"Upper":102}},
         "Memory":{"BytesAllocatedPerOperation":32},"Measurements":[]}]}
        """;

    private const string Placeholder = "recorded-value-placeholder";
    private const string ReportWorkspace = ".artifacts/benchmarks/comparison";
    private const string PreviousReportExport = ".artifacts/benchmarks/previous/Invented-report-full.json";
    private const string CurrentReportExport = ".artifacts/benchmarks/current/Invented-report-full.json";

    private const string CurrentReportJson = """
        {"Title":"Invented","HostEnvironmentInfo":{"BenchmarkDotNetVersion":"0.15.8","Configuration":"Release","HasAttachedDebugger":false},
         "Benchmarks":[{"FullName":"Invented.Check(1)","Statistics":{"Mean":120,"ConfidenceInterval":{"Level":12,"Lower":118,"Upper":122}},
         "Memory":{"BytesAllocatedPerOperation":32},"Measurements":[]},
         {"FullName":"Invented.Check(10)","Statistics":{"Mean":300,"ConfidenceInterval":{"Level":12,"Lower":290,"Upper":310}},
         "Memory":{"BytesAllocatedPerOperation":64},"Measurements":[]}]}
        """;

    private const string CollectedExportJson = """
        {"Title":"Invented","HostEnvironmentInfo":{"BenchmarkDotNetVersion":"0.15.8","OsVersion":"Invented OS 1.0","RuntimeVersion":".NET 10.0.0",
         "Architecture":"X64","Configuration":"RELEASE","HasAttachedDebugger":false,"DotNetCliVersion":"11.0.100-invented.1"},
         "Benchmarks":[{"DisplayInfo":"CheckBenchmarks.Check: ShortRun(Toolchain=.NET 10.0, IterationCount=3, LaunchCount=1, WarmupCount=3)",
         "FullName":"Specht.Benchmarks.Engine.CheckBenchmarks.Check","Statistics":{"Mean":100,
         "ConfidenceInterval":{"Level":12,"Lower":98,"Upper":102}},"Memory":{"BytesAllocatedPerOperation":32},"Measurements":[]}]}
        """;

    private const string MappingJson = """
        {"fullName":"Invented.Check(1)","productCommit":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
         "sourceFile":"Check.cs","sourceDigest":"dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd",
         "case":{"operationId":"check","operationRevision":"1","fixtureId":"synthetic-tree","fixtureRevision":"1",
         "parameters":{"Specifications":{"kind":"number","value":"1"}},"boundary":"engine-run","requiredMetrics":["Time","ManagedAllocation"]}}
        """;

    private const string SessionJson = """
        {"formatVersion":1,"pairDigest":"bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
         "comparatorCommit":"cccccccccccccccccccccccccccccccccccccccc","comparatorVersion":"1","runId":"run-1","attempt":1,
         "sessionId":"session-1","runnerIdentity":"runner-1",
         "previous":{"productCommit":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","harnessVersion":"native-v1","exporterVersion":"0.15.8",
         "invocation":["--filter","*Check*"],"conditions":{"operatingSystem":"Linux","runtime":"10.0.0","architecture":"x64",
         "toolchain":"out-of-process","garbageCollection":"workstation","job":"Short"},"inventoryKnown":true,"expectedCases":[]},
         "current":{"productCommit":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","harnessVersion":"native-v1","exporterVersion":"0.15.8",
         "invocation":["--filter","*Check*"],"conditions":{"operatingSystem":"Linux","runtime":"10.0.0","architecture":"x64",
         "toolchain":"out-of-process","garbageCollection":"workstation","job":"Short"},"inventoryKnown":true,"expectedCases":[]}}
        """;

    private readonly string _directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    private readonly string _extracted = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    private readonly HttpClient _releaseClient = new(new ReleaseAssets());
    private readonly List<string> _checkouts = [];
    private readonly ReleaseRecord _release = JsonSerializer.Deserialize<ReleaseRecord>("""
        {"Repository":"example/tool","Id":20,"Tag":"v2","PublishedAt":"2026-01-04T00:00:00Z","Draft":false,
         "Prerelease":false,"Commit":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","ResolutionFailure":null}
        """)!;
    private readonly IdentityMapping _mapping;
    private SessionManifest _session;
    private ReleaseCatalog _catalog = new([], true, null);
    private ReleasePair? _pair;
    private ReleasePair? _original;
    private MeasurementInventory _previous;
    private MeasurementInventory _current;
    private ImmutableArray<PairedCase> _pairs = [];
    private ComparisonDocument? _document;
    private string? _condition;
    private InvalidDataException? _selectionFailure;
    private string? _invalidSavedJson;
    private ReleasePair? _reportPair;
    private ImmutableArray<IdentityMapping> _reportMappings = [];
    private ReportBundle? _bundle;
    private byte[]? _bundleZip;
    private PublishedBundle? _published;
    private string? _recordedSource;
    private string? _recordedValue;
    private string? _bundleAction;
    private EvidenceManifest? _validated;
    private InvalidDataException? _rejection;
    private CollectionResult? _collection;
}
