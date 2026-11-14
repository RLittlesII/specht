using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Facts;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Facts;

/// <summary>
/// The facts document projected from a model built in memory (<c>0132-F2</c> B-001, B-003 to B-008, B-011 to B-013,
/// B-015, B-017, B-022, B-025, B-026): every Feature specification in the model's order and no epic, each with the path,
/// identity, statuses and dependency lists its frontmatter gives, every claim, constraint, out-of-scope and open-question
/// row with its cells and line and no coverage, each open question's resolved state on OQ-2's default, an out-of-scope
/// section missing or with no table leaving only its own rows out, the records it is handed beside each Feature with the
/// number and status OQ-3's default reads, the same members under two pins, and the same text for the same model.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class FactsDocumentUnitTests
{
    [Fact]
    public void FeaturesInTheModelsOrder_WhenTheDocumentIsMade_ShouldListEverySpecificationInThatOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/zeta/.spec/README.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("epics/0001-epic/F1-feature/spec.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/alpha/.spec/README.md")));

        // When
        var document = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!;

        // Then
        document["specifications"]!.AsArray().Select(static specification => specification!["path"]!.GetValue<string>()).Should().Equal(
            "src/zeta/.spec/README.md", "epics/0001-epic/F1-feature/spec.md", "src/alpha/.spec/README.md");
    }

    [Fact]
    public void AnEpicFileBesideAFeature_WhenTheDocumentIsMade_ShouldGiveOnlyTheFeature()
    {
        // Given
        EpicFile epic = new EpicFileFixture().WithRelativePath("epics/0001-epic/epic.md");
        SpechtModel model = new SpechtModelFixture()
            .WithList([epic])
            .WithFeatures(new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("epics/0001-epic/F1-feature/spec.md")));

        // When
        var document = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!;

        // Then
        document["specifications"]!.AsArray().Select(static specification => specification!["path"]!.GetValue<string>()).Should().Equal(
            "epics/0001-epic/F1-feature/spec.md");
    }

    [Fact]
    public void AFeatureWithAnIdentity_WhenTheDocumentIsMade_ShouldCarryItsPathQualifiedIdAndBothStatuses()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/facts/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/facts/.spec/README.md")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject
                    {
                        ["epic"] = "0132",
                        ["id"] = "F2",
                        ["spec_status"] = "approved",
                        ["status"] = "in-progress",
                    }))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["path"]!.GetValue<string>().Should().Be("src/facts/.spec/README.md");
        specification["id"]!.GetValue<string>().Should().Be("0132-F2");
        specification["specStatus"]!.GetValue<string>().Should().Be("approved");
        specification["status"]!.GetValue<string>().Should().Be("in-progress");
    }

    [Fact]
    public void DependencyListsTheCheckCannotResolve_WhenTheDocumentIsMade_ShouldCarryEachAsTheFrontmatterWritesIt()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject
                    {
                        ["epic"] = "0001",
                        ["id"] = "F1",
                        ["depends_on"] = new JsonArray("F9", "0002/F3"),
                        ["blocks"] = new JsonArray("F4", "F2"),
                        ["children"] = new JsonArray("0400"),
                    }))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["dependsOn"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("F9", "0002/F3");
        specification["blocks"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("F4", "F2");
        specification["children"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("0400");
    }

    [Theory]
    [ClassData(typeof(FactsPinPairs))]
    public void TheSameFeaturesUnderTwoPins_WhenEachDocumentIsMade_ShouldCarryTheSameMembers(string firstPin, string secondPin)
    {
        // Given
        FeatureSpec[] features =
        [
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject
                    {
                        ["epic"] = "0001",
                        ["id"] = "F1",
                        ["spec_status"] = "draft",
                        ["status"] = "ready",
                        ["depends_on"] = new JsonArray("F2"),
                    }))),
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/second/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/second/.spec/README.md")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["id"] = "F2" }))),
        ];
        SpechtModel first = new SpechtModelFixture().WithManifest(new JsonObject { ["schemaVersion"] = firstPin }).WithFeatures(features);
        SpechtModel second = new SpechtModelFixture().WithManifest(new JsonObject { ["schemaVersion"] = secondPin }).WithFeatures(features);

        // When
        var documents = new[] { first, second }.Select(static model => JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!).ToArray();

        // Then
        var members = documents.Select(static document => document["specifications"]!.AsArray()
            .Select(static specification => string.Join(',', specification!.AsObject().Select(static member => member.Key)))
            .Prepend(string.Join(',', document.AsObject().Select(static member => member.Key)))
            .ToArray()).ToArray();
        members[0].Should().HaveCount(3);
        members[1].Should().Equal(members[0]);
    }

    [Fact]
    public void TheSameFeaturesReadIntoTwoModels_WhenEachDocumentIsSerialized_ShouldGiveTheSameText()
    {
        // Given
        var models = Enumerable.Range(0, 2).Select(static _ => (SpechtModel) new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject
                    {
                        ["epic"] = "0001",
                        ["id"] = "F1",
                        ["spec_status"] = "draft",
                        ["status"] = "ready",
                        ["depends_on"] = new JsonArray("F2"),
                        ["blocks"] = new JsonArray(),
                    }))),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/second/.spec/README.md")))).ToArray();

        // When
        var first = FactsDocument.From(models[0], NoRecords).ToJson();
        var again = FactsDocument.From(models[0], NoRecords).ToJson();
        var second = FactsDocument.From(models[1], NoRecords).ToJson();

        // Then
        first.Should().NotBeEmpty();
        again.Should().Be(first);
        second.Should().Be(first);
    }

    [Theory]
    [ClassData(typeof(FactsClaimTables))]
    public void ClaimRowsUnderTheirTablesHeaders_WhenTheDocumentIsMade_ShouldCarryEachClaimsIdTextStatusAndLine(
        string[] headers, string[] first, string[] second)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithSections(new SpechtSection("3. Acceptance Criteria", 10, headers, [first, second], [14, 15]))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["claims"].Should().BeOfType<JsonArray>().Which.Select(static claim => (
                claim!["id"]!.GetValue<string>(),
                claim["text"]!.GetValue<string>(),
                claim["status"]!.GetValue<string>(),
                claim["line"]!.GetValue<int>()))
            .Should().Equal(
                ("B-001", "Given a tree, this Feature gives its facts.", "Active", 14),
                ("B-002", "Given a flag, this Feature writes one document.", "Proposed", 15));
    }

    [Fact]
    public void ConstraintRows_WhenTheDocumentIsMade_ShouldCarryEachConstraintsIdTextWhatItRulesOutAndLine()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithSections(new SpechtSection(
                        "4. Constraints",
                        20,
                        ["ID", "Constraint", "Rules Out"],
                        [["C-1", "Offline and read-only.", "A network call."], ["C-2", "One parser.", "A second parser."]],
                        [24, 25]))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["constraints"].Should().BeOfType<JsonArray>().Which.Select(static constraint => (
                constraint!["id"]!.GetValue<string>(),
                constraint["text"]!.GetValue<string>(),
                constraint["rulesOut"]!.GetValue<string>(),
                constraint["line"]!.GetValue<int>()))
            .Should().Equal(("C-1", "Offline and read-only.", "A network call.", 24), ("C-2", "One parser.", "A second parser.", 25));
    }

    [Theory]
    [ClassData(typeof(FactsRetiredClaimStatuses))]
    public void AClaimWhoseStatusIsTheWithdrawnOrSupersededMarker_WhenTheDocumentIsMade_ShouldStillGiveItWithThatStatus(string status)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithSections(new SpechtSection(
                        "3. Acceptance Criteria",
                        10,
                        ["ID", "Claim", "Source", "Status"],
                        [["B-001", "It holds.", "the owner", "Active"], ["B-002", "It held.", "the owner", status]],
                        [14, 15]))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["claims"].Should().BeOfType<JsonArray>().Which
            .Select(static claim => (claim!["id"]!.GetValue<string>(), claim["status"]!.GetValue<string>(), claim["line"]!.GetValue<int>()))
            .Should().Equal(("B-001", "Active", 14), ("B-002", status, 15));
    }

    [Fact]
    public void TwoClaimsSharingAnIdBesideOneTheGrammarRefuses_WhenTheDocumentIsMade_ShouldGiveEveryRowInItsOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithSections(new SpechtSection(
                        "3. Acceptance Criteria",
                        10,
                        ["ID", "Claim", "Source", "Status"],
                        [
                            ["B-001", "It holds.", "the owner", "Active"],
                            ["B-001", "It holds again.", "the owner", "Active"],
                            ["claim one", "It has no id.", "the owner", "Active"],
                        ],
                        [14, 15, 16]))));
        model.Claims[model.Features[0]].Rows.Select(static row => (row.Id, row.IsValid)).Should().Equal(
            ("B-001", true), ("B-001", true), ("claim one", false));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["claims"].Should().BeOfType<JsonArray>().Which
            .Select(static claim => (claim!["id"]!.GetValue<string>(), claim["text"]!.GetValue<string>(), claim["line"]!.GetValue<int>()))
            .Should().Equal(("B-001", "It holds.", 14), ("B-001", "It holds again.", 15), ("claim one", "It has no id.", 16));
    }

    [Fact]
    public void AClaimWithATraceabilityRowATaggedScenarioAndATestFile_WhenTheDocumentIsMade_ShouldCarryNoneOfThem()
    {
        // Given
        SpechtModel model = new SpechtModelFixture()
            .WithList([new SpechtTestFile("test/ZebraTests.cs", "public void Crossing() { } // B-001")])
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithSections(
                        new SpechtSection(
                            "3. Acceptance Criteria",
                            10,
                            ["ID", "Claim", "Source", "Status"],
                            [["B-001", "It holds.", "the owner", "Active"]],
                            [14]),
                        new SpechtSection(
                            "9. Traceability Matrix",
                            40,
                            ["Claim ID", "Scenario", "Test", "Status"],
                            [["B-001", "Crossing the zebra", "ZebraTests.Crossing", "Covered"]],
                            [44])))
                .WithFeatureFiles(new FeatureTagParser().Read(
                    "src/first/.spec/first.feature",
                    "Feature: it\n\n  @B-001\n  Scenario: Crossing the zebra\n    Given a thing\n")));
        model.MatrixSections[model.Features[0]]!.Rows.Should().ContainSingle();
        model.Features[0].FeatureFiles.Should().ContainSingle().Which.Scenarios.Should().ContainSingle();

        // When
        var json = FactsDocument.From(model, NoRecords).ToJson();

        // Then
        JsonNode.Parse(json)!["specifications"]![0]!["claims"].Should().BeOfType<JsonArray>().Which.Should().ContainSingle()
            .Which!["id"]!.GetValue<string>().Should().Be("B-001");
        json.Should().NotContainEquivalentOf("zebra").And.NotContain("Covered");
    }

    [Fact]
    public void OutOfScopeRows_WhenTheDocumentIsMade_ShouldCarryEachRowsNumberItemReasonAndLine()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithSections(new SpechtSection(
                        "5. Out of Scope",
                        30,
                        ["#", "Item", "Exclusion Reason"],
                        [["1", "Any other input", "Owned by a sibling Feature"], ["2", "Any write", "Not asked for"]],
                        [34, 35]))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["outOfScope"].Should().BeOfType<JsonArray>().Which.Select(static row => (
                row!["number"]!.GetValue<string>(),
                row["item"]!.GetValue<string>(),
                row["reason"]!.GetValue<string>(),
                row["line"]!.GetValue<int>()))
            .Should().Equal(("1", "Any other input", "Owned by a sibling Feature", 34), ("2", "Any write", "Not asked for", 35));
    }

    [Theory]
    [ClassData(typeof(FactsOpenQuestionResolutions))]
    public void AnOpenQuestionsResolution_WhenTheDocumentIsMade_ShouldCarryItsIdQuestionBlocksResolutionLineAndWhetherItIsResolved(
        string resolution, bool resolved)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithSections(new SpechtSection(
                        "11. Open Questions",
                        110,
                        ["ID", "Question", "Blocks", "Resolution"],
                        [["OQ-1", "Which input comes first?", "B-001, B-002", resolution]],
                        [114]))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["openQuestions"].Should().BeOfType<JsonArray>().Which.Select(static question => (
                question!["id"]!.GetValue<string>(),
                question["question"]!.GetValue<string>(),
                question["blocks"]!.GetValue<string>(),
                question["resolution"]!.GetValue<string>(),
                question["resolved"]!.GetValue<bool>(),
                question["line"]!.GetValue<int>()))
            .Should().Equal(("OQ-1", "Which input comes first?", "B-001, B-002", resolution, resolved, 114));
    }

    [Theory]
    [ClassData(typeof(FactsOutOfScopeFaults))]
    public void AnOutOfScopeSectionMissingOrWithNoTable_WhenTheDocumentIsMade_ShouldGiveNoOutOfScopeRowsAndEveryOtherRow(bool sectionPresent)
    {
        // Given
        SpechtSection[] sections =
        [
            new("3. Acceptance Criteria", 10, ["ID", "Claim", "Source", "Status"], [["B-001", "It holds.", "the owner", "Active"]], [14]),
            new("4. Constraints", 20, ["ID", "Constraint", "Rules Out"], [["C-1", "Offline and read-only.", "A network call."]], [24]),
            .. sectionPresent ? new SpechtSection[] { new("5. Out of Scope", 30, [], [], []) } : [],
            new("11. Open Questions", 110, ["ID", "Question", "Blocks", "Resolution"], [["OQ-1", "Which input comes first?", "B-001", "Open"]], [114]),
        ];
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture().WithRelativePath("src/first/.spec/README.md").WithSections(sections)));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["outOfScope"].Should().BeOfType<JsonArray>().Which.Should().BeEmpty();
        specification["claims"].Should().BeOfType<JsonArray>().Which
            .Select(static claim => (claim!["id"]!.GetValue<string>(), claim["line"]!.GetValue<int>())).Should().Equal(("B-001", 14));
        specification["constraints"].Should().BeOfType<JsonArray>().Which
            .Select(static constraint => (constraint!["id"]!.GetValue<string>(), constraint["line"]!.GetValue<int>())).Should().Equal(("C-1", 24));
        specification["openQuestions"].Should().BeOfType<JsonArray>().Which
            .Select(static question => (question!["id"]!.GetValue<string>(), question["line"]!.GetValue<int>())).Should().Equal(("OQ-1", 114));
    }

    [Fact]
    public void RecordsBesideOneFeatureAndNoneBesideAnother_WhenTheDocumentIsMade_ShouldCarryEachRecordsKindNumberTitleStatusAndPathAndNoRecordForTheOther()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/area/.spec/README.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/other/.spec/README.md")));
        var records = new Dictionary<FeatureSpec, IReadOnlyList<RecordFile>>
        {
            [model.Features[0]] =
            [
                new RecordFileFixture()
                    .WithRelativePath("src/area/.spec/adr/0001-read-each-record-as-written.md")
                    .WithKind("adr")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["title"] = "ADR-0001: Read each record as written" }))
                    .WithStatusLine("Accepted, 2026-10-12, by the owner"),
                new RecordFileFixture()
                    .WithRelativePath("src/area/.spec/decisions/0001-the-records-are-given.md")
                    .WithKind("decision")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["title"] = "Decision 0001: the records are given" }))
                    .WithStatusLine("Accepted, 2026-10-12, by the owner"),
                new RecordFileFixture()
                    .WithRelativePath("src/area/.spec/lessons/0001-a-lesson-with-no-frontmatter.md")
                    .WithKind("lesson")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(null))
                    .WithStatusLine("Accepted, 2026-10-12, by the owner"),
            ],
        };

        // When
        var specifications = JsonNode.Parse(FactsDocument.From(model, records).ToJson())!["specifications"]!.AsArray();

        // Then
        specifications[0]!["records"].Should().BeOfType<JsonArray>().Which.Select(static record => (
                record!["kind"]!.GetValue<string>(),
                record["number"]!.GetValue<string>(),
                record["title"]?.GetValue<string>(),
                record["status"]?.GetValue<string>(),
                record["path"]!.GetValue<string>()))
            .Should().Equal(
                ("adr", "0001", "ADR-0001: Read each record as written", "Accepted", "src/area/.spec/adr/0001-read-each-record-as-written.md"),
                ("decision", "0001", "Decision 0001: the records are given", null, "src/area/.spec/decisions/0001-the-records-are-given.md"),
                ("lesson", "0001", null, null, "src/area/.spec/lessons/0001-a-lesson-with-no-frontmatter.md"));
        specifications[0]!["records"]!.AsArray().Select(static record => record!.AsObject().Select(static member => member.Key).ToArray())
            .Should().AllSatisfy(static members => members.Should().Equal("kind", "number", "title", "status", "path"));
        specifications[1]!["records"].Should().BeOfType<JsonArray>().Which.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(FactsRecordNumbers))]
    public void ARecordsFileName_WhenTheDocumentIsMade_ShouldCarryItsFirstWordAsTheNumber(string path, string? number)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/area/.spec/README.md")));
        var records = new Dictionary<FeatureSpec, IReadOnlyList<RecordFile>>
        {
            [model.Features[0]] = [new RecordFileFixture().WithRelativePath(path).WithKind("decision")],
        };

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, records).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        var record = specification["records"].Should().BeOfType<JsonArray>().Which.Should().ContainSingle().Which!.AsObject();
        record.Should().ContainKey("number");
        (record["number"]?.GetValue<string>()).Should().Be(number);
        record["path"]!.GetValue<string>().Should().Be(path);
    }

    [Theory]
    [ClassData(typeof(FactsAdrStatusLines))]
    public void AnAdrsStatusLine_WhenTheDocumentIsMade_ShouldCarryItsFirstWordAsWrittenAsTheStatus(string? statusLine, string? status)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/area/.spec/README.md")));
        var records = new Dictionary<FeatureSpec, IReadOnlyList<RecordFile>>
        {
            [model.Features[0]] = [new RecordFileFixture().WithKind("adr").WithStatusLine(statusLine)],
        };

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, records).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        var record = specification["records"].Should().BeOfType<JsonArray>().Which.Should().ContainSingle().Which!.AsObject();
        record.Should().ContainKey("status");
        (record["status"]?.GetValue<string>()).Should().Be(status);
    }

    private static readonly Dictionary<FeatureSpec, IReadOnlyList<RecordFile>> NoRecords = [];
}
