using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Evaluation;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Manifest;

/// <summary>
/// Steps for the epics opt-in at schema 0.3.0 (<c>0001-F5</c> B-005, B-006, B-024 to B-035, B-042, B-053, B-054, B-069
/// to B-071; decision 0020; item 0345). "The manifest declares no epic grammar" removes <c>identifiers.epic</c> from the
/// in-memory manifest. A specification "declaring no epic" is written by the tree's writer with its <c>epic</c> line
/// removed, so its <c>id</c> stays on line 2. A specification named only by its id sits in the features layout under
/// <c>src/&lt;id&gt;</c>. An item beside a specification under a manifest declaring no epic grammar names the Feature's
/// <c>id</c> as its parent. A rule id a step names is read at the pin the manifest carries.
/// </summary>
public sealed partial class ManifestSteps
{
    [Given("the manifest declares no epic grammar")]
    public void GivenTheManifestDeclaresNoEpicGrammar() => Named("identifiers").Remove("epic");

    [Given("the manifest and the root's Feature schema both declare the local edge form with a hash before the Feature id")]
    public void GivenTheManifestAndTheFeatureSchemaBothDeclareTheHashLocalEdgeForm()
    {
        Named("forms")["localEdge"] = "#{id}";
        EditSchema(SpechtTree.FeatureSchemaPath, static schema =>
        {
            foreach (var key in new[] { "depends_on", "blocks" })
            {
                schema["properties"]![key]!["items"]!["pattern"] = "^#F[0-9]+[a-z]?$";
            }
        });
    }

    [Then("the edge resolves to Feature {word}")]
    public void ThenTheEdgeResolvesToFeature(string id)
    {
        _rejection.Should().BeNull();
        var model = _model ?? throw new InvalidOperationException("The check loaded no model.");
        model.DependencyGraph.DependsOn.Should().ContainKey("F2").WhoseValue.Should().Equal(id);
        ReportedBy("SPEC050").Should().BeEmpty();
    }

    [Then("both items are read whole")]
    public void ThenBothItemsAreReadWhole()
    {
        _rejection.Should().BeNull();
        var model = _model ?? throw new InvalidOperationException("The check loaded no model.");
        model.Items.Select(static item => item.Id).Should().BeEquivalentTo(_items);
        ReportedBy("SPEC043").Should().BeEmpty();
    }

    [Then("the per-Feature sequence is reported contiguous")]
    public void ThenThePerFeatureSequenceIsReportedContiguous() => ReportedBy("SPEC044").Should().BeEmpty();

    [Given("the root holds a specification declaring epic {string} and id {string}")]
    public void GivenTheRootHoldsASpecificationDeclaringEpicAndId(string epic, string id) =>
        _specifications.Add(Relative(Tree.WriteFeature(epic, id)));

    [Then("the specification is read as {string}")]
    public void ThenTheSpecificationIsReadAs(string identity)
    {
        _rejection.Should().BeNull();
        EngineServices.Reader().Read(Tree.Root, null).Features.Should().ContainSingle().Which.Identity.Should().Be(identity);
    }

    [Given("the root holds two specifications in different folders, both declaring id {string} and no epic")]
    public void GivenTheRootHoldsTwoSpecificationsInDifferentFoldersBothDeclaringIdAndNoEpic(string id)
    {
        WriteEpicLess("src/first", id);
        WriteEpicLess("src/second", id);
    }

    [Then("the duplicate-identity rule reports {string} once on each specification")]
    public void ThenTheDuplicateIdentityRuleReportsOnceOnEachSpecification(string identity) =>
        ReportedBy("SPEC012")
            .Select(static violation => (violation.File, violation.Line, violation.Identifier))
            .Should()
            .BeEquivalentTo(_specifications.Select(path => (path, 2, (string?)identity)));

    [Then("each report names both paths")]
    public void ThenEachReportNamesBothPaths() =>
        ReportedBy("SPEC012").Should().NotBeEmpty().And.AllSatisfy(violation =>
            violation.Message.Should().Contain(_specifications[0]).And.Contain(_specifications[1]));

    [Given("the root holds a specification declaring id {string} and no epic")]
    public void GivenTheRootHoldsASpecificationDeclaringIdAndNoEpic(string id) => WriteEpicLess("src/" + id, id);

