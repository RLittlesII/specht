using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Acceptance.Manifest;

/// <summary>
/// Steps for the frontmatter key roles, the identity and edge forms and the grammars of schema 0.3.0 (<c>0001-F5</c>
/// B-004 to B-006, B-014, B-019, B-049 to B-054, B-068; decisions 0010, 0016, 0017 and 0018). A key role is written under
/// <c>keys</c>, grouped by kind, and a form under <c>forms</c>; where a step says the root's schema carries the same
/// change, the on-disk frontmatter schema in the tree is edited to match, its property renamed in <c>properties</c> and
/// <c>required</c>, or its pattern widened. A renamed frontmatter key is written by renaming the line the tree's writer
/// gives the default key. A rule id a step names is read at the pin the manifest carries. An edge written with no colon
/// is in the local form, and the Feature it names blocks <c>F2</c> in that form too; items beside a specification name
/// <c>F1</c> as their parent under a manifest declaring no epic grammar (<c>0001-F5</c> B-005, B-006, B-030; item 0345).
/// </summary>
public sealed partial class ManifestSteps
{
    [Given("the manifest's Feature key roles and the root's Feature schema both name the {word} key {string}")]
    public void GivenTheFeatureKeyRoleAndTheFeatureSchemaBothNameTheKey(string role, string name)
    {
        KeysOf("feature")[role] = name;
        EditSchema(SpechtTree.FeatureSchemaPath, schema => RenameProperty(schema, role, name));
    }

    [Given("the root holds a specification whose {string} names an item that does not exist")]
    public void GivenTheRootHoldsASpecificationWhoseKeyNamesAnItemThatDoesNotExist(string key)
    {
        var path = Tree.WriteFeature("0001", "F1");
        RenameLine(path, "children: []", $"{key}: [\"{AbsentItem}\"]");
        _specifications.Add(Relative(path));
        _renamedKey = key;
    }

    [Given("the root holds a specification whose {string} is done with a {string} cell in its traceability table")]
    public void GivenTheRootHoldsASpecificationWhoseStatusKeyIsDoneWithACellInItsTraceabilityTable(string key, string cell)
    {
        var path = Tree.WriteFeature(
            "0001",
            "F1",
            new Dictionary<string, string> { ["status"] = "done" },
            SpechtTree.SectionsWith(DefaultTitles["matrix"], Matrix(DefaultTitles["matrix"], "Scenario | Test", $"It does the thing | {cell}")));
        RenameLine(path, "status: done", $"{key}: done");
        _specifications.Add(Relative(path));
    }

    [Then("the unresolved-child rule reports it")]
    public void ThenTheUnresolvedChildRuleReportsIt()
    {
        _rejection.Should().BeNull();
        var key = _renamedKey ?? throw new InvalidOperationException("No key was renamed.");
        var lines = File.ReadAllLines(Path.Combine(Tree.Root, _specifications[0]));
        var line = Array.FindIndex(lines, text => text.StartsWith($"{key}:", StringComparison.Ordinal)) + 1;
        _report!.Violations
            .Where(violation => violation.RuleId == RuleIds.Under(Manifest, "SPEC040"))
            .Select(static violation => (violation.File, violation.Line, violation.Identifier))
            .Should()
            .Equal((_specifications[0], line, AbsentItem));
    }

    [Given("the manifest and the root's Feature schema both declare the qualified edge form with a colon between the epic and the Feature id")]
    public void GivenTheManifestAndTheFeatureSchemaBothDeclareTheColonQualifiedEdgeForm()
    {
        Named("forms")["qualifiedEdge"] = "{epic}:{id}";
        EditSchema(SpechtTree.FeatureSchemaPath, static schema =>
        {
            foreach (var key in new[] { "depends_on", "blocks" })
            {
                schema["properties"]![key]!["items"]!["pattern"] = "^([0-9]{4}:)?F[0-9]+[a-z]?$";
            }
        });
    }

    [Given("the root holds a specification whose dependency edge is written {string}")]
    public void GivenTheRootHoldsASpecificationWhoseDependencyEdgeIsWritten(string edge)
    {
        var parts = edge.Split(':');
        var (epic, id, blocks) = parts.Length == 2 ? (parts[0], parts[1], "0001:F2") : ("0001", edge.TrimStart('#'), "#F2");
        _specifications.Add(Relative(Tree.WriteFeature("0001", "F2", new Dictionary<string, string> { ["depends_on"] = $"[\"{edge}\"]" })));
        _specifications.Add(Relative(Tree.WriteFeature(epic, id, new Dictionary<string, string> { ["blocks"] = $"[\"{blocks}\"]" })));
    }

