using System.IO.Abstractions;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules.Matrix;
using Specht.Tests.Shared;

namespace Specht.Acceptance.NativeClaims;

/// <summary>
/// Steps for <c>src/specht/Rules/NativeClaims/.spec/native-claims.feature</c> (0001-F16), bound for the claim members of
/// item 0389, B-002, B-027, B-028, B-029 and B-031, and for the native claim reader of item 0390, B-001, B-004 to B-007,
/// B-011 to B-013, B-025 and B-030, and for a claim file set's record, identity and report of item 0391, B-003, B-017,
/// B-026, B-032 and B-036 to B-038, and for a scenario in a claim file with no claim of item 0393, B-010 and the
/// boundary that no claim is inferred from a title, and for a claim tag on an Examples block of item 0394, B-009, and
/// for an unreadable claim file, set or record of item 0395, B-014 to B-016, B-018, B-019, with its boundary that a
/// Feature-level tag that agrees, or none, is not reported, and B-033 to B-035, and for the existing rules on a native
/// Feature of item 0392, B-008, B-020, with its boundary that a Feature with no record is held to no section, and B-021,
/// and for a claim file, a record's matrix and the trace of item 0396, B-022 to B-024. The B-022 scenario's claim file stays an orphan companion
/// until the build excludes it: "companionFiles" lists "*.feature" and no layout's glob matches a file. "specht trace" runs the built tool's trace
/// command on the root.
/// "The output has the line" binds a stdout line equal to the text given. A record that holds or lacks a section is the
/// canonical sections with that one left out. "The output
/// has a line" binds any line that starts with the text given, a line-<c>0</c> line printing its path with no position.
/// A file that cannot be read is held open with no sharing until the scenario ends. A record at an absolute path is the
/// root's own record file named by its full path, so a check that read it would find it. The other scenarios bind only
/// the Background and are left out as pending. A root is a
/// synthetic tree holding one co-located Markdown specification, <c>F5</c>, so a claim read from a claims table is
/// observable and no identity a scenario names collides with it. The manifest is edited in memory and written when the
/// check runs; "the check runs" launches the built tool on the root, the only place an exit code and its streams are
/// seen. A record is written with a full Feature frontmatter, <c>id</c> <c>F1</c>, and the keys a scenario sets over it.
/// The catalogue, a native Feature's lifecycle, edges and identity are printed by no command, so a step that reads them
/// reads the model the engine builds of the same root. "The output of the same tree before this Feature" is written here, not produced by the
/// tool under test: this tree's clean output under schema <c>0.3.1</c>, exit <c>0</c>, an empty stderr and the four summary
/// lines that <c>specht.tool</c> <c>0.5.35-preview</c>, which predates this Feature, prints for it (B-028).
/// </summary>
[Binding]
[Scope(Feature = "Native Gherkin claims")]
public sealed class NativeClaimsSteps
{
    [Given("the manifest's {string} lists {string}")]
    public void GivenTheManifestsLists(string path, string entry)
    {
        var list = Node(path) as JsonArray;
        if (list is null)
        {
            list = [];
            Set(path, list);
        }

        list.Add(JsonValue.Create(entry));
    }

    [Given("a layout's glob {string} matches no file")]
    public void GivenALayoutsGlobMatchesNoFile(string glob) =>
        ((JsonArray) Manifest["layouts"]!).Add(new JsonObject { ["name"] = "beside", ["glob"] = glob });

    [Given("the glob matches {string} with scenarios tagged {string} and {string}")]
    public void GivenTheGlobMatchesWithScenariosTaggedAnd(string path, string first, string second) =>
        GivenDeclaresScenariosTaggedAnd(path, first, second);

    [Given("the record's {string} has one row per claim in the claim files")]
    public void GivenTheRecordsHasOneRowPerClaimInTheClaimFiles(string title)
    {
        Tree.WriteRaw(AddFile, Adds);
        Set("matrix.scenarioTitles", JsonValue.Create(true));
        WriteMatrix(title, "| B-001 | Adds | Missing | Missing |\n");
    }

    [Given("the claim files declare {string} and {string} and the record's matrix lists only {string}")]
    public void GivenTheClaimFilesDeclareAndAndTheRecordsMatrixListsOnly(string first, string second, string listed)
    {
        GivenDeclaresScenariosTaggedAnd(AddFile, $"@{first}", $"@{second}");
        WriteMatrix("9. Traceability Matrix", $"| {listed} | Adds | Missing | Missing |\n");
    }

    [When("\"specht trace\" runs")]
    public void WhenSpechtTraceRuns()
    {
        File.WriteAllText(ManifestPath, Manifest.ToJsonString());
        _run = AcceptanceTool.Launch(Tree.Root, ["trace", "--root", "."]);
    }

    [Then("each row's scenario resolves against the set's claim files")]
    public void ThenEachRowsScenarioResolvesAgainstTheSetsClaimFiles()
    {
        var model = Model();
        var rows = new MatrixResolver().Resolve(NativeFeature("F1"), model.Schemas.Structure, []);
        rows.Should().NotBeEmpty().And.OnlyContain(static row =>
            row.Scenarios.Count > 0 && row.Scenarios.All(static title => title.Status == MatrixScenarioStatus.Resolved));
    }

    [Then("the output has a line for the record naming {string} with the identifier {string}")]
    public void ThenTheOutputHasALineForTheRecordNamingWithTheIdentifier(string ruleId, string identifier) =>
        Diagnostics().Should().Contain(line => line.StartsWith(Record, StringComparison.Ordinal)
            && line.Contains($" {ruleId}:", StringComparison.Ordinal)
            && line.EndsWith($"[{identifier}]", StringComparison.Ordinal));

    [Then("no {string} is reported for {string}")]
    public void ThenNoIsReportedForPath(string ruleId, string path)
    {
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        Diagnostics().Should().NotContain(line => line.StartsWith(path, StringComparison.Ordinal) && line.Contains($" {ruleId}:", StringComparison.Ordinal));
    }

    [Then("no {string} and no rule of Feature {string} is reported for Feature {string}")]
    public void ThenNoAndNoRuleOfFeatureIsReportedForFeature(string ruleId, string ruleFeature, string feature)
    {
        ruleFeature.Should().Be("0001-F12");
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        ClaimSet(feature).Rows.Should().NotBeEmpty();
        string[] ids = [ruleId, "SPEC4005", "SPEC4006", "SPEC4007", "SPEC4008", "SPEC4009", "SPEC4010"];
        Diagnostics().Should().NotContain(line => ids.Any(id => line.Contains($" {id}:", StringComparison.Ordinal)));
    }