    [Given("the root holds a specification declaring id {string} and no epic, in a folder named {string}")]
    public void GivenTheRootHoldsASpecificationDeclaringIdAndNoEpicInAFolderNamed(string id, string folder) =>
        _specifications.Add(Relative(WithoutEpic(Tree.WriteSpecification($"epics/{folder}/{id}-feature/spec.md", "0001", id))));

    [Given("the root holds a specification declaring id {string} and no epic, which depends on {string}")]
    public void GivenTheRootHoldsASpecificationDeclaringIdAndNoEpicWhichDependsOn(string id, string edge) =>
        WriteEpicLess("src/" + id, id, new Dictionary<string, string> { ["depends_on"] = $"[\"{edge}\"]" });

    [Given("no specification declares id {string}")]
    public void GivenNoSpecificationDeclaresId(string id) =>
        _specifications.Should().NotContain(path => File.ReadAllText(Path.Combine(Tree.Root, path)).Contains($"\nid: \"{id}\"\n"));

    [Then("the unresolved-dependency rule reports {string} on that specification")]
    public void ThenTheUnresolvedDependencyRuleReportsOnThatSpecification(string edge) =>
        ReportedBy("SPEC050").Select(static violation => (violation.File, violation.Identifier)).Should().Equal((_specifications[0], edge));

    [Given("the root holds a specification {string} that depends on {string}")]
    public void GivenTheRootHoldsASpecificationThatDependsOn(string id, string edge) =>
        WriteEpicLess("src/" + id, id, new Dictionary<string, string> { ["depends_on"] = $"[\"{edge}\"]" });

    [Given("a specification {string} that lists {string} as blocked by it")]
    public void GivenASpecificationThatListsAsBlockedByIt(string id, string blocked) =>
        WriteEpicLess("src/" + id, id, new Dictionary<string, string> { ["blocks"] = $"[\"{blocked}\"]" });

    [Then("no dependency violation is reported")]
    public void ThenNoDependencyViolationIsReported() =>
        new[] { "SPEC050", "SPEC051", "SPEC052" }.SelectMany(ReportedBy).Should().BeEmpty();

    [Then("the unresolved-dependency rule reports {string} on specification {string}")]
    public void ThenTheUnresolvedDependencyRuleReportsOnSpecification(string edge, string id) =>
        ReportedBy("SPEC050").Select(static violation => (violation.File, violation.Identifier))
            .Should().Equal(($"src/{id}/.spec/README.md", edge));

    [Given("the root holds a specification {string} with an item beside it whose parent is {string}")]
    public void GivenTheRootHoldsASpecificationWithAnItemBesideItWhoseParentIs(string id, string parent) =>
        Tree.WriteItem(WriteEpicLess("src/" + id, id), "0001-01", parent);

    [Then("no item-parent violation is reported")]
    public void ThenNoItemParentViolationIsReported() =>
        ReportedBy("SPEC043").Should().NotContain(static violation => violation.Message.StartsWith("frontmatter parent", StringComparison.Ordinal));

    [Given("the root holds a specification declaring epic {string} and id {string}, with an item beside it whose parent is {string}")]
    public void GivenTheRootHoldsASpecificationDeclaringEpicAndIdWithAnItemBesideItWhoseParentIs(string epic, string id, string parent)
    {
        var path = Tree.WriteFeature(epic, id);
        Tree.WriteItem(path, $"{epic}-01", parent);
        _items = [Relative(Path.Combine(Path.GetDirectoryName(path)!, $"{epic}-01-item.md"))];
    }

    [Then("the item-parent rule reports that item")]
    public void ThenTheItemParentRuleReportsThatItem() =>
        ReportedBy("SPEC043")
            .Where(static violation => violation.Message.StartsWith("frontmatter parent", StringComparison.Ordinal))
            .Select(static violation => violation.File)
            .Should()
            .Equal(_items);

    [Given("the manifest's task grammar carries no epic part")]
    public void GivenTheManifestsTaskGrammarCarriesNoEpicPart() => Named("identifiers")["task"] = "^F[0-9]+-(?<sequence>[0-9]{2})$";

    [Given("the manifest's task grammar names an epic part")]
    public void GivenTheManifestsTaskGrammarNamesAnEpicPart() =>
        Named("identifiers")["task"] = "^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$";

    [Given("the root holds specifications {string} and {string}, each with its own items numbered 01 and 02")]
    public void GivenTheRootHoldsSpecificationsEachWithItsOwnItemsNumbered01And02(string first, string second)
    {
        foreach (var id in new[] { first, second })
        {
            WriteWithItems(id, $"{id}-01", $"{id}-02");
        }
    }

