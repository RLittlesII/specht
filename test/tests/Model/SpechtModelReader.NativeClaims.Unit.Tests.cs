using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>
/// A synthetic tree in an in-memory file system, read by the model reader under a schema 0.4.0 manifest whose
/// <c>claim.source</c> is <c>gherkin</c> (<c>0001-F16</c> B-001, B-004 to B-007, B-011 to B-013, B-030; C-1, C-2, C-4,
/// C-5; § 7, item <c>0390</c>): each claim file set's claims are read from the <c>.feature</c> files its glob matches
/// outside <c>exclusions</c>, with the tag form, grammar and tag lists the manifest gives, onto
/// <see cref="SpechtModel.ClaimSets"/>; and each set is one native Feature in <see cref="SpechtModel.Features"/>, whose
/// lifecycle and edges its <c>record</c> gives, whose identity is the set's and whose claim grammar no record chooses,
/// located at its record or else its first claim file, and none for a set with neither (B-003, B-017, B-026, B-032,
/// B-036, B-037; § 7, item <c>0391</c>). Unless a test says otherwise, the manifest holds one set, glob
/// <c>.spec/features/arithmetic/*.feature</c> and feature <c>F1</c>, and excludes <c>obj</c> alone.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtModelReaderNativeClaimsUnitTests
{
    [Fact]
    public void AClaimFileSet_WhenReadUnderAGherkinClaimSource_ShouldBuildItsClaimsFromTheFeatureFilesItsGlobMatchesOutsideExclusionsWithNoClaimsTable()
    {
        // Given
        const string Members = """{ "claim": { "files": [ { "glob": ".spec/features/**/*", "feature": "F1" } ] } }""";

        // When
        var model = Read(
            Members,
            (Add, "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n\n  @B-002\n  Scenario: Adds three\n    Given three numbers\n"),
            (".spec/features/arithmetic/notes.md", "@B-005\n"),
            (".spec/features/obj/old.feature", "Feature: Old\n\n  @B-006\n  Scenario: Gone\n    Given nothing\n"));

        // Then
        var set = model.ClaimSets.Should().ContainSingle().Subject;
        set.FileSet.Should().Be(new SpechtClaimFileSet(".spec/features/**/*", "F1", null, null));
        set.Section.Should().BeNull();
        set.Rows.Select(static claim => (claim.Path, claim.Id)).Should().Equal((Add, "B-001"), (Add, "B-002"));
    }

    [Theory]
    [ClassData(typeof(ClaimDeclaringElements))]
    public void AClaimTagOnAnElement_WhenRead_ShouldDeclareOneClaimAsAuthoredAtTheElementsKeywordLine(
        string members, string text, string id, int line, GherkinClaimElement element)
    {
        // Given
        var file = (Add, text);

        // When
        var model = Read(members, file);

        // Then
        var claim = model.ClaimSets.SelectMany(static set => set.Rows).Should().ContainSingle().Subject;
        claim.Id.Should().Be(id);
        claim.Line.Should().Be(line);
        claim.IsValid.Should().BeTrue();
        claim.Path.Should().Be(Add);
        claim.Declaration.Should().NotBeNull();
        claim.Declaration!.Element.Should().Be(element);
    }

    [Theory]
    [ClassData(typeof(OrdinaryClaimTags))]
    public void ATagTheClaimTagFormOrTheClaimGrammarRejects_WhenRead_ShouldDeclareNoClaim(string members, string tags, string[] ids)
    {
        // Given
        var file = (Add, $"Feature: Arithmetic\n\n  {tags}\n  Scenario: Adds\n    Given two numbers\n");

        // When
        var model = Read(members, file);

        // Then
        model.ClaimSets.SelectMany(static set => set.Rows).Select(static claim => claim.Id).Should().Equal(ids);
    }

    [Theory]
    [ClassData(typeof(ClaimLifecycleTags))]
    public void ANativeClaim_WhenRead_ShouldTakeItsStatusFromTheLifecycleTagsTheManifestListsOnItsDeclaringElement(
        string members, string text, string state)
    {
        // Given
        var file = (Add, text);

        // When
        var model = Read(members, file);

        // Then
        model.ClaimSets.SelectMany(static set => set.Rows).Should().ContainSingle()
            .Which.Declaration!.State.Should().Be(state);
    }

    [Theory]
    [ClassData(typeof(PlannedClaimCases))]
    public void APlannedTagTheManifestLists_WhenRead_ShouldKeepTheClaimAndMarkTheCasesItCoversPlanned(
        string members, string text, string id, int[] cases, int[] planned)
    {
        // Given
        var file = (Add, text);

        // When
        var model = Read(members, file);

        // Then
        var declaration = model.ClaimSets.SelectMany(static set => set.Rows).Should().ContainSingle(claim => claim.Id == id)
            .Which.Declaration!;
        declaration.Cases.Select(static item => item.Line).Should().Equal(cases);
        declaration.Cases.Where(static item => item.Planned).Select(static item => item.Line).Should().Equal(planned);
    }

    [Fact]
    public void AClaimFileReorderedAndRetitled_WhenReadAgain_ShouldKeepEveryClaimsIdentifierFeatureIdentityAndStatus()
    {
        // Given
        const string Members = """{ "claim": { "withdrawnTags": ["@withdrawn"] } }""";
        var before = Read(
            Members,
            (Add, """
                Feature: Arithmetic

                  @B-001
                  Scenario: Adds
                    Given two numbers

                  @B-002 @withdrawn
                  Scenario: Subtracts
                    Given two numbers
                """));

        // When
        var after = Read(
            Members,
            (Add, """
                Feature: Arithmetic

                  @B-002 @withdrawn
                  Scenario: Takes one number from another
                    Given two numbers

                  @B-001
                  Scenario: Sums two numbers
                    Given two numbers
                """));

        // Then
        static (string Id, string? Epic, string Feature, string State)[] Catalogue(SpechtModel model) =>
            model.ClaimSets
                .SelectMany(static set => set.Rows.Select(claim => (claim.Id, set.FileSet!.Epic, set.FileSet.Feature, claim.Declaration!.State)))
                .ToArray();
        Catalogue(before).Should().HaveCount(2);
        Catalogue(after).Should().BeEquivalentTo(Catalogue(before));
    }

    [Fact]
    public void ClaimFileSetsAndTheirFiles_WhenRead_ShouldOrderClaimsBySetInManifestOrderThenFileInOrdinalPathOrderThenDocumentAndTagOrder()
    {
        // Given
        const string Members = """
            { "claim": { "files": [
              { "glob": ".spec/features/sums/*.feature", "feature": "F2" },
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" } ] } }
            """;

        // When
        var model = Read(
            Members,
            (Subtract, "Feature: Subtract\n\n  @B-004\n  Scenario: Subtracts\n    Given two numbers\n"),
            (Add, """
                Feature: Add

                  @B-002 @B-001
                  @B-003
                  Scenario: Adds
                    Given two numbers

                  @B-005
                  Scenario: Adds three
                    Given three numbers
                """),
            (".spec/features/sums/total.feature", "Feature: Total\n\n  @B-009\n  Scenario: Totals\n    Given numbers\n"));

        // Then
        model.ClaimSets.Select(static set => set.FileSet!.Feature).Should().Equal("F2", "F1");
        model.ClaimSets.Select(static set => set.Rows.Select(static claim => (claim.Path, claim.Id)).ToArray())
            .Should().BeEquivalentTo(
                [
                    new[] { (".spec/features/sums/total.feature", "B-009") },
                    [(Add, "B-002"), (Add, "B-001"), (Add, "B-003"), (Add, "B-005"), (Subtract, "B-004")],
                ],
                static options => options.WithStrictOrdering());
    }

    [Theory]
    [ClassData(typeof(ClaimFileSetRecords))]
    public void AClaimFileSet_WhenRead_ShouldMakeOneNativeFeatureWithItsRecordsLifecycleAndEdgesOrCurrentAndNoEdgeWithoutARecord(
        string members, string? recordText, string lifecycle, string[] dependsOn, string? record)
    {
        // Given
        (string Path, string Text)[] files = recordText is null ? [(Add, AddsTwoNumbers)] : [(Add, AddsTwoNumbers), (Record, recordText)];

        // When
        var model = Read(members, files);

        // Then
        var feature = model.Features.Should().ContainSingle(static feature => feature.ClaimFileSet != null).Subject;
        feature.Record.Should().Be(record);
        feature.Lifecycle.Should().Be(lifecycle);
        feature.Strings(feature.Keys.DependsOn).Should().Equal(dependsOn);
        feature.Strings(feature.Keys.Blocks).Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(NativeFeatureLocations))]
    public void ANativeFeature_WhenRead_ShouldBeLocatedInTheClaimsLayoutAtItsRecordOrElseItsFirstClaimFileInOrdinalOrder(
        string members, string[] claimFiles, string path)
    {
        // Given
        var files = claimFiles.Select(static file => (Path: file, Text: AddsTwoNumbers)).Append((Path: Record, Text: "---\nid: F1\n---\n")).ToArray();

        // When
        var model = Read(members, files);

        // Then
        var feature = model.Features.Should().ContainSingle(static feature => feature.ClaimFileSet != null).Subject;
        feature.RelativePath.Should().Be(path);
        feature.Location.Layout.Should().Be(new SpechtLayout("claims", ".spec/features/arithmetic/*.feature"));
    }

    [Fact]
    public void AClaimFileSetWithNoRecordWhoseGlobMatchesNoFeatureFile_WhenRead_ShouldMakeNoFeatureAndKeepItsEmptyClaimSet()
    {
        // Given
        const string Members = """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" },
              { "glob": ".spec/features/sums/*.feature", "feature": "F2" } ] } }
            """;

        // When
        var model = Read(Members, (".spec/features/sums/total.feature", AddsTwoNumbers));

        // Then
        model.Features.Where(static feature => feature.ClaimFileSet != null).Select(static feature => feature.Id).Should().Equal("F2");
        model.ClaimSets.Select(static set => (set.FileSet!.Feature, set.Rows.Count)).Should().Equal(("F1", 0), ("F2", 1));
    }

    [Theory]
    [ClassData(typeof(RecordIdentities))]
    public void ARecordDeclaringAnIdentityOtherThanItsSets_WhenRead_ShouldLeaveTheNativeFeatureTheSetsIdentity(
        string members, string recordFrontmatter, string? epic, string id)
    {
        // Given
        var record = (Record, $"---\n{recordFrontmatter}\n---\n\n# Arithmetic\n");

        // When
        var model = Read(members, (Add, AddsTwoNumbers), record);

        // Then
        var feature = model.Features.Should().ContainSingle(static feature => feature.ClaimFileSet != null).Subject;
        (feature.Epic, feature.Id).Should().Be((epic, id));
        model.Features.Should().NotContain(static feature => feature.Id == "F9");
    }

    [Fact]
    public void ARecordDeclaringAClaimPrefix_WhenRead_ShouldLeaveTheNativeFeatureTheGrammarOfAFeatureWithNoPrefix()
    {
        // Given
        const string Members = """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] } }
            """;

        // When
        var model = Read(
            Members,
            (Record, "---\nid: F1\nclaim_prefix: ADD-\n---\n\n# Arithmetic\n"),
            (Add, "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n\n  @ADD-002\n  Scenario: Adds three\n    Given three numbers\n"));

        // Then
        var feature = model.Features.Should().ContainSingle(static feature => feature.ClaimFileSet != null).Subject;
        feature.ClaimGrammar.Should().Be(model.Schemas.Structure.ClaimGrammar(null));
        model.Claims.Should().ContainKey(feature).WhoseValue.Rows.Select(static claim => claim.Id).Should().Equal("B-001");
    }

    [Fact]
    public void ARecordALayoutAlsoDiscovers_WhenRead_ShouldBeReadOnceAsTheNativeFeatureAndNotAsTheLayoutsFeature()
    {
        // Given
        const string Members = """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": "src/arithmetic/.spec/README.md" } ] } }
            """;

        // When
        var model = Read(Members, (Add, AddsTwoNumbers), ("src/arithmetic/.spec/README.md", "---\nid: F1\n---\n\n# Arithmetic\n"));

        // Then
        model.Features.Should().ContainSingle(static feature => feature.RelativePath == "src/arithmetic/.spec/README.md")
            .Which.ClaimFileSet.Should().NotBeNull();
    }

    private static SpechtModel Read(string members, params (string Path, string Text)[] files)
    {
        var manifest = JsonNode.Parse(members)!.AsObject();
        manifest["schemaVersion"] = "0.4.0";
        manifest["exclusions"] = new JsonArray("obj");
        if (manifest["claim"] is not JsonObject claim)
        {
            claim = [];
            manifest["claim"] = claim;
        }

        claim["source"] = "gherkin";
        claim["files"] ??= new JsonArray(new JsonObject { ["glob"] = ".spec/features/arithmetic/*.feature", ["feature"] = "F1" });
        var fileSystem = new MockFileSystem(
            files.Append((Path: SpechtTree.ManifestPath, Text: manifest.ToJsonString()))
                .ToDictionary(
                    static file => Path.Combine(["repo", .. file.Path.Split('/')]),
                    static file => new MockFileData(file.Text)));

        return EngineServices.Reader(fileSystem).Read(fileSystem.Path.GetFullPath("repo"), null);
    }

    private const string Add = ".spec/features/arithmetic/add.feature";

    private const string AddsTwoNumbers = "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n";

    private const string Record = ".spec/features/arithmetic/README.md";

    private const string Subtract = ".spec/features/arithmetic/subtract.feature";
}