    [Then("each claim of Feature {string} has the status {string}")]
    public void ThenEachClaimOfFeatureHasTheStatus(string feature, string status)
    {
        var ids = ClaimSet(feature).Rows.Select(static claim => claim.Id).ToArray();
        ids.Should().NotBeEmpty();
        Run.ExitCode.Should().Be(0, Run.Stderr);
        var lines = Run.Stdout.Split('\n').Select(static line => line.TrimEnd('\r').Split('\t')).Where(static fields => fields.Length == 5).ToArray();
        lines.Select(static fields => (fields[1], fields[2])).Should().BeEquivalentTo(ids.Select(id => (id, status)));
    }

    [Given("a root with a manifest pinned to schema {string}")]
    public void GivenARootWithAManifestPinnedToSchema(string pin)
    {
        _tree = new SpechtTree();
        _tree.WriteCoLocatedFeature("src/arithmetic", "0001", "F5");
        Manifest = JsonNode.Parse(File.ReadAllText(ManifestPath))!.AsObject();
        Manifest["schemaVersion"] = pin;
    }

    [Given("the manifest's {string} does not hold {string}")]
    public void GivenTheManifestsDoesNotHold(string path, string entry) =>
        Set(path, new JsonArray(((JsonArray) Node(path)!)
            .Select(static node => node!.GetValue<string>())
            .Where(value => value != entry)
            .Select(static value => (JsonNode?) JsonValue.Create(value))
            .ToArray()));

    [Given("the manifest sets {string} to {string}")]
    public void GivenTheManifestSetsTo(string path, string value) => Set(path, JsonValue.Create(value));

    [Given("the manifest sets {string} to the number {int}")]
    public void GivenTheManifestSetsToTheNumber(string path, int value) => Set(path, JsonValue.Create(value));

    [Given("{string} holds one claim file set with glob {string} and feature {string}")]
    public void GivenHoldsOneClaimFileSetWithGlobAndFeature(string path, string glob, string feature) =>
        Set(path, new JsonArray(new JsonObject { ["glob"] = glob, ["feature"] = feature }));

    [Given("the manifest leaves {string} and {string} at their defaults")]
    public void GivenTheManifestLeavesAndAtTheirDefaults(string first, string second)
    {
        Remove(first);
        Remove(second);
    }

    [Given("the manifest has no {string}")]
    public void GivenTheManifestHasNo(string path) => Remove(path);

    [Given("the manifest declares {string}")]
    public void GivenTheManifestDeclares(string path) => Set(path, JsonValue.Create("^[0-9]{4}$"));

    [Given("the manifest declares no epic grammar")]
    public void GivenTheManifestDeclaresNoEpicGrammar() => Remove("identifiers.epic");

    [Given("the claim file set has epic {string} and feature {string}")]
    public void GivenTheClaimFileSetHasEpicAndFeature(string epic, string feature)
    {
        ClaimFileSet["epic"] = epic;
        ClaimFileSet["feature"] = feature;
    }

    [Given("the claim file set has epic {string}")]
    public void GivenTheClaimFileSetHasEpic(string epic) => ClaimFileSet["epic"] = epic;

    [Given("the claim file set has no epic")]
    public void GivenTheClaimFileSetHasNoEpic() => ClaimFileSet.Remove("epic");

    [Given("the manifest is pinned to schema {string}")]
    public void GivenTheManifestIsPinnedToSchema(string pin) => Manifest["schemaVersion"] = pin;

    [Given("{string} has no member beyond {string} and {string}")]
    public void GivenHasNoMemberBeyondAnd(string path, string first, string second)
    {
        var claim = (JsonObject) Node(path)!;
        foreach (var member in claim.Select(static entry => entry.Key).Where(member => member != first && member != second).ToList())
        {
            claim.Remove(member);
        }
    }

    [Given("a tree the check passes without {string}")]
    public void GivenATreeTheCheckPassesWithout(string path)
    {
        Remove(path);
        _baseline = Launch();
        _baseline.Value.ExitCode.Should().Be(0, _baseline.Value.Stderr);
    }

    [Given("the manifest's {string} is absent")]
    public void GivenTheManifestsIsAbsent(string path) => Remove(path);

    [Given("the manifest's {string} is an empty list")]
    public void GivenTheManifestsIsAnEmptyList(string path) => Set(path, new JsonArray());

    [Given("the manifest's {string} is one entry with no {string}")]
    public void GivenTheManifestsIsOneEntryWithNo(string path, string member) => ((JsonObject) ((JsonArray) Node(path)!)[0]!).Remove(member);

    [Given("the manifest's {string} is one entry beside {string} {string}")]
    public void GivenTheManifestsIsOneEntryBeside(string path, string source, string value) => Set(source, JsonValue.Create(value));

    [Given("the manifest's {string} is one entry beside no {string}")]
    public void GivenTheManifestsIsOneEntryBesideNo(string path, string source) => Remove(source);

    [Given("{string} declares scenarios tagged {string} and {string}")]
    [Given("{string} has scenarios tagged {string} and {string}")]
    public void GivenDeclaresScenariosTaggedAnd(string path, string first, string second) =>
        Tree.WriteRaw(
            path,
            $"Feature: Arithmetic\n\n  {first}\n  Scenario: Adds\n    Given two numbers\n\n  {second}\n  Scenario: Adds three\n    Given three numbers\n");

    [Given(@"^""([^""]*)"" carries ""([^""]*)"" on a (Feature|Rule|Scenario|Scenario Outline) at line (\d+)$")]
    public void GivenCarriesOnAnElementAtLine(string path, string tag, string element, int line)
    {
        var lines = new List<string>();
        if (element != "Feature")
        {
            lines.Add("Feature: Arithmetic");
        }

        if (line - 2 < lines.Count)
        {
            throw new InvalidOperationException(
                $"A {element} at line {line} leaves no line above it for {tag}: a tag sits on a line before the keyword it declares on.");
        }

        while (lines.Count < line - 2)
        {
            lines.Add(string.Empty);
        }

        lines.Add(tag);
        lines.AddRange(element switch
        {
            "Feature" => ["Feature: Arithmetic", string.Empty, "  Scenario: Adds", "    Given two numbers"],
            "Rule" => ["  Rule: Sums", string.Empty, "    Scenario: Adds", "      Given two numbers"],
            "Scenario" => ["  Scenario: Adds", "    Given two numbers"],
            _ => ["  Scenario Outline: Adds <a>", "    Given <a>", string.Empty, "    Examples:", "      | a |", "      | 1 |"],
        });
        Tree.WriteRaw(path, string.Join('\n', lines) + "\n");
    }