    [Then("the edge resolves to Feature {word} of epic {word}")]
    public void ThenTheEdgeResolvesToFeatureOfEpic(string id, string epic)
    {
        _rejection.Should().BeNull();
        var model = _model ?? throw new InvalidOperationException("The check loaded no model.");
        model.DependencyGraph.DependsOn["0001-F2"].Should().Equal($"{epic}-{id}");
        _report!.Violations.Should().NotContain(violation => violation.RuleId == RuleIds.Under(Manifest, "SPEC050"));
    }

    [Then("no frontmatter violation is reported")]
    public void ThenNoFrontmatterViolationIsReported()
    {
        _rejection.Should().BeNull();
        _frontmatterViolations.Should().BeEmpty();
    }

    [Given("the manifest's task grammar and the root's frontmatter schemas all carry a three-digit sequence")]
    [Given("the task grammar and on-disk schemas carry a three-digit sequence")]
    public void GivenTheTaskGrammarAndTheSchemasCarryAThreeDigitSequence()
    {
        Named("identifiers")["task"] = "^(?<epic>[0-9]{4})-(?<sequence>[0-9]{3})$";
        EditSchema(SpechtTree.ItemSchemaPath, static schema => schema["properties"]!["id"]!["pattern"] = ThreeDigitTask);
        EditSchema(SpechtTree.FeatureSchemaPath, static schema =>
        {
            foreach (var key in new[] { "children", "spikes" })
            {
                schema["properties"]![key]!["items"]!["pattern"] = ThreeDigitTask;
            }
        });
    }