    [Then("no item-sequence violation is reported")]
    public void ThenNoItemSequenceViolationIsReported()
    {
        _rejection.Should().BeNull();
        ReportedBy("SPEC044").Should().BeEmpty();
    }

    [Given("the root holds specification {string} with items {string} and {string}, and specification {string} with item {string}")]
    public void GivenTheRootHoldsSpecificationWithItemsAndSpecificationWithItem(
        string first,
        string firstItem,
        string secondItem,
        string second,
        string thirdItem)
    {
        WriteWithItems(first, firstItem, secondItem);
        WriteWithItems(second, thirdItem);
    }

    [Then(@"^the item-sequence rule reports that Feature (\S+)'s items skip ([0-9]+)$")]
    public void ThenTheItemSequenceRuleReportsThatFeaturesItemsSkip(string id, string missing) =>
        ReportedBy("SPEC044")
            .Select(static violation => (violation.File, violation.Message))
            .Should()
            .Equal(($"src/{id}/.spec/{_items[^1]}-item.md", PerFeatureGap(id, missing)));

    [Given("the root holds an item numbered 02 in a folder that holds no specification")]
    public void GivenTheRootHoldsAnItemNumbered02InAFolderThatHoldsNoSpecification() =>
        Tree.WriteRaw(
            "src/loose/.spec/0001-02-item.md",
            "---\nid: \"0001-02\"\nparent: \"F1\"\ntype: task\nstatus: ready\npriority: med\ncreated: \"2026-10-07\"\n"
                + "updated: \"2026-10-07\"\ngithub_issue: null\n---\n# Item\n");

    [Given("the root holds specification {string} with items {string} and {string}")]
    public void GivenTheRootHoldsSpecificationWithItems(string id, string first, string second) => WriteWithItems(id, first, second);

    [Then(@"^the sequence-gap message says Feature ""(.*)""'s item sequence skips ([0-9]+), and that tasks are numbered per Feature$")]
    public void ThenTheSequenceGapMessageSaysTheFeaturesItemSequenceSkips(string id, string missing) =>
        ReportedBy("SPEC044").Select(static violation => violation.Message).Should().Equal(PerFeatureGap(id, missing));

    [Given("the root holds no epic frontmatter schema")]
    public void GivenTheRootHoldsNoEpicFrontmatterSchema() => File.Delete(Path.Combine(Tree.Root, SpechtTree.EpicSchemaPath));

    [Then("the root is checked")]
    public void ThenTheRootIsChecked()
    {
        _rejection.Should().BeNull();
        _report.Should().NotBeNull();
    }

    [Then("nothing is reported about a missing schema")]
    public void ThenNothingIsReportedAboutAMissingSchema() =>
        _report!.Violations.Should().NotContain(static violation =>
            violation.Message.Contains(Path.GetFileName(SpechtTree.EpicSchemaPath), StringComparison.Ordinal));

    [Given("the root holds an epic file whose frontmatter would fail the epic schema")]
    public void GivenTheRootHoldsAnEpicFileWhoseFrontmatterWouldFailTheEpicSchema() =>
        Tree.WriteEpic("0001", new Dictionary<string, string> { ["type"] = "feature" });

    [Then("the epic-frontmatter rule reports nothing")]
    public void ThenTheEpicFrontmatterRuleReportsNothing()
    {
        _rejection.Should().BeNull();
        ReportedBy("SPEC004").Should().BeEmpty();
    }

    [Given("the root holds an epic file with no frontmatter")]
    public void GivenTheRootHoldsAnEpicFileWithNoFrontmatter() => Tree.WriteRaw(EpicFile, "# Epic\n");

    [Then("the frontmatter-present rule reports nothing on the epic file")]
    public void ThenTheFrontmatterPresentRuleReportsNothingOnTheEpicFile()
    {
        _rejection.Should().BeNull();
        ReportedBy("SPEC001").Should().NotContain(static violation => violation.File == EpicFile);
    }

    [When("the frontmatter command checks that file as an epic")]
    public void WhenTheFrontmatterCommandChecksThatFileAsAnEpic()
    {
        File.WriteAllText(ManifestPath, Manifest.ToJsonString());
        _run = AcceptanceTool.Launch(Tree.Root, "frontmatter", "--kind", "epic", EpicFile, "--root", ".");
    }