    [Given(@"^""([^""]*)"" has a scenario tagged (.+)$")]
    public void GivenHasAScenarioTagged(string path, string tags)
    {
        var written = string.Join(' ', Regex.Matches(tags, "\"([^\"]*)\"").Select(static match => match.Groups[1].Value));
        Tree.WriteRaw(path, $"Feature: Arithmetic\n\n  {written}\n  Scenario: Adds\n    Given two numbers\n");
        _keywordLines["Scenario"] = [4];
    }

    [Given("{string} has a Scenario Outline tagged {string} with three Examples rows")]
    public void GivenHasAScenarioOutlineTaggedWithThreeExamplesRows(string path, string tag)
    {
        Tree.WriteRaw(
            path,
            $"Feature: Arithmetic\n\n  {tag}\n  Scenario Outline: Adds <a>\n    Given <a>\n\n"
            + "    Examples:\n      | a |\n      | 1 |\n      | 2 |\n      | 3 |\n");
        _keywordLines["Scenario Outline"] = [4];
    }

    [Given("{string} has a Scenario Outline tagged {string} with an Examples block tagged {string}")]
    public void GivenHasAScenarioOutlineTaggedWithAnExamplesBlockTagged(string path, string tag, string blockTag)
    {
        Tree.WriteRaw(
            path,
            $"Feature: Arithmetic\n\n  {tag}\n  Scenario Outline: Adds <a>\n    Given <a>\n\n"
            + $"    {blockTag}\n    Examples:\n      | a |\n      | 1 |\n      | 2 |\n");
        _keywordLines["Examples rows"] = [10, 11];
    }

    [Given("{string} has a Rule tagged {string} holding two untagged scenarios")]
    public void GivenHasARuleTaggedHoldingTwoUntaggedScenarios(string path, string tag)
    {
        Tree.WriteRaw(
            path,
            $"Feature: Arithmetic\n\n  {tag}\n  Rule: Sums\n\n    Scenario: Adds two\n      Given two numbers\n\n"
            + "    Scenario: Adds three\n      Given three numbers\n");
        _keywordLines["Rule"] = [4];
        _keywordLines["Rule scenarios"] = [6, 9];
        _claimFile = path;
    }

    [Given(@"^""([^""]*)"" carries ""([^""]*)"" on (a Scenario|a Scenario Outline|a Rule with two children|the Feature)$")]
    public void GivenCarriesOn(string path, string tag, string element)
    {
        var lines = ScenarioSetLayout.ToArray();
        lines[element switch
        {
            "the Feature" => 0,
            "a Scenario" => 2,
            "a Scenario Outline" => 5,
            _ => 12,
        }] = tag;
        Tree.WriteRaw(path, string.Join('\n', lines) + "\n");
        _keywordLines["Scenario"] = [4];
        _keywordLines["Scenario Outline"] = [7];
        _keywordLines["Rule scenarios"] = [16, 19];
    }

    [Given("{string} has a scenario at line {int} with no claim tag of its own, its Rule or its Feature")]
    public void GivenHasAScenarioAtLineWithNoClaimTagOfItsOwnItsRuleOrItsFeature(string path, int line)
    {
        var lines = new List<string>
        {
            "Feature: Arithmetic", string.Empty, "  @B-001", "  Scenario: Adds", "    Given two numbers", string.Empty, "  Rule: Sums",
        };
        if (line <= lines.Count)
        {
            throw new InvalidOperationException($"A scenario at line {line} leaves no room for the claimed scenario and the untagged Rule above it.");
        }

        while (lines.Count < line - 1)
        {
            lines.Add(string.Empty);
        }

        lines.AddRange(["    Scenario: Adds three", "      Given three numbers"]);
        Tree.WriteRaw(path, string.Join('\n', lines) + "\n");
    }

    [Given("{string} has an Examples block tagged {string} at line {int}")]
    public void GivenHasAnExamplesBlockTaggedAtLine(string path, string tag, int line)
    {
        var lines = new List<string> { "Feature: Arithmetic", string.Empty, "  @B-001", "  Scenario Outline: Adds <a>", "    Given <a>", string.Empty };
        if (line <= lines.Count)
        {
            throw new InvalidOperationException($"A tag at line {line} leaves no room for the claimed outline above its Examples block.");
        }

        while (lines.Count < line - 1)
        {
            lines.Add(string.Empty);
        }

        lines.AddRange([$"    {tag}", "    Examples:", "      | a |", "      | 1 |"]);
        Tree.WriteRaw(path, string.Join('\n', lines) + "\n");
    }

    [Given("{string} tags its Feature {string} and holds an untagged scenario")]
    public void GivenTagsItsFeatureAndHoldsAnUntaggedScenario(string path, string tag)
    {
        Tree.WriteRaw(path, $"{tag}\nFeature: Arithmetic\n\n  Scenario: Adds\n    Given two numbers\n");
        _keywordLines["Scenario"] = [4];
        _claimFile = path;
    }

    [Given("{string} opens with {string} at line {int}")]
    public void GivenOpensWithAtLine(string path, string header, int line)
    {
        Tree.WriteRaw(path, string.Concat(Enumerable.Repeat("# A comment\n", line - 1)) + header + "\n" + Adds);
        _claimFile = path;
    }

    [Given("{string} has no {string} line")]
    public void GivenHasNoLine(string path, string keyword)
    {
        const string Text = "  @B-001\n  Scenario: Adds\n    Given two numbers\n";
        Text.Should().NotContain(keyword);
        Tree.WriteRaw(path, Text);
        _claimFile = path;
    }

    [Given("{string} exists")]
    public void GivenExists(string path)
    {
        Tree.WriteRaw(path, Adds);
        _claimFile = path;
    }

    [Given("{string} tags its Feature {string} at line {int}")]
    public void GivenTagsItsFeatureAtLine(string path, string tag, int line)
    {
        Tree.WriteRaw(path, string.Concat(Enumerable.Repeat("# A comment\n", line - 1)) + tag + "\n" + Adds);
        _claimFile = path;
    }

