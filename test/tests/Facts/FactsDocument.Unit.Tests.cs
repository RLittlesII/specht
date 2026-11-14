using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Facts;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Facts;

/// <summary>
/// The facts document projected from a model built in memory (<c>0132-F2</c> B-001, B-003 to B-008, B-011 to B-013,
/// B-015, B-017 to B-020, B-022, B-025, B-026): every Feature specification in the model's order and no epic, each with
/// the path, identity, statuses and dependency lists its frontmatter gives, every claim, constraint, out-of-scope and
/// open-question row with its cells and line and no coverage, each open question's resolved state on OQ-2's default, an
/// out-of-scope section missing or with no table leaving only its own rows out, the records it is handed beside each
/// Feature with the number and status OQ-3's default reads, the same members under two pins, the same text for the same
/// model, a selection by a specification's path or by a directory, by whole path segments in the document's order,
/// or of nothing, and the text lines (B-024, OQ-5's default): one per claim, constraint, out-of-scope row and open
/// question in the document's order, five tab-separated fields, a missing text an empty field, and a tab or line break
/// inside a field written as a space. Each lessons-and-deltas entry, a list item's text or a table row's cells, with its line in
/// line order (B-009), each sign-off row's section, status, reviewer, note and line under the headers the manifest's
/// <c>tables</c> names or the engine's (B-010), a claims or sign-off section the manifest's <c>roles</c> renames read under
/// its new title, and a section the manifest's <c>sections</c> does not list leaving only its own member empty (B-016).
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

    [Fact]
    public void ASpecificationsPath_WhenSelected_ShouldGiveThatSpecificationOnly()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/tool/Features/Facts/.spec/README.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/toolbox/.spec/README.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("epics/0001-epic/F1-feature/spec.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/tool/.spec/README.md")));
        var document = FactsDocument.From(model, NoRecords);

        // When
        var selected = JsonNode.Parse(document.Select("src/tool/.spec/README.md").ToJson())!;

        // Then
        selected["specifications"]!.AsArray().Select(static specification => specification!["path"]!.GetValue<string>()).Should().Equal(
            "src/tool/.spec/README.md");
    }

    [Theory]
    [ClassData(typeof(FactsSelectedDirectories))]
    public void ADirectory_WhenSelected_ShouldGiveEverySpecificationUnderItByWholePathSegmentsInTheDocumentsOrder(string directory, string[] expected)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/tool/Features/Facts/.spec/README.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/toolbox/.spec/README.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("epics/0001-epic/F1-feature/spec.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/tool/.spec/README.md")));
        var document = FactsDocument.From(model, NoRecords);

        // When
        var selected = JsonNode.Parse(document.Select(directory).ToJson())!;

        // Then
        selected["specifications"]!.AsArray().Select(static specification => specification!["path"]!.GetValue<string>()).Should().Equal(expected);
    }

    [Theory]
    [ClassData(typeof(FactsUnselectingPaths))]
    public void APathNamingNoSpecification_WhenSelected_ShouldGiveNoSpecification(string path)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/tool/Features/Facts/.spec/README.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/toolbox/.spec/README.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/tool/.spec/README.md")));
        var document = FactsDocument.From(model, NoRecords);
        JsonNode.Parse(document.ToJson())!["specifications"]!.AsArray().Should().HaveCount(3);

        // When
        var selected = JsonNode.Parse(document.Select(path).ToJson())!;

        // Then
        selected["specifications"].Should().BeOfType<JsonArray>().Which.Should().BeEmpty();
    }

    [Fact]
    public void FeaturesWithEveryKindOfFactAndRecords_WhenLinesAreMade_ShouldGiveOneFiveFieldLinePerFactInTheDocumentsOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/zeta/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/zeta/.spec/README.md")
                    .WithSections(
                        new SpechtSection(
                            "3. Acceptance Criteria", 10, ["ID", "Claim", "Source", "Status"], [["B-001", "It holds.", "the owner", "Active"]], [14]),
                        new SpechtSection(
                            "11. Open Questions", 36, ["ID", "Question", "Blocks", "Resolution"], [["OQ-2", "Which comes last?", "B-001", "Open"]], [40]))),
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/area/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/area/.spec/README.md")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject
                    {
                        ["epic"] = "0001",
                        ["id"] = "F1",
                        ["spec_status"] = "approved",
                        ["status"] = "ready",
                        ["depends_on"] = new JsonArray("F2"),
                    }))
                    .WithSections(
                        new SpechtSection(
                            "3. Acceptance Criteria",
                            57,
                            ["ID", "Claim", "Source", "Status"],
                            [
                                ["B-001", "Given a tree, this Feature gives its facts.", "the owner", "Active"],
                                ["B-002", "Given a flag, this Feature writes one document.", "the owner", "Withdrawn"],
                            ],
                            [61, 62]),
                        new SpechtSection(
                            "4. Constraints", 64, ["ID", "Constraint", "Rules Out"], [["C-1", "Offline and read-only.", "A network call."]], [68]),
                        new SpechtSection("5. Out of Scope", 73, ["#", "Item", "Exclusion Reason"], [["1", "Any other input", "Owned elsewhere"]], [77]),
                        new SpechtSection(
                            "11. Open Questions",
                            108,
                            ["ID", "Question", "Blocks", "Resolution"],
                            [["OQ-1", "Which input comes first?", "B-001", "Resolved 2026-10-30 by the owner: the tree"]],
                            [112]))),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/empty/.spec/README.md")));
        var records = new Dictionary<FeatureSpec, IReadOnlyList<RecordFile>>
        {
            [model.Features[1]] = [new RecordFileFixture().WithRelativePath("src/area/.spec/adr/0001-an-adr.md").WithKind("adr")],
        };

        // When
        var lines = FactsDocument.From(model, records).Lines();

        // Then
        lines.Should().Equal(
            "src/zeta/.spec/README.md\tclaim\tB-001\t14\tIt holds.",
            "src/zeta/.spec/README.md\topenQuestion\tOQ-2\t40\tWhich comes last?",
            "src/area/.spec/README.md\tclaim\tB-001\t61\tGiven a tree, this Feature gives its facts.",
            "src/area/.spec/README.md\tclaim\tB-002\t62\tGiven a flag, this Feature writes one document.",
            "src/area/.spec/README.md\tconstraint\tC-1\t68\tOffline and read-only.",
            "src/area/.spec/README.md\toutOfScope\t1\t77\tAny other input",
            "src/area/.spec/README.md\topenQuestion\tOQ-1\t112\tWhich input comes first?");
    }

    [Fact]
    public void RowsUnderTablesLackingTheirTextHeader_WhenLinesAreMade_ShouldWriteEachMissingTextAsAnEmptyFifthField()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/area/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/area/.spec/README.md")
                    .WithSections(
                        new SpechtSection("3. Acceptance Criteria", 10, ["ID", "Source", "Status"], [["B-001", "the owner", "Active"]], [14]),
                        new SpechtSection("4. Constraints", 20, ["ID", "Rules Out"], [["C-1", "A network call."]], [24]),
                        new SpechtSection("5. Out of Scope", 30, ["#", "Exclusion Reason"], [["1", "Owned elsewhere"]], [34]),
                        new SpechtSection("11. Open Questions", 110, ["ID", "Blocks", "Resolution"], [["OQ-1", "B-001", "Open"]], [114]))));
        var document = FactsDocument.From(model, NoRecords);
        JsonNode.Parse(document.ToJson())!["specifications"]![0]!["claims"]![0]!.AsObject().Should().ContainKey("text")
            .WhoseValue.Should().BeNull();

        // When
        var lines = document.Lines();

        // Then
        lines.Should().Equal(
            "src/area/.spec/README.md\tclaim\tB-001\t14\t",
            "src/area/.spec/README.md\tconstraint\tC-1\t24\t",
            "src/area/.spec/README.md\toutOfScope\t1\t34\t",
            "src/area/.spec/README.md\topenQuestion\tOQ-1\t114\t");
    }

    [Theory]
    [ClassData(typeof(FactsLineBreakingCharacters))]
    public void ATabOrLineBreakInsideAField_WhenLinesAreMade_ShouldWriteEachSuchCharacterAsOneSpaceAndKeepOneLinePerFact(string character, string written)
    {
        // Given
        var path = $"src/a{character}b/.spec/README.md";
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath(path)
                    .WithSections(
                        new SpechtSection(
                            "3. Acceptance Criteria",
                            10,
                            ["ID", "Claim", "Source", "Status"],
                            [[$"B-0{character}01", $"Given a{character}tree.", "the owner", "Active"]],
                            [14]),
                        new SpechtSection(
                            "4. Constraints", 20, ["ID", "Constraint", "Rules Out"], [[$"C-{character}1", $"Read{character}only.", "A write."]], [24]),
                        new SpechtSection(
                            "5. Out of Scope", 30, ["#", "Item", "Exclusion Reason"], [[$"1{character}2", $"Any{character}write", "Not asked"]], [34]),
                        new SpechtSection(
                            "11. Open Questions",
                            110,
                            ["ID", "Question", "Blocks", "Resolution"],
                            [[$"OQ-{character}1", $"Which{character}one?", "B-001", "Open"]],
                            [114]))));

        // When
        var lines = FactsDocument.From(model, NoRecords).Lines();

        // Then
        var shown = $"src/a{written}b/.spec/README.md";
        lines.Should().Equal(
            $"{shown}\tclaim\tB-0{written}01\t14\tGiven a{written}tree.",
            $"{shown}\tconstraint\tC-{written}1\t24\tRead{written}only.",
            $"{shown}\toutOfScope\t1{written}2\t34\tAny{written}write",
            $"{shown}\topenQuestion\tOQ-{written}1\t114\tWhich{written}one?");
    }

    [Theory]
    [ClassData(typeof(FactsDeltaSections))]
    public void ALessonsAndDeltasSectionsListItemsAndTableRows_WhenTheDocumentIsMade_ShouldCarryEachEntryWithItsTextOrCellsAndLineInLineOrder(
        string[] items,
        int[] itemLines,
        string[] headers,
        string[][] rows,
        int[] rowLines,
        string?[] texts,
        string[][] cells,
        int[] lines)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithSections(new SpechtSection("10. Lessons / Spec Deltas", 128, headers, rows, rowLines) { Items = items, ItemLines = itemLines })));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["deltas"].Should().BeOfType<JsonArray>().Which.Select(static entry => new
        {
            Text = entry!["text"]?.GetValue<string>(),
            Cells = entry["cells"]!.AsArray().Select(static cell => cell!.GetValue<string>()).ToArray(),
            Line = entry["line"]!.GetValue<int>(),
        })
            .Should().BeEquivalentTo(
                texts.Select((text, index) => new { Text = text, Cells = cells[index], Line = lines[index] }),
                static options => options.WithStrictOrdering());
        specification["deltas"]!.AsArray().Select(static entry => entry!.AsObject().Select(static member => member.Key).ToArray())
            .Should().AllSatisfy(static members => members.Should().Equal("text", "cells", "line"));
    }

    [Fact]
    public void ASpecificationWithNoLessonsAndDeltasOrSignOffSection_WhenTheDocumentIsMade_ShouldCarryNoEntryAndNoSignOffRowAndItsClaims()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithSections(new SpechtSection(
                        "3. Acceptance Criteria", 10, ["ID", "Claim", "Source", "Status"], [["B-001", "It holds.", "the owner", "Active"]], [14]))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["deltas"].Should().BeOfType<JsonArray>().Which.Should().BeEmpty();
        specification["signOff"].Should().BeOfType<JsonArray>().Which.Should().BeEmpty();
        specification["claims"].Should().BeOfType<JsonArray>().Which
            .Select(static claim => (claim!["id"]!.GetValue<string>(), claim["line"]!.GetValue<int>())).Should().Equal(("B-001", 14));
    }

    [Theory]
    [ClassData(typeof(FactsUnlistedSections))]
    public void ASectionTheManifestsSectionsDoesNotList_WhenTheDocumentIsMade_ShouldGiveItsMemberEmptyAndEveryOtherMemberItsRow(string title, string member)
    {
        // Given
        var listed = ((SpechtModel) new SpechtModelFixture().WithManifest(new JsonObject { ["schemaVersion"] = "0.2.0" })).Schemas.Structure.Sections;
        listed.Should().Contain(title);
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(new JsonObject
            {
                ["schemaVersion"] = "0.2.0",
                ["sections"] = new JsonArray(listed.Where(entry => entry != title).Select(static entry => (JsonNode?) entry).ToArray()),
            })
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithSections(
                        new SpechtSection(
                            "4. Constraints", 20, ["ID", "Constraint", "Rules Out"], [["C-1", "Offline and read-only.", "A network call."]], [24]),
                        new SpechtSection("5. Out of Scope", 30, ["#", "Item", "Exclusion Reason"], [["1", "Any other input", "Owned elsewhere"]], [34]),
                        new SpechtSection("10. Lessons / Spec Deltas", 100, [], [], []) { Items = ["A lesson."], ItemLines = [102] },
                        new SpechtSection(
                            "11. Open Questions",
                            110,
                            ["ID", "Question", "Blocks", "Resolution"],
                            [["OQ-1", "Which input comes first?", "B-001", "Open"]],
                            [114]))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        new[] { "constraints", "outOfScope", "deltas", "openQuestions" }
            .Select(name => (name, specification[name].Should().BeOfType<JsonArray>().Which.Count))
            .Should().Equal(new[] { "constraints", "outOfScope", "deltas", "openQuestions" }.Select(name => (name, name == member ? 0 : 1)));
    }

    [Theory]
    [ClassData(typeof(FactsSignOffTables))]
    public void ASignOffTableUnderTheManifestsOrTheEnginesHeaders_WhenTheDocumentIsMade_ShouldCarryEachRowsSectionStatusReviewerNoteAndLine(
        string manifest,
        string[] headers,
        string[] row,
        string? section,
        string? status,
        string? reviewer,
        string? note)
    {
        // Given
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(JsonNode.Parse(manifest)!.AsObject())
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithSections(new SpechtSection("12. Sign-off", 148, headers, [row], [152]))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        var given = specification["signOff"].Should().BeOfType<JsonArray>().Which.Should().ContainSingle().Which!.AsObject();
        given.Select(static member => member.Key).Should().Equal("section", "status", "reviewer", "note", "line");
        (given["section"]?.GetValue<string>(),
                given["status"]?.GetValue<string>(),
                given["reviewer"]?.GetValue<string>(),
                given["note"]?.GetValue<string>(),
                given["line"]!.GetValue<int>())
            .Should().Be((section, status, reviewer, note, 152));
    }

    [Theory]
    [ClassData(typeof(FactsRenamedRoles))]
    public void ASectionTheManifestsRolesRename_WhenTheDocumentIsMade_ShouldReadItsRowsUnderTheNewTitleAndNotTheDefault(
        string role,
        string defaultTitle,
        string renamed,
        string member,
        string[] headers,
        string[] row)
    {
        // Given
        var listed = ((SpechtModel) new SpechtModelFixture().WithManifest(new JsonObject { ["schemaVersion"] = "0.2.0" })).Schemas.Structure.Sections;
        listed.Should().Contain(defaultTitle);
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(new JsonObject
            {
                ["schemaVersion"] = "0.2.0",
                ["sections"] = new JsonArray(listed.Select(entry => (JsonNode?) (entry == defaultTitle ? renamed : entry)).ToArray()),
                ["roles"] = new JsonObject { [role] = renamed },
            })
            .WithFeatures(new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithSections(
                        new SpechtSection(defaultTitle, 10, headers, [row, row], [14, 15]),
                        new SpechtSection(renamed, 20, headers, [row, row], [24, 25]))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model, NoRecords).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification[member].Should().BeOfType<JsonArray>().Which.Select(static entry => entry!["line"]!.GetValue<int>()).Should().Equal(24, 25);
    }

    private static readonly Dictionary<FeatureSpec, IReadOnlyList<RecordFile>> NoRecords = [];
}