    [Then("the epic-frontmatter rule reports it")]
    public void ThenTheEpicFrontmatterRuleReportsIt()
    {
        Run.ExitCode.Should().Be(1, Run.Stdout + Run.Stderr);
        Run.Stdout.Should().Contain($"{EpicFile}(").And.Contain($" {RuleIds.Under(Manifest, "SPEC004")}: ");
    }

    [Given("the root holds a specification in the {word} layout declaring id {string} and no epic")]
    public void GivenTheRootHoldsASpecificationInTheLayoutDeclaringIdAndNoEpic(string layout, string id) =>
        _specifications.Add(Relative(WithoutEpic(layout == "epics" ? Tree.WriteFeature("0001", id) : Tree.WriteCoLocatedFeature("src/area", "0001", id))));

    [Then("the path-identity rule reports that specification at the line of its id, under identity {string}")]
    public void ThenThePathIdentityRuleReportsThatSpecificationAtTheLineOfItsIdUnderIdentity(string identity) =>
        ReportedBy("SPEC011")
            .Select(static violation => (violation.File, violation.Line, violation.Identifier))
            .Should()
            .Equal((_specifications[0], 2, identity));

    [Then("the message says the frontmatter has no epic while the manifest declares the epic grammar")]
    public void ThenTheMessageSaysTheFrontmatterHasNoEpicWhileTheManifestDeclaresTheEpicGrammar() =>
        ReportedBy("SPEC011").Select(static violation => violation.Message).Should()
            .Equal("frontmatter has no epic - the manifest declares the epic grammar");

    [Given("the manifest's epics layout declares which path segments carry the epic and the Feature id")]
    public void GivenTheManifestsEpicsLayoutDeclaresWhichPathSegmentsCarryTheEpicAndTheFeatureId() =>
        EpicsLayout()["identity"] = new JsonObject { ["epic"] = 1, ["feature"] = 2 };

    [Given("the root holds a specification in that layout whose Feature folder matches its id and whose epic folder is named nothing like its frontmatter")]
    public void GivenTheRootHoldsASpecificationInThatLayoutWhoseEpicFolderIsNamedNothingLikeItsFrontmatter() =>
        _specifications.Add(Relative(Tree.WriteSpecification("epics/misc-group/F1-feature/spec.md", "0001", "F1")));

    [Given("the root holds a second specification in that layout whose Feature folder does not match its id")]
    public void GivenTheRootHoldsASecondSpecificationInThatLayoutWhoseFeatureFolderDoesNotMatchItsId() =>
        _specifications.Add(Relative(Tree.WriteSpecification("epics/0001-epic/F9-feature/spec.md", "0001", "F2")));

    [Then("the path-identity rule reports the second specification for its Feature folder")]
    public void ThenThePathIdentityRuleReportsTheSecondSpecificationForItsFeatureFolder() =>
        ReportedBy("SPEC011").Should().Contain(violation =>
            violation.File == _specifications[1] && violation.Line == 2 && violation.Message.Contains("'F9-feature'", StringComparison.Ordinal));

    [Then("the path-identity rule reports nothing else")]
    public void ThenThePathIdentityRuleReportsNothingElse() => ReportedBy("SPEC011").Should().ContainSingle();

    private static string WithoutEpic(string path)
    {
        var text = File.ReadAllText(path);
        text.Should().Contain("\nepic: \"0001\"\n");
        File.WriteAllText(path, text.Replace("\nepic: \"0001\"\n", "\n", StringComparison.Ordinal));

        return path;
    }

    private static string PerFeatureGap(string id, string missing) =>
        $"Feature {id}'s item sequence skips {missing} - tasks are numbered per Feature, contiguously from 01";

    private IEnumerable<SpechtViolation> ReportedBy(string key)
    {
        _rejection.Should().BeNull();

        return (_report ?? throw new InvalidOperationException("The check has not run.")).Violations
            .Where(violation => violation.RuleId == RuleIds.Under(Manifest, key));
    }

    private string WriteEpicLess(string area, string id, IReadOnlyDictionary<string, string>? frontmatter = null)
    {
        var path = WithoutEpic(Tree.WriteCoLocatedFeature(area, "0001", id, frontmatter));
        _specifications.Add(Relative(path));

        return path;
    }

    private void WriteWithItems(string id, params string[] items)
    {
        var path = WriteEpicLess("src/" + id, id);
        foreach (var item in items)
        {
            Tree.WriteItem(path, item, id);
        }

        _items = items;
    }

    private const string EpicFile = "epics/0001-epic/epic.md";
}