    [Given("the root holds items {string} and {string} beside a specification")]
    public void GivenTheRootHoldsItemsBesideASpecification(string first, string second)
    {
        var path = Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["children"] = $"[\"{first}\", \"{second}\"]" });
        _specifications.Add(Relative(path));
        var parent = Named("identifiers")["epic"] is null ? "F1" : "0001-F1";
        Tree.WriteItem(path, first, parent);
        Tree.WriteItem(path, second, parent);
        _items = [first, second];
    }

    [Then("both items are read with epic {string}")]
    public void ThenBothItemsAreReadWithEpic(string epic)
    {
        _rejection.Should().BeNull();
        var model = _model ?? throw new InvalidOperationException("The check loaded no model.");
        var grammar = new Regex(model.Schemas.Structure.Identifiers["task"]);
        model.Items.Select(static item => item.Id).Should().BeEquivalentTo(_items);
        model.Items.Select(item => grammar.Match(item.Id!).Groups["epic"].Value).Should().AllBe(epic);
        _report!.Violations.Should().NotContain(violation => violation.RuleId == RuleIds.Under(Manifest, "SPEC043"));
    }

    [Then("the per-epic sequence is reported contiguous")]
    public void ThenThePerEpicSequenceIsReportedContiguous() =>
        _report!.Violations.Should().NotContain(violation => violation.RuleId == RuleIds.Under(Manifest, "SPEC044"));

    [Then("the sequence-gap message names the missing number {string}")]
    public void ThenTheSequenceGapMessageNamesTheMissingNumber(string missing)
    {
        _rejection.Should().BeNull();
        _report!.Violations
            .Where(violation => violation.RuleId == RuleIds.Under(Manifest, "SPEC044"))
            .Should()
            .ContainSingle()
            .Which.Message.Should()
            .MatchRegex($"(?<![0-9]){missing}(?![0-9])");
    }

    [Given("the manifest declares the epic grammar")]
    public void GivenTheManifestDeclaresTheEpicGrammar() => Named("identifiers")["epic"] = "^[0-9]{4}$";

    [Given("the manifest's {word} grammar is not a valid expression")]
    public void GivenTheManifestsGrammarIsNotAValidExpression(string grammar) => Named("identifiers")[GrammarKey(grammar)] = "[";

    [Then("the rejection names the {word} grammar")]
    public void ThenTheRejectionNamesTheGrammar(string grammar) =>
        (_rejection ?? throw new InvalidOperationException("The manifest was accepted.")).Message.Should().MatchRegex($@"\b{GrammarKey(grammar)}\b");

    [Then("only the rejection is written to stderr")]
    public void ThenOnlyTheRejectionIsWrittenToStderr()
    {
        Run.Stdout.Should().BeEmpty();
        Run.Stderr.Split('\n', StringSplitOptions.RemoveEmptyEntries).Should().ContainSingle().Which.Should().StartWith("specht: .specht/manifest.json:");
    }

    [Given("the manifest and on-disk schemas name the Feature id key {string} and the item id key {string}")]
    public void GivenTheManifestAndOnDiskSchemasNameTheFeatureIdKeyAndTheItemIdKey(string feature, string item)
    {
        KeysOf("feature")["id"] = feature;
        KeysOf("task")["id"] = item;
        EditSchema(SpechtTree.FeatureSchemaPath, schema => RenameProperty(schema, "id", feature));
        EditSchema(SpechtTree.ItemSchemaPath, schema => RenameProperty(schema, "id", item));
        _identityKeys = (feature, item);
        _fromRoot = true;
    }

    [Given("the root holds a Feature and an item using those respective keys")]
    public void GivenTheRootHoldsAFeatureAndAnItemUsingThoseRespectiveKeys()
    {
        var (feature, item) = _identityKeys ?? throw new InvalidOperationException("No id keys were named.");
        var path = Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["children"] = "[\"0001-01\"]" });
        RenameLine(path, "id: \"F1\"", $"{feature}: \"F1\"");
        Tree.WriteItem(path, "0001-01", "0001-F1");
        RenameLine(Path.Combine(Path.GetDirectoryName(path)!, "0001-01-item.md"), "id: \"0001-01\"", $"{item}: \"0001-01\"");
        _specifications.Add(Relative(path));
    }

    [Then("the Feature and item identities are read from their own kind's key")]
    public void ThenTheFeatureAndItemIdentitiesAreReadFromTheirOwnKindsKey()
    {
        _rejection.Should().BeNull();
        var model = _model ?? throw new InvalidOperationException("The check loaded no model.");
        model.Features.Select(static feature => feature.Identity).Should().Equal("0001-F1");
        model.Items.Select(static item => item.Id).Should().Equal("0001-01");
    }

    [Then("no identity mismatch is reported")]
    public void ThenNoIdentityMismatchIsReported()
    {
        string[] identity = [.. new[] { "SPEC011", "SPEC012", "SPEC043" }.Select(id => RuleIds.Under(Manifest, id))];
        _report!.Violations.Should().NotContain(violation => identity.Contains(violation.RuleId));
    }

    [Given("the tool ships schema version {string} alongside {string}")]
    public void GivenTheToolShipsSchemaVersionAlongside(string newer, string older)
    {
        var versions = EngineServices.Versions();
        foreach (var number in new[] { newer, older })
        {
            SemanticVersion.TryParse(number, out var version).Should().BeTrue();
            versions.Find(version).Should().NotBeNull(number);
        }
    }

    [Given("the root exercises a group-less task grammar, a malformed edge and a custom sequence width")]
    public void GivenTheRootExercisesAGrouplessTaskGrammarAMalformedEdgeAndACustomSequenceWidth()
    {
        Named("identifiers")["task"] = "^[0-9]{4}-[0-9]{3}$";
        Named("identifiers")["feature"] = "^F[0-9]+$";
        var path = Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["depends_on"] = "[\"F2x\"]" });
        _specifications.Add(Relative(path));
        Tree.WriteItem(path, "0001-001", "0001-F1");
        Tree.WriteItem(path, "0001-003", "0001-F1");
    }

    [Then("the verdict and manifest rejection behavior equal the recorded behavior for {string}")]
    public void ThenTheVerdictAndManifestRejectionBehaviorEqualTheRecordedBehaviorFor(string pin)
    {
        _rejection.Should().BeNull(pin);
        var directory = _specifications[0][..(_specifications[0].LastIndexOf('/') + 1)];
        _report!.Violations
            .Where(static violation => violation.RuleId is "SPEC043" or "SPEC044" or "SPEC050")
            .Select(static violation => (violation.RuleId, violation.File, violation.Identifier))
            .Should()
            .Equal(
                ("SPEC043", directory + "0001-001-item.md", "0001-001"),
                ("SPEC043", directory + "0001-003-item.md", "0001-003"),
                ("SPEC050", _specifications[0], "0001-F2x"));
    }

    [Given("its task grammar is a valid expression with no named sequence group")]
    public void GivenItsTaskGrammarIsAValidExpressionWithNoNamedSequenceGroup() =>
        Named("identifiers")["task"] = "^(?<epic>[0-9]{4})-[0-9]{2}$";

    [Given("its task grammar is {string}, the grammar the default manifests of earlier versions declare")]
    public void GivenItsTaskGrammarIsTheGrammarTheDefaultManifestsOfEarlierVersionsDeclare(string grammar)
    {
        foreach (var version in EngineServices.Versions().Versions.Where(static version => version.Number.CompareTo(new SemanticVersion(0, 3, 0)) < 0))
        {
            JsonNode.Parse(version.Manifest)!["identifiers"]!["task"]!.GetValue<string>().Should().Be(grammar);
        }

        Named("identifiers")["task"] = grammar;
    }

    [Given("its task grammar names the sequence group but not the epic group")]
    public void GivenItsTaskGrammarNamesTheSequenceGroupButNotTheEpicGroup() =>
        Named("identifiers")["task"] = "^[0-9]{4}-(?<sequence>[0-9]{2})$";

    [Then("the manifest rejection names the task grammar and the missing {word} group")]
    public void ThenTheManifestRejectionNamesTheTaskGrammarAndTheMissingGroup(string group) =>
        (_rejection ?? throw new InvalidOperationException("The manifest was accepted.")).Message.Should()
            .MatchRegex(@"\btask\b").And.MatchRegex($@"\b{group}\b");

    [Given("its {string} form is {string}")]
    public void GivenItsFormIs(string form, string value) => Named("forms")[form] = value;

    [Then("the manifest rejection names {string} and {string}")]
    public void ThenTheManifestRejectionNamesAnd(string form, string value) =>
        (_rejection ?? throw new InvalidOperationException("The manifest was accepted.")).Message.Should().Contain(form).And.Contain(value);

    [Given("the manifest omits the qualified-edge form")]
    public void GivenTheManifestOmitsTheQualifiedEdgeForm()
    {
        Manifest["forms"] = new JsonObject { ["identity"] = "{epic}-{id}", ["localEdge"] = "{id}" };
    }

    [Then("its qualified-edge form is {string}")]
    public void ThenItsQualifiedEdgeFormIs(string form) =>
        (EngineServices.Parser().ParseDocument(Manifest.ToJsonString()).Document["forms"]?["qualifiedEdge"]
            ?? throw new InvalidOperationException("The effective manifest holds no qualified-edge form.")).GetValue<string>().Should().Be(form);

    [Given("the root holds a specification whose dependency edge is {string}")]
    public void GivenTheRootHoldsASpecificationWhoseDependencyEdgeIs(string edge)
    {
        _specifications.Add(Relative(Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["depends_on"] = $"[\"{edge}\"]" })));
        _edge = edge;
    }

    [Given("that edge matches neither configured edge form")]
    public void GivenThatEdgeMatchesNeitherConfiguredEdgeForm()
    {
        Named("identifiers")["feature"] = "^F[0-9]+$";
        var edge = _edge ?? throw new InvalidOperationException("No edge was written.");
        Regex.IsMatch(edge, "^F[0-9]+$").Should().BeFalse();
        edge.Should().NotContain("/");
    }

    [Then("the unresolved-edge rule reports identifier {string}")]
    public void ThenTheUnresolvedEdgeRuleReportsIdentifier(string identifier)
    {
        _rejection.Should().BeNull();
        _report!.Violations
            .Where(violation => violation.RuleId == RuleIds.Under(Manifest, "SPEC050"))
            .Select(static violation => (violation.File, violation.Identifier))
            .Should()
            .Equal((_specifications[0], identifier));
    }

    [Given("the manifest declares the epic grammar and no task grammar")]
    public void GivenTheManifestDeclaresTheEpicGrammarAndNoTaskGrammar()
    {
        var identifiers = Named("identifiers");
        identifiers["epic"] = "^[0-9]{4}$";
        identifiers.Remove("task");
    }

    [Then("the manifest is not rejected")]
    public void ThenTheManifestIsNotRejected()
    {
        _rejection.Should().BeNull();
        _report.Should().NotBeNull();
    }

    [Then("its task grammar names the epic and sequence groups")]
    public void ThenItsTaskGrammarNamesTheEpicAndSequenceGroups() =>
        new Regex(EngineServices.Parser().Parse(Manifest.ToJsonString()).Identifiers["task"]).GetGroupNames()
            .Should().Contain(["epic", "sequence"]);

    private static void RenameProperty(JsonObject schema, string from, string to)
    {
        var properties = schema["properties"]!.AsObject();
        var property = properties[from]!.DeepClone();
        properties.Remove(from);
        properties[to] = property;
        var required = schema["required"]!.AsArray();
        var names = required.Select(static name => name!.GetValue<string>()).ToList();
        required[names.IndexOf(from)] = to;
    }

    private static void RenameLine(string path, string line, string replacement)
    {
        var text = File.ReadAllText(path);
        text.Should().Contain($"\n{line}\n");
        File.WriteAllText(path, text.Replace($"\n{line}\n", $"\n{replacement}\n", StringComparison.Ordinal));
    }

    private static string GrammarKey(string grammar) => grammar == "open-question" ? "openQuestion" : grammar;

    private JsonObject KeysOf(string kind)
    {
        var keys = Named("keys");
        if (keys[kind] is not JsonObject roles)
        {
            roles = [];
            keys[kind] = roles;
        }

        return roles;
    }

    private void EditSchema(string relativePath, Action<JsonObject> edit)
    {
        var path = Path.Combine(Tree.Root, relativePath);
        var schema = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        edit(schema);
        File.WriteAllText(path, schema.ToJsonString());
    }

    private const string AbsentItem = "0001-09";

    private const string ThreeDigitTask = "^[0-9]{4}-[0-9]{3}$";

    private string? _renamedKey;
    private string? _edge;
    private IReadOnlyList<string> _items = [];
    private (string Feature, string Item)? _identityKeys;
}