    [Given("{string} tags its Feature {string}")]
    public void GivenTagsItsFeature(string path, string tag)
    {
        Tree.WriteRaw(path, tag + "\n" + Adds);
        _claimFiles.Add(path);
    }

    [Given("{string} has no Feature-level tag")]
    public void GivenHasNoFeatureLevelTag(string path)
    {
        Tree.WriteRaw(path, Adds);
        _claimFiles.Add(path);
    }

    [Given("no file is at that path")]
    public void GivenNoFileIsAtThatPath() => File.Delete(RecordPath());

    [Given("the file at that path cannot be read")]
    public void GivenTheFileAtThatPathCannotBeRead() => _lock = new FileStream(RecordPath(), FileMode.Open, FileAccess.Read, FileShare.None);

    [Given("the path leads outside the root")]
    public void GivenThePathLeadsOutsideTheRoot() =>
        Path.GetRelativePath(Tree.Root, RecordPath()).Split(Path.DirectorySeparatorChar)[0].Should().Be("..");

    [Given("the claim file set names a record at an absolute path")]
    public void GivenTheClaimFileSetNamesARecordAtAnAbsolutePath()
    {
        GivenTheClaimFileSetNamesTheRecord(".spec/features/arithmetic/README.md");
        _absoluteRecord = RecordPath();
        ClaimFileSet["record"] = _absoluteRecord;
    }

    [Given(@"^the claim file set names a record that (names no file|names a file that cannot be read|is an absolute path|leads outside the root)$")]
    public void GivenTheClaimFileSetNamesARecordThat(string condition)
    {
        GivenTheClaimFileSetNamesTheRecord(condition == "leads outside the root" ? "../outside/README.md" : ".spec/features/arithmetic/README.md");
        _recordFrontmatter["spec_status"] = "\"approved\"";
        _recordFrontmatter["depends_on"] = "[\"F2\"]";
        if (condition != "leads outside the root")
        {
            WriteRecord();
        }

        switch (condition)
        {
            case "names no file":
                GivenNoFileIsAtThatPath();
                break;
            case "names a file that cannot be read":
                GivenTheFileAtThatPathCannotBeRead();
                break;
            case "is an absolute path":
                _absoluteRecord = RecordPath();
                ClaimFileSet["record"] = _absoluteRecord;
                break;
            default:
                GivenThePathLeadsOutsideTheRoot();
                break;
        }
    }

    [Given("{string} has an untagged scenario titled {string}")]
    public void GivenHasAnUntaggedScenarioTitled(string path, string title)
    {
        Tree.WriteRaw(path, $"Feature: Arithmetic\n\n  Scenario: {title}\n    Given two numbers\n");
        _keywordLines["Scenario"] = [3];
        _claimFile = path;
    }

    [Given("a catalogue built from {string}")]
    public void GivenACatalogueBuiltFrom(string path)
    {
        _claimFile = path;
        Tree.WriteRaw(
            path,
            "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n\n  @B-002\n  Scenario: Subtracts\n    Given two numbers\n");
        _run = Launch();
        _catalogue = Catalogue();
        _catalogue.Should().NotBeEmpty("the file declares B-001 and B-002");
    }

    [Given("the file's scenarios are reordered and retitled with their tags kept")]
    public void GivenTheFilesScenariosAreReorderedAndRetitledWithTheirTagsKept() =>
        Tree.WriteRaw(
            _claimFile ?? throw new InvalidOperationException("No claim file was written."),
            "Feature: Arithmetic\n\n  @B-002\n  Scenario: Takes one number from another\n    Given two numbers\n\n"
            + "  @B-001\n  Scenario: Sums two numbers\n    Given two numbers\n");

    [Given("the manifest sets {string} to {string} and {string} to {string}")]
    public void GivenTheManifestSetsToAndTo(string first, string firstTag, string second, string secondTag)
    {
        Set(first, new JsonArray(firstTag));
        Set(second, new JsonArray(secondTag));
    }

    [Given("the manifest leaves {string} at its default")]
    public void GivenTheManifestLeavesAtItsDefault(string path) => Remove(path);

    [Given("the glob matches {string} and {string}")]
    public void GivenTheGlobMatchesAnd(string first, string second)
    {
        Tree.WriteRaw(first, "Feature: First\n\n  @B-001\n  Scenario: Subtracts\n    Given two numbers\n");
        Tree.WriteRaw(second, "Feature: Second\n\n  @B-002\n  Scenario: Adds\n    Given two numbers\n");
    }

    [Given("the claim file set has feature {string}")]
    public void GivenTheClaimFileSetHasFeature(string feature) => ClaimFileSet["feature"] = feature;

