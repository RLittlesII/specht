using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
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
        var mapping = JsonSerializer.Deserialize<IdentityMapping>(MappingJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } })!;
        _previous = BenchmarkEvidenceReader.Read(ExportJson, "previous/full.json", _session.Previous, [mapping]);
        _current = BenchmarkEvidenceReader.Read(ExportJson, "current/full.json", _session.Current, [mapping]);
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
    public void WhenPairing() => _pairs = CasePairing.Pair(_previous, _current, _session);

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

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }

    private const string ExportJson = """
        {"Title":"Invented","HostEnvironmentInfo":{"BenchmarkDotNetVersion":"0.15.8","Configuration":"Release","HasAttachedDebugger":false},
         "Benchmarks":[{"FullName":"Invented.Check(1)","Statistics":{"Mean":100,"ConfidenceInterval":{"Level":12,"Lower":98,"Upper":102}},
         "Memory":{"BytesAllocatedPerOperation":32},"Measurements":[]}]}
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
    private readonly ReleaseRecord _release = JsonSerializer.Deserialize<ReleaseRecord>("""
        {"Repository":"example/tool","Id":20,"Tag":"v2","PublishedAt":"2026-01-04T00:00:00Z","Draft":false,
         "Prerelease":false,"Commit":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","ResolutionFailure":null}
        """)!;
    private SessionManifest _session;
    private ReleaseCatalog _catalog = new([], true, null);
    private ReleasePair? _pair;
    private ReleasePair? _original;
    private MeasurementInventory _previous;
    private MeasurementInventory _current;
    private ImmutableArray<PairedCase> _pairs = [];
    private string? _condition;
    private InvalidDataException? _selectionFailure;
    private string? _invalidSavedJson;
}
