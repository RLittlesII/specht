using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules.Matrix;
using Specht.Tests.Shared;
using Specht.Trace;

namespace Specht.Tests.Trace;

/// <summary>
/// The trace projected from a model built in memory through the matrix resolver (<c>0001-F13</c> B-001, B-003 to B-005,
/// B-007, B-010 to B-012, B-019, B-020): every specification in the model's order and every claim in its claims table's order, the same text for the
/// same model, each specification's path and id, each claim's line, status, row, scenarios and tests, a test's file and
/// line only when it resolves, no row and no test for a claim with no traceability row, every test of a waived, withdrawn
/// or superseded row <c>skipped</c>, and one tab-separated line per claim holding the path, the id, the row's Status or
/// <c>none</c>, the scenario count and the resolved tests out of those named, a skipped test counting in neither. With no
/// <c>tests</c> key every test outside a waived, withdrawn or superseded row is <c>unconfigured</c> and every test in one
/// <c>skipped</c>; the unresolved-test rule set <c>off</c> leaves each test's resolution as it is.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class TraceDocumentUnitTests
{
    [Fact]
    public void FeaturesInTheModelsOrder_WhenTheTraceIsMade_ShouldListEachSpecificationInThatOrderWithItsClaimsInClaimsTableOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/zeta/.spec/README.md"))
                .WithDocument(DocumentParser.Parse("""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-002 | It traces second. | brd | Active |
                    | B-001 | It traces first. | brd | Active |
                    """, "src/zeta/.spec/README.md")),
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/alpha/.spec/README.md"))
                .WithDocument(DocumentParser.Parse("""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-001 | It traces. | brd | Active |
                    """, "src/alpha/.spec/README.md")));

        // When
        var document = JsonNode.Parse(TraceDocument.From(model, new MatrixResolver()).ToJson())!;

        // Then
        document["specifications"]!.AsArray().Select(static specification => (
                specification!["path"]!.GetValue<string>(),
                string.Join(',', specification["claims"]!.AsArray().Select(static claim => claim!["id"]!.GetValue<string>()))))
            .Should().Equal(("src/zeta/.spec/README.md", "B-002,B-001"), ("src/alpha/.spec/README.md", "B-001"));
    }

    [Fact]
    public void TheSameModel_WhenTheTraceIsMadeTwice_ShouldGiveTheSameDocumentAndTheSameLines()
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0", ["tests"] = new JsonObject { ["files"] = new JsonArray("test/**/*.cs") } };
        SpechtTestFile file = new SpechtTestFileFixture().WithRelativePath("test/TraceTests.cs").WithText("public void TracesOne() { }");
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(manifest)
            .WithList([file])
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/trace/.spec/README.md"))
                .WithDocument(DocumentParser.Parse("""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-001 | It traces one claim. | brd | Active |
                    | B-002 | It traces another. | brd | Active |

                    ## 9. Traceability Matrix

                    | Claim ID | Scenario | Test | Status |
                    | -------- | -------- | ---- | ------ |
                    | B-001 | Tracing one | `TracesOne`, `TracesNone` | Partial |
                    """, "src/trace/.spec/README.md"))
                .WithFeatureFiles(new FeatureTagParser().Read(
                    "src/trace/.spec/trace.feature",
                    "Feature: Trace\n\n  @B-001\n  Scenario: Tracing one\n    Given a thing\n",
                    EngineServices.Parser().Parse(manifest.ToJsonString()))));

        // When
        var first = TraceDocument.From(model, new MatrixResolver());
        var second = TraceDocument.From(model, new MatrixResolver());

        // Then
        first.ToJson().Should().NotBeEmpty();
        second.ToJson().Should().Be(first.ToJson());
        second.Lines().Should().NotBeEmpty().And.Equal(first.Lines());
    }

    [Fact]
    public void AClaimWithARowAScenarioAndTwoTests_WhenTheTraceIsMade_ShouldCarryItsPathIdLineStatusRowScenarioAndEachTestsResolution()
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0", ["tests"] = new JsonObject { ["files"] = new JsonArray("test/**/*.cs") } };
        SpechtTestFile file = new SpechtTestFileFixture()
            .WithRelativePath("test/TraceTests.cs")
            .WithText("namespace Trace;\n\npublic sealed class TraceTests\n{\n    public void TracesOne() { }\n}\n");
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(manifest)
            .WithList([file])
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/trace/.spec/README.md"))
                .WithDocument(DocumentParser.Parse("""
                    ---
                    epic: "0001"
                    id: "F1"
                    ---
                    # Specification: F1

                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-001 | It traces one claim. | brd | Active |

                    ## 9. Traceability Matrix

                    | Claim ID | Scenario | Test | Status |
                    | -------- | -------- | ---- | ------ |
                    | B-001 | Tracing one | `TracesOne`, `TracesNone` | Partial |
                    """, "src/trace/.spec/README.md"))
                .WithFeatureFiles(new FeatureTagParser().Read(
                    "src/trace/.spec/trace.feature",
                    "Feature: Trace\n\n  @B-001\n  Scenario: Tracing one\n    Given a thing\n",
                    EngineServices.Parser().Parse(manifest.ToJsonString()))));

        // When
        var specification = JsonNode.Parse(TraceDocument.From(model, new MatrixResolver()).ToJson())!["specifications"]!.AsArray()
            .Should().ContainSingle().Which!;

        // Then
        specification["path"]!.GetValue<string>().Should().Be("src/trace/.spec/README.md");
        specification["id"]!.GetValue<string>().Should().Be("0001-F1");
        var claim = specification["claims"]!.AsArray().Should().ContainSingle().Which!;
        (claim["id"]!.GetValue<string>(), claim["line"]!.GetValue<int>(), claim["status"]!.GetValue<string>()).Should().Be(("B-001", 11, "Active"));
        var row = claim["row"]!;
        (row["line"]!.GetValue<int>(), row["claim"]!.GetValue<string>(), row["scenario"]!.GetValue<string>(),
                row["test"]!.GetValue<string>(), row["status"]!.GetValue<string>())
            .Should().Be((17, "B-001", "Tracing one", "TracesOne, TracesNone", "Partial"));
        claim["scenarios"]!.AsArray()
            .Select(static scenario => (scenario!["title"]!.GetValue<string>(), scenario["file"]!.GetValue<string>(), scenario["line"]!.GetValue<int>()))
            .Should().Equal(("Tracing one", "src/trace/.spec/trace.feature", 4));
        var tests = claim["tests"]!.AsArray().Select(static test => test!.AsObject()).ToArray();
        tests.Select(static test => (test["name"]!.GetValue<string>(), test["resolution"]!.GetValue<string>()))
            .Should().Equal(("TracesOne", "resolved"), ("TracesNone", "unresolved"));
        (tests[0]["file"]!.GetValue<string>(), tests[0]["line"]!.GetValue<int>()).Should().Be(("test/TraceTests.cs", 5));
        tests[1].ContainsKey("file").Should().BeFalse();
        tests[1].ContainsKey("line").Should().BeFalse();
    }

    [Fact]
    public void AClaimWithNoTraceabilityRow_WhenTheTraceIsMade_ShouldLeaveItsRowOutAndCarryNoTest()
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0", ["tests"] = new JsonObject { ["files"] = new JsonArray("test/**/*.cs") } };
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(manifest)
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/trace/.spec/README.md"))
                .WithDocument(DocumentParser.Parse("""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-001 | It traces one claim. | brd | Active |
                    | B-002 | It traces another. | brd | Active |

                    ## 9. Traceability Matrix

                    | Claim ID | Scenario | Test | Status |
                    | -------- | -------- | ---- | ------ |
                    | B-001 | Tracing one | `TracesOne` | Partial |
                    """, "src/trace/.spec/README.md")));

        // When
        var claims = JsonNode.Parse(TraceDocument.From(model, new MatrixResolver()).ToJson())!["specifications"]![0]!["claims"]!.AsArray();

        // Then
        var claim = claims.Should().ContainSingle(static claim => claim!["id"]!.GetValue<string>() == "B-002").Which!.AsObject();
        claim.ContainsKey("row").Should().BeFalse();
        claim["tests"]!.AsArray().Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(TraceSkippingMarkers))]
    public void ARowWhoseStatusIsAMarkerThatSkips_WhenTheTraceIsMade_ShouldGiveEachTestItNamesSkippedWithNoFileOrLine(string marker)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0", ["tests"] = new JsonObject { ["files"] = new JsonArray("test/**/*.cs") } };
        SpechtTestFile file = new SpechtTestFileFixture().WithRelativePath("test/TraceTests.cs").WithText("public void TracesAnother() { }");
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(manifest)
            .WithList([file])
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/trace/.spec/README.md"))
                .WithDocument(DocumentParser.Parse($$"""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-002 | It traces another. | brd | Active |

                    ## 9. Traceability Matrix

                    | Claim ID | Scenario | Test | Status |
                    | -------- | -------- | ---- | ------ |
                    | B-002 | Tracing another | `TracesAnother`, `TracesNowhere` | {{marker}} |
                    """, "src/trace/.spec/README.md")));

        // When
        var tests = JsonNode.Parse(TraceDocument.From(model, new MatrixResolver()).ToJson())!["specifications"]![0]!["claims"]![0]!["tests"]!
            .AsArray().Select(static test => test!.AsObject()).ToArray();

        // Then
        tests.Select(static test => (test["name"]!.GetValue<string>(), test["resolution"]!.GetValue<string>()))
            .Should().Equal(("TracesAnother", "skipped"), ("TracesNowhere", "skipped"));
        tests.Should().AllSatisfy(static test => test.Select(static member => member.Key).Should().Equal("name", "resolution"));
    }

    [Fact]
    public void AManifestWithNoTestsKey_WhenTheTraceIsMade_ShouldGiveEveryTestOutsideASkippingRowUnconfiguredAndNoneUnresolved()
    {
        // Given
        SpechtTestFile file = new SpechtTestFileFixture().WithRelativePath("test/TraceTests.cs").WithText("public void TracesOne() { }");
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(new JsonObject { ["schemaVersion"] = "0.2.0" })
            .WithList([file])
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/trace/.spec/README.md"))
                .WithDocument(DocumentParser.Parse("""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-001 | It traces one claim. | brd | Active |
                    | B-002 | It traces another. | brd | Active |

                    ## 9. Traceability Matrix

                    | Claim ID | Scenario | Test | Status |
                    | -------- | -------- | ---- | ------ |
                    | B-001 | Tracing one | `TracesOne`, `TracesNone` | Partial |
                    | B-002 | Tracing another | `TracesNowhere` | Covered |
                    """, "src/trace/.spec/README.md")));

        // When
        var trace = TraceDocument.From(model, new MatrixResolver());

        // Then
        var tests = JsonNode.Parse(trace.ToJson())!["specifications"]![0]!["claims"]!.AsArray()
            .SelectMany(static claim => claim!["tests"]!.AsArray())
            .Select(static test => test!.AsObject())
            .ToArray();
        tests.Select(static test => (test["name"]!.GetValue<string>(), test["resolution"]!.GetValue<string>()))
            .Should().Equal(("TracesOne", "unconfigured"), ("TracesNone", "unconfigured"), ("TracesNowhere", "unconfigured"));
        tests.Should().AllSatisfy(static test => test.Select(static member => member.Key).Should().Equal("name", "resolution"));
        trace.Lines().Select(static line => line.Split('\t')[4]).Should().Equal("0/2", "0/1");
    }

    [Theory]
    [ClassData(typeof(TraceRulesSwitchedOff))]
    public void TheUnresolvedTestRuleSwitchedOff_WhenTheTraceIsMade_ShouldStillGiveEachTestTheResolutionThatRuleJudges(string pin, string rule)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = pin,
            ["tests"] = new JsonObject { ["files"] = new JsonArray("test/**/*.cs") },
            ["rules"] = new JsonObject { [rule] = "off" },
        };
        SpechtTestFile file = new SpechtTestFileFixture().WithRelativePath("test/TraceTests.cs").WithText("public void TracesOne() { }");
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(manifest)
            .WithList([file])
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/trace/.spec/README.md"))
                .WithDocument(DocumentParser.Parse("""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-001 | It traces one claim. | brd | Active |

                    ## 9. Traceability Matrix

                    | Claim ID | Scenario | Test | Status |
                    | -------- | -------- | ---- | ------ |
                    | B-001 | Tracing one | `TracesOne`, `TracesNone` | Partial |
                    """, "src/trace/.spec/README.md")));

        // When
        var tests = JsonNode.Parse(TraceDocument.From(model, new MatrixResolver()).ToJson())!["specifications"]![0]!["claims"]![0]!["tests"]!
            .AsArray();

        // Then
        model.Schemas.Structure.Rules[rule].Should().Be(SpechtRuleLevel.Off);
        tests.Select(static test => (test!["name"]!.GetValue<string>(), test["resolution"]!.GetValue<string>()))
            .Should().Equal(("TracesOne", "resolved"), ("TracesNone", "unresolved"));
    }

    [Theory]
    [ClassData(typeof(TraceSkippingMarkers))]
    public void AManifestWithNoTestsKeyAndARowWhoseStatusSkips_WhenTheTraceIsMade_ShouldGiveThatRowsTestSkippedAndAnotherRowsUnconfigured(string marker)
    {
        // Given
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(new JsonObject { ["schemaVersion"] = "0.2.0" })
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/trace/.spec/README.md"))
                .WithDocument(DocumentParser.Parse($$"""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-001 | It traces one claim. | brd | Active |
                    | B-002 | It traces another. | brd | Active |

                    ## 9. Traceability Matrix

                    | Claim ID | Scenario | Test | Status |
                    | -------- | -------- | ---- | ------ |
                    | B-001 | Tracing one | `TracesOne` | Covered |
                    | B-002 | Tracing another | `TracesAnother` | {{marker}} |
                    """, "src/trace/.spec/README.md")));

        // When
        var claims = JsonNode.Parse(TraceDocument.From(model, new MatrixResolver()).ToJson())!["specifications"]![0]!["claims"]!.AsArray();

        // Then
        claims.Select(static claim => (
                claim!["id"]!.GetValue<string>(),
                string.Join(',', claim["tests"]!.AsArray().Select(static test => test!["resolution"]!.GetValue<string>()))))
            .Should().Equal(("B-001", "unconfigured"), ("B-002", "skipped"));
    }

    [Fact]
    public void AManifestPinningSchema010WithRenamedMatrixHeaders_WhenTheTraceIsMade_ShouldReadTheColumnsByPosition()
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.1.0",
            ["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim", "Case", "Check", "State") },
        };
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(manifest)
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/trace/.spec/README.md"))
                .WithDocument(DocumentParser.Parse("""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-002 | It traces another. | brd | Active |

                    ## 9. Traceability Matrix

                    | Claim | Case | Check | State |
                    | ----- | ---- | ----- | ----- |
                    | B-002 | Tracing another | `TracesA`, `TracesB`, `TracesC` | Covered |
                    """, "src/trace/.spec/README.md")));

        // When
        var document = TraceDocument.From(model, new MatrixResolver());
        var row = JsonNode.Parse(document.ToJson())!["specifications"]![0]!["claims"]![0]!["row"]!;

        // Then
        (row["claim"]!.GetValue<string>(), row["scenario"]!.GetValue<string>(), row["test"]!.GetValue<string>(), row["status"]!.GetValue<string>())
            .Should().Be(("B-002", "Tracing another", "TracesA, TracesB, TracesC", "Covered"));
        document.Lines().Should().ContainSingle().Which.Split('\t')[2..].Should().Equal("Covered", "0", "0/3");
    }

    [Theory]
    [ClassData(typeof(TraceSkippingMarkers))]
    public void AManifestPinningSchema010AndARowWhoseStatusIsAMarkerThatSkips_WhenTheTraceIsMade_ShouldGiveEachTestItNamesSkipped(string marker)
    {
        // Given
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(new JsonObject { ["schemaVersion"] = "0.1.0" })
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/trace/.spec/README.md"))
                .WithDocument(DocumentParser.Parse($$"""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-001 | It traces one claim. | brd | Active |
                    | B-002 | It traces another. | brd | Active |

                    ## 9. Traceability Matrix

                    | Claim ID | Scenario | Test | Status |
                    | -------- | -------- | ---- | ------ |
                    | B-001 | Tracing one | `TracesOne` | Covered |
                    | B-002 | Tracing another | `TracesAnother`, `TracesNowhere` | {{marker}} |
                    """, "src/trace/.spec/README.md")));

        // When
        var claims = JsonNode.Parse(TraceDocument.From(model, new MatrixResolver()).ToJson())!["specifications"]![0]!["claims"]!.AsArray();

        // Then
        claims.Select(static claim => (
                claim!["id"]!.GetValue<string>(),
                string.Join(',', claim["tests"]!.AsArray().Select(static test => test!["resolution"]!.GetValue<string>()))))
            .Should().Equal(("B-001", "unconfigured"), ("B-002", "skipped,skipped"));
    }

    [Fact]
    public void AManifestPinningSchema010_WhenTheTraceIsMade_ShouldGiveEveryTestOutsideASkippingRowUnconfiguredEvenOneAFileHolds()
    {
        // Given
        SpechtTestFile file = new SpechtTestFileFixture().WithRelativePath("test/TraceTests.cs").WithText("public void TracesOne() { }");
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(new JsonObject { ["schemaVersion"] = "0.1.0" })
            .WithList([file])
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/trace/.spec/README.md"))
                .WithDocument(DocumentParser.Parse("""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-001 | It traces one claim. | brd | Active |

                    ## 9. Traceability Matrix

                    | Claim ID | Scenario | Test | Status |
                    | -------- | -------- | ---- | ------ |
                    | B-001 | Tracing one | `TracesOne`, `TracesNone` | Partial |
                    """, "src/trace/.spec/README.md")));

        // When
        var document = TraceDocument.From(model, new MatrixResolver());
        var tests = JsonNode.Parse(document.ToJson())!["specifications"]![0]!["claims"]![0]!["tests"]!.AsArray()
            .Select(static test => test!.AsObject()).ToArray();

        // Then
        tests.Select(static test => (test["name"]!.GetValue<string>(), test["resolution"]!.GetValue<string>()))
            .Should().Equal(("TracesOne", "unconfigured"), ("TracesNone", "unconfigured"));
        tests.Should().AllSatisfy(static test => test.Select(static member => member.Key).Should().Equal("name", "resolution"));
        document.Lines().Should().ContainSingle().Which.Split('\t')[4].Should().Be("0/2");
    }

    [Theory]
    [ClassData(typeof(TraceLineCases))]
    public void AClaim_WhenItsLineIsMade_ShouldHoldThePathIdStatusScenarioCountAndResolvedOutOfNamedTests(string? row, string expected)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0", ["tests"] = new JsonObject { ["files"] = new JsonArray("test/**/*.cs") } };
        SpechtTestFile file = new SpechtTestFileFixture()
            .WithRelativePath("test/TraceTests.cs")
            .WithText("public void TracesAnother() { }\npublic void TracesAgain() { }\n");
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(manifest)
            .WithList([file])
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/trace/.spec/README.md"))
                .WithDocument(DocumentParser.Parse($$"""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-001 | It traces one claim. | brd | Active |
                    | B-002 | It traces another. | brd | Active |

                    ## 9. Traceability Matrix

                    | Claim ID | Scenario | Test | Status |
                    | -------- | -------- | ---- | ------ |
                    | B-001 | Tracing one | Missing | Missing |
                    {{row}}
                    """, "src/trace/.spec/README.md"))
                .WithFeatureFiles(new FeatureTagParser().Read(
                    "src/trace/.spec/trace.feature",
                    "Feature: Trace\n\n  @B-002\n  Scenario: Tracing another\n    Given a thing\n\n  @B-002\n  Scenario: Tracing again\n    Given a thing\n",
                    EngineServices.Parser().Parse(manifest.ToJsonString()))));

        // When
        var lines = TraceDocument.From(model, new MatrixResolver()).Lines().ToArray();

        // Then
        lines.Should().HaveCount(2);
        lines[1].Should().Be(expected);
    }

    [Fact]
    public void TwoSpecificationsWithClaims_WhenTheLinesAreMade_ShouldGiveOneLinePerClaimInTheDocumentsOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/zeta/.spec/README.md"))
                .WithDocument(DocumentParser.Parse("""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-002 | It traces second. | brd | Active |
                    | B-001 | It traces first. | brd | Active |
                    """, "src/zeta/.spec/README.md")),
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/alpha/.spec/README.md"))
                .WithDocument(DocumentParser.Parse("""
                    ## 3. Acceptance Criteria

                    | ID | Claim | Source | Status |
                    | -- | ----- | ------ | ------ |
                    | B-001 | It traces. | brd | Active |
                    """, "src/alpha/.spec/README.md")));

        // When
        var lines = TraceDocument.From(model, new MatrixResolver()).Lines().ToArray();

        // Then
        lines.Select(static line => string.Join('\t', line.Split('\t').Take(2)))
            .Should().Equal("src/zeta/.spec/README.md\tB-002", "src/zeta/.spec/README.md\tB-001", "src/alpha/.spec/README.md\tB-001");
        lines.Should().AllSatisfy(static line => line.Split('\t').Should().HaveCount(5));
    }

    private static SpechtDocumentParser DocumentParser => new(new FrontmatterReader());
}