    [Given("the claim file set names the record {string}")]
    public void GivenTheClaimFileSetNamesTheRecord(string path)
    {
        ClaimFileSet["record"] = path;
        _record = path;
        if (Path.GetFullPath(Path.Combine(Tree.Root, path)).StartsWith(Tree.Root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            WriteRecord();
        }
    }

    [Given("{string} tags a scenario {string} at line {int}")]
    public void GivenTagsAScenarioAtLine(string path, string tag, int line) => GivenCarriesOnAnElementAtLine(path, tag, "Scenario", line);

    [Given("the record holds every manifest section except {string}")]
    [Given("the record lacks the section {string}")]
    public void GivenTheRecordHoldsEveryManifestSectionExcept(string title)
    {
        _recordSections = [.. SpechtTree.Sections.Where(section => !section.StartsWith($"## {title}\n", StringComparison.Ordinal))];
        _recordSections.Should().HaveCount(SpechtTree.Sections.Count - 1);
        WriteRecord();
    }

    [Given("three claim files sit beside the record")]
    public void GivenThreeClaimFilesSitBesideTheRecord()
    {
        var folder = Path.GetDirectoryName(_record ?? throw new InvalidOperationException("No record was named."))!.Replace('\\', '/');
        foreach (var (name, id) in new[] { ("add", "B-001"), ("subtract", "B-002"), ("total", "B-003") })
        {
            var path = $"{folder}/{name}.feature";
            Tree.WriteRaw(path, $"Feature: Arithmetic\n\n  @{id}\n  Scenario: {name}\n    Given numbers\n");
            _claimFiles.Add(path);
        }
    }

    [Given("the record's frontmatter sets {string} to {string}")]
    public void GivenTheRecordsFrontmatterSetsTo(string key, string value)
    {
        _recordFrontmatter[key] = ListKeys.Contains(key) ? $"[\"{value}\"]" : $"\"{value}\"";
        WriteRecord();
    }

    [Given("the record's frontmatter sets {string} to {string} and {string} to {string}")]
    public void GivenTheRecordsFrontmatterSetsToAndTo(string first, string firstValue, string second, string secondValue)
    {
        GivenTheRecordsFrontmatterSetsTo(first, firstValue);
        GivenTheRecordsFrontmatterSetsTo(second, secondValue);
    }

    [Given("the claim file set names no record")]
    public void GivenTheClaimFileSetNamesNoRecord() => ClaimFileSet.Remove("record");

    [Given("neither set names a record")]
    public void GivenNeitherSetNamesARecord()
    {
        foreach (var set in ((JsonArray) Node("claim.files")!).Cast<JsonObject>())
        {
            set.Remove("record");
        }
    }

    [Given("the glob matches {string}")]
    public void GivenTheGlobMatches(string path) => Tree.WriteRaw(path, "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n");

    [Given("the globs match {string} and {string}")]
    public void GivenTheGlobsMatchAnd(string first, string second) => GivenTheGlobMatchesAnd(first, second);

    [Given("no file matches {string} outside the exclusions")]
    public void GivenNoFileMatchesOutsideTheExclusions(string glob)
    {
        var directory = Path.Combine(Tree.Root, Path.GetDirectoryName(glob)!);
        (Directory.Exists(directory) ? Directory.EnumerateFiles(directory, Path.GetFileName(glob)) : []).Should().BeEmpty();
    }

    [Given("the layouts discover {string} with id {string}")]
    public void GivenTheLayoutsDiscoverWithId(string path, string id) => Tree.WriteSpecification(path, "0001", id);

    [Given("the manifest declares a second claim file set with glob {string} and feature {string}")]
    public void GivenTheManifestDeclaresASecondClaimFileSetWithGlobAndFeature(string glob, string feature) =>
        ((JsonArray) Node("claim.files")!).Add(new JsonObject { ["glob"] = glob, ["feature"] = feature });

    [Given("the manifest declares a layout named {string} that discovers one specification")]
    public void GivenTheManifestDeclaresALayoutNamedThatDiscoversOneSpecification(string name)
    {
        ((JsonArray) Manifest["layouts"]!).Insert(0, new JsonObject { ["name"] = name, ["glob"] = $"docs/{name}/**/spec.md" });
        Tree.WriteSpecification($"docs/{name}/spec.md", "0001", "F7");
    }

    [When("the check runs")]
    public void WhenTheCheckRuns() => _run = Launch();

    [When("the check runs with {string}")]
    public void WhenTheCheckRunsWith(string argument) => _run = Launch(argument);

    [When("the check runs again")]
    public void WhenTheCheckRunsAgain() => _run = Launch();

    [When("the check runs twice")]
    public void WhenTheCheckRunsTwice()
    {
        _run = Launch();
        _second = Launch();
    }

    [Then("the exit code is {int}")]
    public void ThenTheExitCodeIs(int code) => Run.ExitCode.Should().Be(code, Run.Stderr);

    [Then("the exit code is {int} without {string}")]
    public void ThenTheExitCodeIsWithout(int code, string argument)
    {
        _arguments.Should().NotContain(argument);
        Run.ExitCode.Should().Be(code, Run.Stderr);
    }

    [Then("stderr names {string}")]
    public void ThenStderrNames(string key) => Run.Stderr.Should().Contain(key);

    [Then("no violation is printed")]
    public void ThenNoViolationIsPrinted() => Run.Stdout.Should().NotMatchRegex(@": (error|warning) SPEC[0-9]");

    [Then("every claim comes from a specification's claims table")]
    public void ThenEveryClaimComesFromASpecificationsClaimsTable()
    {
        Run.ExitCode.Should().Be(0, Run.Stderr);
        var model = EngineServices.Reader().Read(Tree.Root, null);
        model.Features.Should().ContainSingle();
        model.Claims.Should().OnlyContain(claims => model.Features.Contains(claims.Key) && claims.Value.Section != null);
        model.Claims.Values.SelectMany(static claims => claims.Rows).Select(static claim => claim.Id).Should().Equal("B-001");
    }

    [Then("the claim file set declares no Feature")]
    public void ThenTheClaimFileSetDeclaresNoFeature()
    {
        var structure = EngineServices.Manifest(new FileSystem(), Tree.Root);
        structure.ClaimInput!.Source.Should().Be(SpechtClaimSource.Markdown);
        structure.ClaimInput.Files.Should().BeEmpty();
    }

    [Then("Feature {string} has the identity {string} {string}")]
    public void ThenFeatureHasTheIdentity(string feature, string epic, string expectedFeature)
    {
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        EngineServices.Manifest(new FileSystem(), Tree.Root).ClaimInput!.Files.Should().ContainSingle()
            .Which.Should().Match<SpechtClaimFileSet>(set => set.Feature == feature && set.Epic == epic && feature == expectedFeature);
    }

    [Then("Feature {string} has the identity {string}")]
    public void ThenFeatureHasTheIdentity(string feature, string identity) => NativeFeature(feature).Identity.Should().Be(identity);

    [Then("no Feature has the identity {string}")]
    public void ThenNoFeatureHasTheIdentity(string identity)
    {
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        Model().Features.Should().NotContain(feature => feature.Identity == identity);
    }

    [Then("Feature {string} has the lifecycle {string}")]
    public void ThenFeatureHasTheLifecycle(string feature, string lifecycle) => NativeFeature(feature).Lifecycle.Should().Be(lifecycle);

    [Then("Feature {string} depends on {string}")]
    public void ThenFeatureDependsOn(string feature, string target)
    {
        var native = NativeFeature(feature);
        native.Strings(native.Keys.DependsOn).Should().Equal(target);
    }

    [Then("Feature {string} declares no dependency edge")]
    public void ThenFeatureDeclaresNoDependencyEdge(string feature)
    {
        var native = NativeFeature(feature);
        native.Strings(native.Keys.DependsOn).Concat(native.Strings(native.Keys.Blocks)).Should().BeEmpty();
    }

    [Then("the record's frontmatter is held to the Feature schema")]
    public void ThenTheRecordsFrontmatterIsHeldToTheFeatureSchema()
    {
        var record = _record ?? throw new InvalidOperationException("No record was named.");
        Diagnostics().Should().NotContain(line => line.StartsWith($"{record}(", StringComparison.Ordinal)
            && (line.Contains(" SPEC1001:", StringComparison.Ordinal) || line.Contains(" SPEC1002:", StringComparison.Ordinal)));
        _recordFrontmatter["spec_status"] = "\"unknown\"";
        WriteRecord();
        _run = Launch();
        Diagnostics().Should().Contain(line =>
            line.StartsWith($"{record}(", StringComparison.Ordinal) && line.Contains(" SPEC1002:", StringComparison.Ordinal));
    }

    [Then(@"^the output has a line ""([^""]*)""$")]
    public void ThenTheOutputHasALine(string start) => Diagnostics().Should().Contain(line => line.StartsWith(start, StringComparison.Ordinal));

    [Then("the output has the line {string}")]
    public void ThenTheOutputHasTheLine(string line) =>
        Run.Stdout.Split('\n').Select(static output => output.TrimEnd('\r')).Should().Contain(line, Run.Stderr);

    [Then("no {string} is reported for the record")]
    public void ThenNoIsReportedForTheRecord(string ruleId)
    {
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        Diagnostics().Should().NotContain(line => line.StartsWith(Record, StringComparison.Ordinal) && line.Contains($" {ruleId}:", StringComparison.Ordinal));
    }

    [Then("no {string} is reported for the record's claims section")]
    public void ThenNoIsReportedForTheRecordsClaimsSection(string ruleId)
    {
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        var claims = Node("roles.claims")?.GetValue<string>() ?? "3. Acceptance Criteria";
        Diagnostics().Should().NotContain(line => line.StartsWith(Record, StringComparison.Ordinal)
            && line.Contains($" {ruleId}:", StringComparison.Ordinal)
            && line.EndsWith($"[{claims}]", StringComparison.Ordinal));
    }

    [Then("the output has a line for the record naming {string}")]
    public void ThenTheOutputHasALineForTheRecordNaming(string ruleId) =>
        Diagnostics().Should().Contain(line => line.StartsWith(Record, StringComparison.Ordinal) && line.Contains($" {ruleId}:", StringComparison.Ordinal));

    [Then("no {string} is reported for Feature {string}")]
    public void ThenNoIsReportedForFeature(string ruleId, string feature)
    {
        ClaimFileSet["feature"]!.GetValue<string>().Should().Be(feature);
        _claimFiles.Should().HaveCount(3);
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        Diagnostics().Should().NotContain(line => line.Contains($" {ruleId}:", StringComparison.Ordinal)
            && _claimFiles.Append(Record).Any(path => line.StartsWith(path, StringComparison.Ordinal)));
    }

    [Then("Feature {string} has no claim from that file")]
    public void ThenFeatureHasNoClaimFromThatFile(string feature) => ClaimSet(feature).Rows.Should().NotContain(claim => claim.Path == ClaimFile);

    [Then("neither Feature {string} nor Feature {string} has a claim from that file")]
    public void ThenNeitherFeatureNorFeatureHasAClaimFromThatFile(string first, string second)
    {
        ThenFeatureHasNoClaimFromThatFile(first);
        ThenFeatureHasNoClaimFromThatFile(second);
    }

    [Then("no {string} is reported for either file")]
    public void ThenNoIsReportedForEitherFile(string ruleId)
    {
        _claimFiles.Should().HaveCount(2);
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        Diagnostics().Should().NotContain(line => line.Contains($" {ruleId}:", StringComparison.Ordinal)
            && _claimFiles.Any(path => line.StartsWith(path, StringComparison.Ordinal)));
    }

    [Then("no output line holds an absolute path")]
    public void ThenNoOutputLineHoldsAnAbsolutePath()
    {
        var absolute = _absoluteRecord ?? throw new InvalidOperationException("No absolute record was named.");
        (Run.Stdout + Run.Stderr).Should().NotContain(Tree.Root).And.NotContain(absolute);
    }

    [Then("no violation is reported for the scenario")]
    public void ThenNoViolationIsReportedForTheScenario()
    {
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        Diagnostics().Should().NotContain(line => line.StartsWith(ScenarioPlace(), StringComparison.Ordinal));
    }

    [Then("the scenario is reported under {string}")]
    public void ThenTheScenarioIsReportedUnder(string ruleId) =>
        Diagnostics().Should().Contain(line =>
            line.StartsWith(ScenarioPlace(), StringComparison.Ordinal) && line.Contains($" {ruleId}:", StringComparison.Ordinal));

    [Then("Feature {string} has no claim {string}")]
    public void ThenFeatureHasNoClaim(string feature, string id) => ClaimSet(feature).Rows.Should().NotContain(claim => claim.Id == id);

    [Then("the output has a line {string} naming {string}")]
    public void ThenTheOutputHasALineNaming(string start, string ruleId) =>
        Diagnostics().Should().Contain(line => line.StartsWith(start, StringComparison.Ordinal) && line.Contains($" {ruleId}:", StringComparison.Ordinal));

    [Then("the output has a line {string} naming {string} with the identifier {string}")]
    public void ThenTheOutputHasALineNamingWithTheIdentifier(string start, string ruleId, string identifier) =>
        Diagnostics().Should().Contain(line => line.StartsWith(start, StringComparison.Ordinal)
            && line.Contains($" {ruleId}:", StringComparison.Ordinal)
            && line.EndsWith($"[{identifier}]", StringComparison.Ordinal));

    [Then("no {string} is reported")]
    public void ThenNoIsReported(string ruleId) => Diagnostics().Should().NotContain(line => line.Contains($" {ruleId}:", StringComparison.Ordinal));

    [Then("the summary lines count {int} under {string}, after the manifest's layouts")]
    public void ThenTheSummaryLinesCountUnderAfterTheManifestsLayouts(int count, string layout)
    {
        var entries = SummaryCounts();
        entries[ManifestLayoutCounts(entries).Length..].Should().Equal($"{layout} {count}");
    }

    [Then("the summary lines count no Feature under {string}")]
    public void ThenTheSummaryLinesCountNoFeatureUnder(string layout) => ThenTheSummaryLinesCountUnderAfterTheManifestsLayouts(0, layout);

    [Then("the summary lines have two {string} counts, the manifest layout's first")]
    public void ThenTheSummaryLinesHaveTwoCountsTheManifestLayoutsFirst(string layout) =>
        TwoEntries(SummaryCounts(), layout);

    [Then("the report's layouts count {int} under {string}, after the manifest's layouts")]
    public void ThenTheReportsLayoutsCountUnderAfterTheManifestsLayouts(int count, string layout)
    {
        var entries = ReportLayouts();
        entries[ManifestLayoutCounts(entries).Length..].Should().Equal($"{layout} {count}");
    }

    [Then("the report's layouts have two {string} entries, the manifest layout's first")]
    public void ThenTheReportsLayoutsHaveTwoEntriesTheManifestLayoutsFirst(string layout) => TwoEntries(ReportLayouts(), layout);

    [Then("the output equals the output of the same tree before this Feature")]
    public void ThenTheOutputEqualsTheOutputOfTheSameTreeBeforeThisFeature() =>
        Run.Should().Be((OutputBeforeThisFeature, string.Empty, 0));

    [Then("the output equals the output without {string}")]
    public void ThenTheOutputEqualsTheOutputWithout(string path) =>
        Run.Should().Be(_baseline ?? throw new InvalidOperationException($"No run without {path} was recorded."));

    [Then("Feature {string} has the claims {string} and {string}")]
    public void ThenFeatureHasTheClaimsAnd(string feature, string first, string second) =>
        ClaimSet(feature).Rows.Select(static claim => claim.Id).Should().Equal(first, second);

    [Then("no claims table is read for Feature {string}")]
    public void ThenNoClaimsTableIsReadForFeature(string feature) => ClaimSet(feature).Section.Should().BeNull();

    [Then("Feature {string} has the claim {string} located at {string} line {int}")]
    public void ThenFeatureHasTheClaimLocatedAtLine(string feature, string id, string path, int line)
    {
        var claim = ClaimSet(feature).Rows.Should().ContainSingle(claim => claim.Id == id).Subject;
        claim.Path.Should().Be(path);
        claim.Line.Should().Be(line);
    }

    [Then("Feature {string} has exactly the claim {string}")]
    public void ThenFeatureHasExactlyTheClaim(string feature, string id) =>
        ClaimSet(feature).Rows.Select(static claim => claim.Id).Should().Equal(id);

    [Then("no violation names {string} or {string}")]
    public void ThenNoViolationNamesOr(string first, string second) =>
        Diagnostics().Should().NotContain(line => line.Contains(first, StringComparison.Ordinal) || line.Contains(second, StringComparison.Ordinal));

    [Then("Feature {string} has the claim {string} exactly once")]
    public void ThenFeatureHasTheClaimExactlyOnce(string feature, string id)
    {
        ClaimSet(feature).Rows.Should().ContainSingle(claim => claim.Id == id);
        _claimId = id;
    }

    [Then(@"^the claim is located at the (Rule|Scenario Outline)'s keyword line$")]
    public void ThenTheClaimIsLocatedAtTheElementsKeywordLine(string element) =>
        Claim(_claimId).Line.Should().Be(_keywordLines[element].Single());

    [Then("no violation names either scenario")]
    public void ThenNoViolationNamesEitherScenario()
    {
        var places = _keywordLines["Rule scenarios"].Select(line => $"{_claimFile}({line})").Concat(["Adds two", "Adds three"]).ToArray();
        Diagnostics().Should().NotContain(line => places.Any(place => line.Contains(place, StringComparison.Ordinal)));
    }

    [Then("every claim has the same identifier, Feature identity and lifecycle as before")]
    public void ThenEveryClaimHasTheSameIdentifierFeatureIdentityAndLifecycleAsBefore() =>
        Catalogue().Should().BeEquivalentTo(_catalogue ?? throw new InvalidOperationException("No catalogue was built before."));

    [Then("the claim {string} has the status {string}")]
    public void ThenTheClaimHasTheStatus(string id, string status) => Claim(id).Declaration!.State.Should().Be(status);

    [Then("Feature {string} has the claim {string}")]
    public void ThenFeatureHasTheClaim(string feature, string id)
    {
        ClaimSet(feature).Rows.Should().ContainSingle(claim => claim.Id == id);
        _claimId = id;
    }

    [Then("the scenario is marked planned in the catalogue")]
    public void ThenTheScenarioIsMarkedPlannedInTheCatalogue() =>
        Claim(_claimId).Declaration!.Cases.Should().ContainSingle(item => item.Line == _keywordLines["Scenario"].Single())
            .Which.Planned.Should().BeTrue();

    [Then("that block's rows are marked planned in the catalogue")]
    public void ThenThatBlocksRowsAreMarkedPlannedInTheCatalogue()
    {
        var rows = _keywordLines["Examples rows"];
        Claim(_claimId).Declaration!.Cases.Where(item => rows.Contains(item.Line)).Select(static item => (item.Line, item.Planned))
            .Should().Equal(rows.Select(static row => (row, true)));
    }

    [Then(@"^the scenarios of claim ""([^""]*)"" are "
        + "(that Scenario|that Scenario Outline|both scenarios under the Rule|every Scenario and Scenario Outline in the file)$")]
    public void ThenTheScenariosOfClaimAre(string id, string scenarios) =>
        Claim(id).Declaration!.Scenarios.Should().Equal(scenarios switch
        {
            "that Scenario" => _keywordLines["Scenario"],
            "that Scenario Outline" => _keywordLines["Scenario Outline"],
            "both scenarios under the Rule" => _keywordLines["Rule scenarios"],
            _ => [.. _keywordLines["Scenario"], .. _keywordLines["Scenario Outline"], .. _keywordLines["Rule scenarios"]],
        });

    [Then("the two outputs are byte-identical")]
    public void ThenTheTwoOutputsAreByteIdentical() =>
        (_second ?? throw new InvalidOperationException("The check was not run twice.")).Should().Be(Run);

    [Then("the claims of {string} precede those of {string}")]
    public void ThenTheClaimsOfPrecedeThoseOf(string first, string second)
    {
        var paths = Model().ClaimSets.SelectMany(static set => set.Rows).Select(static claim => claim.Path).ToList();
        paths.Should().Contain(first).And.Contain(second);
        paths.FindLastIndex(path => path == first).Should().BeLessThan(paths.FindIndex(path => path == second));
    }

    [AfterScenario]
    public void DeleteTree()
    {
        _lock?.Dispose();
        _tree?.Dispose();
    }

    private SpechtTree Tree => _tree ?? throw new InvalidOperationException("No root was prepared.");

    private (string Stdout, string Stderr, int ExitCode) Run => _run ?? throw new InvalidOperationException("The check was not run.");

    private string ClaimFile => _claimFile ?? throw new InvalidOperationException("No claim file was written.");

    private string Record => _record ?? throw new InvalidOperationException("No record was named.");

    private JsonObject ClaimFileSet => (JsonObject) ((JsonArray) Node("claim.files")!)[0]!;

    private string ManifestPath => Path.Combine(Tree.Root, SpechtTree.ManifestPath);

    private JsonObject Manifest { get => field ?? throw new InvalidOperationException("No manifest was read."); set; }

    private SpechtModel Model() => EngineServices.Reader().Read(Tree.Root, null);

    private FeatureSpec NativeFeature(string feature)
    {
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        return Model().Features.Should().ContainSingle(spec => spec.ClaimFileSet != null && spec.ClaimFileSet.Feature == feature).Subject;
    }

    private string[] SummaryCounts() =>
        Run.Stdout.Split('\n').Select(static line => line.TrimEnd('\r'))
            .Should().ContainSingle(static line => line.StartsWith(SummaryStart, StringComparison.Ordinal))
            .Subject[SummaryStart.Length..].Split(", ");

    private string[] ReportLayouts() =>
        JsonNode.Parse(Run.Stdout)!["layouts"]!.AsArray()
            .Select(static entry => $"{entry!["layout"]!.GetValue<string>()} {entry["specificationCount"]!.GetValue<int>()}")
            .ToArray();

    private string[] ManifestLayoutCounts(string[] entries)
    {
        var names = ((JsonArray) Manifest["layouts"]!).Select(static layout => layout!["name"]!.GetValue<string>()).ToArray();
        entries.Length.Should().BeGreaterThan(names.Length, "the manifest's layouts are followed by one more entry");
        entries[..names.Length].Select(static entry => entry[..entry.LastIndexOf(' ')]).Should().Equal(names);
        return entries[..names.Length];
    }

    private void TwoEntries(string[] entries, string layout)
    {
        var manifest = ManifestLayoutCounts(entries);
        manifest.Should().ContainSingle(entry => entry.StartsWith($"{layout} ", StringComparison.Ordinal));
        entries.Should().HaveCount(manifest.Length + 1);
        entries[^1].Should().StartWith($"{layout} ");
    }

    private void WriteMatrix(string title, string rows)
    {
        _recordSections =
        [
            .. SpechtTree.SectionsWith(
                title,
                $"## {title}\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n{rows}"),
        ];
        WriteRecord();
    }

    private void WriteRecord() => Tree.WriteSpecification(Record, "0001", "F1", _recordFrontmatter, _recordSections);

    private SpechtClaims ClaimSet(string feature)
    {
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        return Model().ClaimSets.Should().ContainSingle(set => set.FileSet!.Feature == feature).Subject;
    }

    private string RecordPath()
    {
        var record = _record ?? throw new InvalidOperationException("No record was named.");
        return Path.GetFullPath(Path.Combine(Tree.Root, record.Replace('/', Path.DirectorySeparatorChar)));
    }

    private string ScenarioPlace() =>
        $"{_claimFile ?? throw new InvalidOperationException("No claim file was written.")}({_keywordLines["Scenario"].Single()}):";

    private SpechtClaim Claim(string? id) =>
        Model().ClaimSets.SelectMany(static set => set.Rows).Should().ContainSingle(claim => claim.Id == id).Subject;

    private (string Id, string? Epic, string Feature, string State)[] Catalogue() =>
        Model().ClaimSets
            .SelectMany(static set => set.Rows.Select(claim => (claim.Id, set.FileSet!.Epic, set.FileSet.Feature, claim.Declaration!.State)))
            .ToArray();

    private string[] Diagnostics() =>
        Run.Stdout.Split('\n').Where(static line => Regex.IsMatch(line, ": (error|warning) SPEC[0-9]")).ToArray();

    private (string Stdout, string Stderr, int ExitCode) Launch(params string[] arguments)
    {
        File.WriteAllText(ManifestPath, Manifest.ToJsonString());
        _arguments = arguments;
        return AcceptanceTool.Launch(Tree.Root, ["--root", ".", .. arguments]);
    }

    private JsonNode? Node(string path) =>
        path.Split('.').Aggregate<string, JsonNode?>(Manifest, static (node, member) => node?[member]);

    private void Set(string path, JsonNode? value)
    {
        var members = path.Split('.');
        var parent = Manifest;
        foreach (var member in members[..^1])
        {
            if (parent[member] is not JsonObject child)
            {
                child = [];
                parent[member] = child;
            }

            parent = child;
        }

        parent[members[^1]] = value;
    }

    private void Remove(string path)
    {
        var members = path.Split('.');
        (members.Length == 1 ? Manifest : Node(string.Join('.', members[..^1])) as JsonObject)?.Remove(members[^1]);
    }

    private const string SummaryStart = "specifications: ";

    private const string AddFile = ".spec/features/arithmetic/add.feature";

    private const string Adds = "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n";

    private static readonly string OutputBeforeThisFeature = string.Join(
        Environment.NewLine,
        "specifications: epics 0, features 1",
        "items: 0",
        "rules evaluated: 23",
        "errors: 0, warnings: 0",
        string.Empty);

    private static readonly string[] ListKeys = ["children", "depends_on", "blocks", "spikes"];

    private static readonly string[] ScenarioSetLayout =
    [
        string.Empty,
        "Feature: Arithmetic",
        string.Empty,
        "  Scenario: Adds",
        "    Given two numbers",
        string.Empty,
        "  Scenario Outline: Adds <a>",
        "    Given <a>",
        string.Empty,
        "    Examples:",
        "      | a |",
        "      | 1 |",
        string.Empty,
        "  Rule: Sums",
        string.Empty,
        "    Scenario: Adds three",
        "      Given three numbers",
        string.Empty,
        "    Scenario: Adds four",
        "      Given four numbers",
    ];

    private readonly List<string> _claimFiles = [];

    private readonly Dictionary<string, int[]> _keywordLines = new(StringComparer.Ordinal);

    private readonly Dictionary<string, string> _recordFrontmatter = new(StringComparer.Ordinal);

    private (string Id, string? Epic, string Feature, string State)[]? _catalogue;

    private (string Stdout, string Stderr, int ExitCode)? _second;

    private string[] _arguments = [];

    private string? _claimFile;

    private string? _claimId;

    private string? _record;

    private string[]? _recordSections;

    private string? _absoluteRecord;

    private FileStream? _lock;

    private SpechtTree? _tree;

    private (string Stdout, string Stderr, int ExitCode)? _run;

    private (string Stdout, string Stderr, int ExitCode)? _baseline;
}
