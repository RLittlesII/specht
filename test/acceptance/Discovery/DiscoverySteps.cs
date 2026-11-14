using System.Diagnostics;
using System.IO.Abstractions;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Discovery;
using Specht.Evaluation;
using Specht.Model;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Discovery;

/// <summary>
/// Steps for <c>src/specht/Discovery/.spec/discovery.feature</c> (0001-F6): item 0005 binds B-001, B-002, B-003, B-009 and B-012,
/// item 0006 binds B-004, item 0007 binds the first B-006 scenario, and item 0008 binds B-005, B-010 and the B-006 scenario
/// without git on the path.
/// The manifest is edited in memory from the tree's default copy and written when the check runs, and "the check runs" is
/// the engine's runner; the summary is the lines its report document makes and the report is that document serialized,
/// which <c>0001-F2</c> B-002 and <c>0001-F3</c> B-001 hold the tool's own output to. A specification a step writes has no
/// companion beside it unless the step says so, so one that is discovered is reported under its own path and one that is
/// not is reported nowhere. A layout's path identity is written into its <c>layouts</c> entry as <c>identity</c>, and a
/// specification is reported for the mismatch when the path-identity rule names its file once (B-004; decision 0009). The
/// check runs over the real file system through one whose directory records every listing it is asked for and every path
/// a listing returns, so a directory is opened when a listing asks for it or a listing returns a path below it (B-006,
/// C-1). A git work tree is made by the real <c>git</c> in the scenario's root, and the check's child process records each
/// program discovery asks for and what it gave back; "no git executable is on the path" starts a program that does not
/// exist in git's place, so the process-wide <c>PATH</c> is never changed (B-005, B-006, B-010). Item 0009 binds B-007, B-008
/// and B-011: the specifications, items, epics and companions are discovered directly, once through the real <c>git</c> and
/// once with git's place taken by that absent program, each run required to have listed through git with exit code <c>0</c>
/// or to have started nothing that ran, so two walks never compare equal. A root that is not yet a work tree is made one
/// before it is discovered through git. The clean checkout is a shared clone of the repository this assembly was built in,
/// checked out at that repository's <c>HEAD</c> in a temporary directory with no hook able to run, and nothing is
/// committed from it; a link is made with a target relative to the root, so the tree holds no absolute path.
/// </summary>
[Binding]
[Scope(Feature = "Discovery")]
public sealed class DiscoverySteps(PackedTool tool)
{
    [Given("a repository root holding a manifest and the three frontmatter schemas")]
    public void GivenARepositoryRootHoldingAManifestAndTheThreeFrontmatterSchemas() => _tree = new SpechtTree();

    [Given("the manifest is the default one this repository checks itself with")]
    public void GivenTheManifestIsTheDefaultOneThisRepositoryChecksItselfWith() =>
        Manifest = JsonNode.Parse(File.ReadAllText(ManifestPath))!.AsObject();

    [Given("the manifest declares a third layout whose specification file is a differently named markdown file under a documentation folder")]
    public void GivenTheManifestDeclaresAThirdLayout() =>
        Manifest["layouts"] = new JsonArray(
            Layout("epics", EpicsGlob),
            Layout("features", FeaturesGlob),
            Layout("documentation", "docs/**/specification.md"));

    [Given("the root holds a specification at that place")]
    public void GivenTheRootHoldsASpecificationAtThatPlace() =>
        Write(Subject = "docs/guide/specification.md", "F1", sections: ThreeDefects);

    [Given("the manifest declares one layout only, the features layout")]
    public void GivenTheManifestDeclaresOneLayoutOnlyTheFeaturesLayout() =>
        Manifest["layouts"] = new JsonArray(Layout("features", FeaturesGlob));

    [Given("the root holds a specification where the default epics layout would find it")]
    public void GivenTheRootHoldsASpecificationWhereTheDefaultEpicsLayoutWouldFindIt() =>
        Write(_underEpics = "epics/0001-epic/F1-feature/spec.md", "F1");

    [Given("the root holds a specification where the features layout finds it")]
    public void GivenTheRootHoldsASpecificationWhereTheFeaturesLayoutFindsIt() =>
        Write(_besideCode = "src/area/.spec/README.md", "F2");

    [Given("the manifest excludes the directory name {string}")]
    [Given("the manifest excludes the root-relative path {string}")]
    public void GivenTheManifestExcludes(string entry) => Manifest["exclusions"] = new JsonArray(entry);

    [Given("the root holds a co-located specification three levels below a {string} directory")]
    public void GivenTheRootHoldsACoLocatedSpecificationThreeLevelsBelowADirectory(string directory) =>
        Write(Subject = $"src/{directory}/one/two/three/.spec/README.md", "F1");

    [Given("the root holds a README in its root {string} folder")]
    public void GivenTheRootHoldsAReadmeInItsRootFolder(string folder) => Write(_rootReadme = $"{folder}/README.md", "F1");

    [Given("the root holds a co-located specification under {string}")]
    public void GivenTheRootHoldsACoLocatedSpecificationUnder(string folder) => Write($"{folder}/README.md", "F2");

    [Given("the root holds co-located specifications under {string} and {string}")]
    public void GivenTheRootHoldsCoLocatedSpecificationsUnder(string first, string second)
    {
        Write($"{first}/README.md", "F1");
        Write($"{second}/README.md", "F2");
        _pair = [$"{first}/README.md", $"{second}/README.md"];
    }

    [Given("the manifest declares an item file shape, an epic file glob and a companion glob")]
    public void GivenTheManifestDeclaresAnItemFileShapeAnEpicFileGlobAndACompanionGlob()
    {
        Manifest["taskFiles"] = new JsonArray("{task}-*.markdown");
        Manifest["epicFiles"] = new JsonArray("epics/*/epic.md");
        Manifest["companionFiles"] = new JsonArray("*.gherkin");
    }

    [Given("the manifest declares two item file shapes, two epic file globs and two companion globs")]
    public void GivenTheManifestDeclaresTwoItemFileShapesTwoEpicFileGlobsAndTwoCompanionGlobs()
    {
        Manifest["taskFiles"] = new JsonArray("{task}-*.md", "{task}-*.markdown");
        Manifest["epicFiles"] = new JsonArray("epics/**/epic.md", "portfolio/*/epic.md");
        Manifest["companionFiles"] = new JsonArray("*.feature", "*.gherkin");
    }

    [Given("the root holds, beside a specification, one item matching the first item shape and one matching the second")]
    public void GivenTheRootHoldsBesideASpecificationOneItemMatchingTheFirstItemShapeAndOneMatchingTheSecond()
    {
        var specification = Write(
            "src/area/.spec/README.md",
            "F1",
            new Dictionary<string, string> { ["children"] = "[\"0001-01\", \"0001-02\"]" });
        var directory = Path.GetDirectoryName(specification)!;
        Tree.WriteItem(specification, "0001-01", "F1");
        Tree.WriteItem(specification, "0001-02", "F1");
        File.Move(Path.Combine(directory, "0001-02-item.md"), Path.Combine(directory, "0001-02-item.markdown"));
    }

    [Given("the root holds one epic file matching the first epic glob and one matching the second")]
    public void GivenTheRootHoldsOneEpicFileMatchingTheFirstEpicGlobAndOneMatchingTheSecond()
    {
        var urgent = new Dictionary<string, string> { ["priority"] = "urgent" };
        Tree.WriteEpic("0002", urgent);
        var written = Path.Combine(Tree.Root, "epics", "0002-epic");
        File.Move(Path.Combine(written, "epic.md"), Tree.WriteRaw(SecondEpic, string.Empty), overwrite: true);
        Directory.Delete(written);
        Tree.WriteEpic("0001", urgent);
    }

    [Given("the root holds, beside a second specification, one companion matching the second companion glob only")]
    public void GivenTheRootHoldsBesideASecondSpecificationOneCompanionMatchingTheSecondCompanionGlobOnly()
    {
        Write(SecondSpecification, "F2");
        Tree.WriteRaw(SecondCompanion, PhantomTag);
    }

    [Given("the root holds one item and one companion matching them beside a specification")]
    public void GivenTheRootHoldsOneItemAndOneCompanionMatchingThemBesideASpecification()
    {
        var specification = Write("src/area/.spec/README.md", "F1", new Dictionary<string, string> { ["children"] = "[\"0001-01\"]" });
        var directory = Path.GetDirectoryName(specification)!;
        Tree.WriteItem(specification, "0001-01", "F1");
        File.Move(Path.Combine(directory, "0001-01-item.md"), Path.Combine(directory, "0001-01-item.markdown"));
        Tree.WriteRaw(Companion, PhantomTag);
    }

    [Given("the root holds an epic file at {string}")]
    public void GivenTheRootHoldsAnEpicFileAt(string path)
    {
        Tree.WriteEpic("0001", new Dictionary<string, string> { ["priority"] = "urgent" });
        var written = Path.Combine(Tree.Root, "epics", "0001-epic");
        File.Move(Path.Combine(written, "epic.md"), Tree.WriteRaw(path, string.Empty), overwrite: true);
        Directory.Delete(written);
        _epic = path;
    }

    [Given("the manifest names its layouts {string} and {string}")]
    public void GivenTheManifestNamesItsLayouts(string first, string second) =>
        Manifest["layouts"] = new JsonArray(Layout(first, EpicsGlob), Layout(second, FeaturesGlob));

    [Given("the root holds one specification in each layout")]
    [Given("the root holds one specification under the epics folder and one beside code")]
    public void GivenTheRootHoldsOneSpecificationInEachLayout()
    {
        Write("epics/0001-epic/F1-feature/spec.md", "F1");
        Write("src/area/.spec/README.md", "F2");
    }

    [Given("the manifest's epics layout declares which path segments carry the epic and the Feature id")]
    public void GivenTheManifestsEpicsLayoutDeclaresWhichPathSegmentsCarryTheEpicAndTheFeatureId() =>
        Entry("epics")["identity"] = Identity(1, 2);

    [Given("the manifest's features layout declares none")]
    public void GivenTheManifestsFeaturesLayoutDeclaresNone() => Entry("features").Remove("identity");

    [Given("the root holds a specification in the epics layout whose epic directory does not match its frontmatter")]
    public void GivenTheRootHoldsASpecificationInTheEpicsLayoutWhoseEpicDirectoryDoesNotMatchItsFrontmatter() =>
        Write(_underEpics = "epics/0002-epic/F1-feature/spec.md", "F1");

    [Given("the root holds a specification in the features layout under a folder named nothing like its identity")]
    public void GivenTheRootHoldsASpecificationInTheFeaturesLayoutUnderAFolderNamedNothingLikeItsIdentity() =>
        Write(_besideCode = "src/area/.spec/README.md", "F2");

    [Given("the manifest declares a third layout under a documentation folder, two folders deeper than the epics layout")]
    public void GivenTheManifestDeclaresAThirdLayoutUnderADocumentationFolderTwoFoldersDeeperThanTheEpicsLayout()
    {
        _layoutFolder = "docs/archive/tree";
        Manifest["layouts"]!.AsArray().Add(Layout("documentation", $"{_layoutFolder}/**/spec.md"));
    }

    [Given("that layout declares the path segments that carry the epic and the Feature id at their places in it")]
    public void GivenThatLayoutDeclaresThePathSegmentsThatCarryTheEpicAndTheFeatureIdAtTheirPlacesInIt() =>
        Entry("documentation")["identity"] = Identity(3, 4);

    [Given("the root holds a specification in that layout whose epic and Feature folders match its frontmatter")]
    public void GivenTheRootHoldsASpecificationInThatLayoutWhoseEpicAndFeatureFoldersMatchItsFrontmatter() =>
        Write(_first = $"{_layoutFolder}/0001-epic/F1-feature/spec.md", "F1");

    [Given("the root holds a second specification in that layout whose Feature folder does not match its frontmatter")]
    public void GivenTheRootHoldsASecondSpecificationInThatLayoutWhoseFeatureFolderDoesNotMatchItsFrontmatter() =>
        Write(Subject = $"{_layoutFolder}/0001-epic/F9-feature/spec.md", "F2");

    [Given("the manifest renames its epics layout {string} and keeps the path segments that layout declares")]
    public void GivenTheManifestRenamesItsEpicsLayoutAndKeepsThePathSegmentsThatLayoutDeclares(string name)
    {
        _layoutFolder = "epics";
        Entry("epics")["name"] = name;
    }

    [Given("the root holds a specification in that layout whose Feature folder does not match its frontmatter")]
    public void GivenTheRootHoldsASpecificationInThatLayoutWhoseFeatureFolderDoesNotMatchItsFrontmatter() =>
        Write(Subject = $"{_layoutFolder}/0001-epic/F9-feature/spec.md", "F1");

    [Given("the root is not inside a git work tree")]
    public void GivenTheRootIsNotInsideAGitWorkTree()
    {
        for (var directory = new DirectoryInfo(Tree.Root); directory is not null; directory = directory.Parent)
        {
            Path.Exists(Path.Combine(directory.FullName, ".git")).Should().BeFalse("the root must lie outside every git work tree");
        }
    }

    [Given("{string} holds a co-located specification three levels deep")]
    public void GivenHoldsACoLocatedSpecificationThreeLevelsDeep(string directory) =>
        Write(Subject = $"{directory}/one/two/three/.spec/README.md", "F1");

    [Given("the root is a git work tree")]
    public void GivenTheRootIsAGitWorkTree() => Git("init", "--quiet");

    [Given("the root is a git work tree whose ignore file lists {string}")]
    public void GivenTheRootIsAGitWorkTreeWhoseIgnoreFileLists(string directory)
    {
        Git("init", "--quiet");
        Tree.WriteRaw(".gitignore", directory + "\n");
    }

    [Given("the manifest does not exclude {string}")]
    public void GivenTheManifestDoesNotExclude(string directory) =>
        Manifest["exclusions"]!.AsArray().Select(static entry => entry!.GetValue<string>()).Should().NotContain([directory, "/" + directory]);

    [Given("the root holds a co-located specification that is not yet tracked and not ignored")]
    public void GivenTheRootHoldsACoLocatedSpecificationThatIsNotYetTrackedAndNotIgnored()
    {
        Write(Subject = "src/area/.spec/README.md", "F1");
        Untracked().Should().Contain(Subject);
    }

    [Given("the manifest excludes the root-relative path {string} and the directory name {string}")]
    public void GivenTheManifestExcludesTheRootRelativePathAndTheDirectoryName(string path, string directory) =>
        Manifest["exclusions"] = new JsonArray(path, directory);

    [Given("a README in the root {string} folder is tracked")]
    public void GivenAReadmeInTheRootFolderIsTracked(string folder)
    {
        Write(_rootReadme = $"{folder}/README.md", "F1");
        Git("add", _rootReadme);
    }

    [Given("a co-located specification under a {string} directory is tracked")]
    public void GivenACoLocatedSpecificationUnderADirectoryIsTracked(string directory)
    {
        Write(_tracked = $"src/{directory}/one/.spec/README.md", "F2");
        Git("add", _tracked);
    }

    [Given("a second co-located specification under a {string} directory is neither tracked nor ignored")]
    public void GivenASecondCoLocatedSpecificationUnderADirectoryIsNeitherTrackedNorIgnored(string directory)
    {
        Write(_untracked = $"tools/{directory}/two/.spec/README.md", "F3");
        Untracked().Should().Contain(_untracked);
    }

    [Given("no git executable is on the path")]
    public void GivenNoGitExecutableIsOnThePath() => _childProcess = new RecordingChildProcess(AbsentProgram);

    [Given("a clean checkout of this repository at one commit")]
    public void GivenACleanCheckoutOfThisRepositoryAtOneCommit()
    {
        var commit = GitIn(tool.Repository, "rev-parse", "HEAD").Trim();
        _checkoutParent = Directory.CreateTempSubdirectory("specht-checkout-").FullName;
        var checkout = Path.Combine(_checkoutParent, "checkout");
        GitIn(_checkoutParent, "clone", "--quiet", "--shared", "--no-checkout", tool.Repository, checkout);
        GitIn(checkout, "-c", "core.hooksPath=" + Path.Combine(_checkoutParent, "no-hooks"), "checkout", "--quiet", "--detach", commit);
        GitIn(checkout, "rev-parse", "HEAD").Trim().Should().Be(commit);
        GitIn(checkout, "status", "--porcelain", "--untracked-files=all", "--ignored")
            .Should().BeEmpty("a clean checkout holds no change and no untracked or ignored path");
        _checkout = checkout;
    }

    [Given("the root holds specifications under {string}, {string} and {string}")]
    public void GivenTheRootHoldsSpecificationsUnder(string first, string second, string third)
    {
        _folders = [first, second, third];
        foreach (var (folder, index) in _folders.Select(static (folder, index) => (folder, index)))
        {
            Write($"{folder}/.spec/README.md", $"F{index + 1}");
        }
    }

    [Given("the root holds a symbolic link to a directory outside it")]
    public void GivenTheRootHoldsASymbolicLinkToADirectoryOutsideIt()
    {
        _outside = Directory.CreateTempSubdirectory("specht-outside-").FullName;
        Directory.CreateSymbolicLink(Path.Combine(Tree.Root, Link), Path.GetRelativePath(Tree.Root, _outside));
    }

    [Given("that directory holds a co-located specification")]
    public void GivenThatDirectoryHoldsACoLocatedSpecification()
    {
        var outside = _outside ?? throw new InvalidOperationException("No step made the directory outside the root.");
        var directory = Directory.CreateDirectory(Path.Combine(outside, "area", ".spec"));
        File.WriteAllText(Path.Combine(directory.FullName, "README.md"), string.Empty);
        Subject = $"{Link}/area/.spec/README.md";
        File.Exists(Path.Combine(Tree.Root, Subject)).Should().BeTrue("the specification is reachable through the link");
    }

    [When("specifications, items, epics and companions are discovered through git")]
    [When("they are discovered through git")]
    [When("the specifications are discovered through git")]
    public void WhenTheyAreDiscoveredThroughGit()
    {
        if (_checkout is null)
        {
            File.WriteAllText(ManifestPath, Manifest.ToJsonString());
            if (!Path.Exists(Path.Combine(Tree.Root, ".git")))
            {
                Git("init", "--quiet");
            }
        }

        _gitRun = new RecordingChildProcess();
        ThroughGit = Discover(_gitRun);
    }

    [When("they are discovered by walking the tree")]
    public void WhenTheyAreDiscoveredByWalkingTheTree()
    {
        _walkRun = new RecordingChildProcess(AbsentProgram);
        Walked = Discover(_walkRun);
    }

    [When("the check runs")]
    public void WhenTheCheckRuns()
    {
        File.WriteAllText(ManifestPath, Manifest.ToJsonString());
        Report = EngineServices.Check(fileSystem: _fileSystem, childProcess: _childProcess).Run(Tree.Root, null);
    }

    [Then("that specification is discovered")]
    public void ThenThatSpecificationIsDiscovered() => Reported.Should().Contain(Subject);

    [Then("that specification is not discovered")]
    public void ThenThatSpecificationIsNotDiscovered() => Reported.Should().NotContain(Subject);

    [Then("it is checked by every rule")]
    public void ThenItIsCheckedByEveryRule()
    {
        using var twin = new SpechtTree();
        twin.WriteSpecification("src/area/.spec/README.md", "0001", "F1", sections: ThreeDefects);
        var expected = twin.Run().Violations.Select(Verdict).ToList();
        expected.Select(static verdict => verdict.RuleId).Distinct().Should().HaveCountGreaterThanOrEqualTo(3);
        Report.Violations.Where(violation => violation.File == Subject).Select(Verdict).Should().Equal(expected);
    }

    [Then("only the specification in the features layout is discovered")]
    public void ThenOnlyTheSpecificationInTheFeaturesLayoutIsDiscovered()
    {
        Reported.Should().Contain(_besideCode).And.NotContain(_underEpics);
        Report.SpecificationCount.Should().Be(1);
    }

    [Then("the summary names one layout")]
    public void ThenTheSummaryNamesOneLayout() => Summary[0].Should().Be("specifications: features 1");

    [Then("the root README is not discovered")]
    public void ThenTheRootReadmeIsNotDiscovered() => Reported.Should().NotContain(_rootReadme);

    [Then("the specification under {string} is discovered")]
    public void ThenTheSpecificationUnderIsDiscovered(string folder) => Reported.Should().Contain($"{folder}/README.md");

    [Then("neither specification is discovered")]
    public void ThenNeitherSpecificationIsDiscovered()
    {
        _pair.Should().HaveCount(2);
        Reported.Should().NotContain(_pair);
    }

    [Then("the summary counts one item")]
    public void ThenTheSummaryCountsOneItem() => Summary[1].Should().Be("items: 1");

    [Then("the epic's frontmatter is checked")]
    public void ThenTheEpicsFrontmatterIsChecked() =>
        Report.Violations.Should().Contain(violation => violation.RuleId == RuleIds.Under(Tree, "SPEC004") && violation.File == _epic);

    [Then("the companion's tags are resolved against the specification")]
    public void ThenTheCompanionsTagsAreResolvedAgainstTheSpecification()
    {
        Report.Violations.Should().NotContain(violation => violation.RuleId == RuleIds.Under(Tree, "SPEC020"));
        Report.Violations.Where(violation => violation.RuleId == RuleIds.Under(Tree, "SPEC021"))
            .Should().ContainSingle()
            .Which.Should().Match<SpechtViolation>(static violation => violation.File == Companion && violation.Identifier == "B-404");
    }

    [Then("the summary counts two items")]
    public void ThenTheSummaryCountsTwoItems() => Summary[1].Should().Be("items: 2");

    [Then("both epics' frontmatter is checked")]
    public void ThenBothEpicsFrontmatterIsChecked() =>
        Report.Violations.Where(violation => violation.RuleId == RuleIds.Under(Tree, "SPEC004"))
            .Select(static violation => violation.File)
            .Distinct()
            .Should().BeEquivalentTo(FirstEpic, SecondEpic);

    [Then("that companion's tags are resolved against the second specification")]
    public void ThenThatCompanionsTagsAreResolvedAgainstTheSecondSpecification()
    {
        Report.Violations.Should().NotContain(violation => violation.RuleId == RuleIds.Under(Tree, "SPEC020") && violation.File == SecondSpecification);
        Report.Violations.Where(violation => violation.RuleId == RuleIds.Under(Tree, "SPEC021"))
            .Should().ContainSingle()
            .Which.Should().Match<SpechtViolation>(static violation => violation.File == SecondCompanion && violation.Identifier == "B-404");
    }

    [Then("the summary and the report name the layouts {string} and {string}")]
    [Then("the summary and the report name the layouts {string} and {string}, in that order")]
    public void ThenTheSummaryAndTheReportNameTheLayouts(string first, string second)
    {
        Summary[0].Should().Be($"specifications: {first} 1, {second} 1");
        JsonNode.Parse(new SpechtReportSerializer().Serialize(new SpechtReportDocumentMapper().Map(Report)))!["layouts"]!.AsArray()
            .Select(static layout => layout!["layout"]!.GetValue<string>())
            .Should().Equal(first, second);
    }

    [Then("the specification in the epics layout is reported for the mismatch")]
    public void ThenTheSpecificationInTheEpicsLayoutIsReportedForTheMismatch() => Mismatches(_underEpics).Should().ContainSingle();

    [Then("the specification in the features layout is not")]
    public void ThenTheSpecificationInTheFeaturesLayoutIsNot() => Mismatches(_besideCode).Should().BeEmpty();

    [Then("the second specification is reported for the mismatch")]
    [Then("that specification is reported for the mismatch")]
    public void ThenThatSpecificationIsReportedForTheMismatch() => Mismatches(Subject).Should().ContainSingle();

    [Then("the first is not")]
    public void ThenTheFirstIsNot() => Mismatches(_first).Should().BeEmpty();

    [Then("no directory under {string} is opened")]
    public void ThenNoDirectoryUnderIsOpened(string directory)
    {
        (_fileSystem.Recording.Count > 0 || _childProcess.Calls.Any(static call => call.Result is not null))
            .Should().BeTrue("the tree was listed, by the walk or by git");
        _fileSystem.Recording.Select(listing => Relative(listing.Asked))
            .Should().NotContain(path => path == directory || path.StartsWith(directory + "/", StringComparison.Ordinal));
        _fileSystem.Recording.SelectMany(static listing => listing.Returned).Select(Relative)
            .Should().NotContain(path => path.StartsWith(directory + "/", StringComparison.Ordinal));
    }

    [Then("none of the three files is discovered")]
    public void ThenNoneOfTheThreeFilesIsDiscovered() =>
        Reported.Should().NotContain(
            new[] { _rootReadme, _tracked, _untracked }.Select(static path => path ?? throw new InvalidOperationException("A step wrote none of the three.")));

    [Then("git is never asked to list the root {string} folder or any {string} directory")]
    public void ThenGitIsNeverAskedToListTheRootFolderOrAnyDirectory(string folder, string directory)
    {
        _childProcess.Calls.Should().NotBeEmpty().And.OnlyContain(static call => call.FileName == "git" && call.Result != null && call.Result.ExitCode == 0);
        _childProcess.Calls.SelectMany(static call => call.Result!.StandardOutput.Split('\0', StringSplitOptions.RemoveEmptyEntries))
            .Should().NotContain(path => path.StartsWith(folder + "/", StringComparison.Ordinal) || path.Split('/').Contains(directory));
    }

    [Then("discovery completes by walking the tree")]
    public void ThenDiscoveryCompletesByWalkingTheTree()
    {
        _childProcess.Calls.Should().NotBeEmpty().And.OnlyContain(static call => call.FileName == "git" && call.Result == null);
        _fileSystem.Recording.Select(listing => Relative(listing.Asked)).Should().Contain(".");
    }

    [Then("the exit code reflects the violations alone")]
    public void ThenTheExitCodeReflectsTheViolationsAlone()
    {
        var twin = Directory.CreateTempSubdirectory("specht-spec-").FullName;
        try
        {
            foreach (var file in Directory.EnumerateFiles(Tree.Root, "*", SearchOption.AllDirectories)
                         .Where(file => !Relative(file).StartsWith(".git/", StringComparison.Ordinal)))
            {
                var copy = Path.Combine(twin, Path.GetRelativePath(Tree.Root, file));
                Directory.CreateDirectory(Path.GetDirectoryName(copy)!);
                File.Copy(file, copy);
            }

            var outsideGit = EngineServices.Check().Run(twin, null);
            Report.Violations.Select(Placed).Should().Equal(outsideGit.Violations.Select(Placed));
        }
        finally
        {
            Directory.Delete(twin, recursive: true);
        }
    }

    [Then("the two sets are equal")]
    public void ThenTheTwoSetsAreEqual()
    {
        ThroughGit.Specifications.Should().BeEquivalentTo(Walked.Specifications);
        ThroughGit.Items.Should().BeEquivalentTo(Walked.Items);
        ThroughGit.Epics.Should().BeEquivalentTo(Walked.Epics);
        ThroughGit.Companions.Should().BeEquivalentTo(Walked.Companions);
        ThroughGit.Specifications.Should().NotBeEmpty();
        ThroughGit.Epics.Should().NotBeEmpty();
        ThroughGit.Companions.Should().NotBeEmpty();
    }

    [Then("both list them in the same order")]
    public void ThenBothListThemInTheSameOrder()
    {
        Paths(ThroughGit).Should().BeEquivalentTo(_folders.Select(static folder => $"{folder}/.spec/README.md"));
        Paths(Walked).Should().Equal(Paths(ThroughGit));
    }

    [Then("the order is by root-relative path, ordinally, so {string} comes before {string}")]
    public void ThenTheOrderIsByRootRelativePathOrdinallySoComesBefore(string before, string after)
    {
        foreach (var listed in new[] { Paths(ThroughGit), Paths(Walked) })
        {
            listed.Should().Equal(listed.Order(StringComparer.Ordinal));
            listed.Should().ContainInOrder($"{before}/.spec/README.md", $"{after}/.spec/README.md");
        }
    }

    [Then("neither discovers that specification")]
    public void ThenNeitherDiscoversThatSpecification()
    {
        Paths(ThroughGit).Should().NotContain(Subject);
        Paths(Walked).Should().NotContain(Subject);
    }

    [AfterScenario]
    public void DeleteTree()
    {
        _tree?.Dispose();
        foreach (var directory in new[] { _checkoutParent, _outside }.OfType<string>().Where(Directory.Exists))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static IReadOnlyList<string> ThreeDefects =>
        SpechtTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n")
            .Where(static section => !section.StartsWith("## 11.", StringComparison.Ordinal))
            .ToList();

    private SpechtTree Tree => _tree ?? throw new InvalidOperationException("No repository root was prepared.");

    private JsonObject Manifest { get => field ?? throw new InvalidOperationException("No manifest was read."); set; }

    private SpechtReport Report { get => field ?? throw new InvalidOperationException("The check gave no report."); set; }

    private string Subject { get => field ?? throw new InvalidOperationException("No step named that specification."); set; }

    private string ManifestPath => Path.Combine(Tree.Root, SpechtTree.ManifestPath);

    private Discovered ThroughGit
    {
        get
        {
            var run = _gitRun ?? throw new InvalidOperationException("Nothing was discovered through git.");
            run.Calls.Should().HaveCount(2, "the specifications and the epics are each listed through git once")
                .And.OnlyContain(static call => call.FileName == "git" && call.Result != null && call.Result.ExitCode == 0);
            return field ?? throw new InvalidOperationException("Nothing was discovered through git.");
        }

        set;
    }

    private Discovered Walked
    {
        get
        {
            var run = _walkRun ?? throw new InvalidOperationException("Nothing was discovered by walking.");
            run.Calls.Should().OnlyContain(static call => call.FileName == "git" && call.Result == null);
            return field ?? throw new InvalidOperationException("Nothing was discovered by walking.");
        }

        set;
    }

    private IEnumerable<string> Reported => Report.Violations.Select(static violation => violation.File).Distinct();

    private IReadOnlyList<string> Summary => new SpechtReportDocumentMapper().Map(Report).SummaryLines();

    private static JsonObject Layout(string name, string glob) => new() { ["name"] = name, ["glob"] = glob };

    private static JsonObject Identity(int epic, int feature) => new() { ["epic"] = epic, ["feature"] = feature };

    private static (string RuleId, SpechtSeverity Severity, int Line, string? Identifier, string Message) Verdict(SpechtViolation violation) =>
        (violation.RuleId, violation.Severity, violation.Line, violation.Identifier, violation.Message);

    private static (string File, string RuleId, SpechtSeverity Severity, int Line, string? Identifier, string Message) Placed(
        SpechtViolation violation) =>
        (violation.File, violation.RuleId, violation.Severity, violation.Line, violation.Identifier, violation.Message);

    private static IReadOnlyList<string> Paths(Discovered discovered) =>
        discovered.Specifications.Select(static specification => specification.Path).ToList();

    private static string GitIn(string directory, params string[] arguments)
    {
        var start = new ProcessStartInfo("git") { WorkingDirectory = directory };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        var (stdout, stderr, exitCode) = AcceptanceTool.Run(start);
        exitCode.Should().Be(0, stdout + stderr);

        return stdout;
    }

    private string Git(params string[] arguments) => GitIn(Tree.Root, arguments);

    private Discovered Discover(IChildProcess childProcess)
    {
        var fileSystem = new FileSystem();
        var root = _checkout ?? Tree.Root;
        var structure = EngineServices.Manifest(fileSystem, root);
        var discovery = new SpechtDiscovery(fileSystem, childProcess);
        var specifications = discovery.FindSpecifications(root, structure.Discovery);

        return new Discovered(
            [.. specifications.Select(static location => (location.RelativePath, location.Layout.Name))],
            discovery.FindChildItems(root, specifications, structure.Discovery, structure.Identifiers["task"]),
            discovery.FindEpics(root, structure.Discovery),
            [.. specifications.SelectMany(location => discovery.FindCompanions(root, location, structure.Discovery))]);
    }

    private string[] Untracked() => Git("ls-files", "-z", "--others", "--exclude-standard").Split('\0', StringSplitOptions.RemoveEmptyEntries);

    private string Relative(string path) =>
        Path.GetRelativePath(Path.GetFullPath(Tree.Root), Path.GetFullPath(path)).Replace(Path.DirectorySeparatorChar, '/');

    private JsonObject Entry(string layout) =>
        Manifest["layouts"]!.AsArray().Select(static entry => entry!.AsObject()).Single(entry => entry["name"]!.GetValue<string>() == layout);

    private IEnumerable<SpechtViolation> Mismatches(string? specification) =>
        Report.Violations.Where(violation =>
            violation.RuleId == RuleIds.Under(Tree, "SPEC011")
            && violation.File == (specification ?? throw new InvalidOperationException("No step wrote that specification.")));

    private string Write(
        string path,
        string id,
        IReadOnlyDictionary<string, string>? frontmatter = null,
        IReadOnlyList<string>? sections = null) =>
        Tree.WriteSpecification(path, "0001", id, frontmatter, sections);

    private sealed record Listing(string Asked, IReadOnlyList<string> Returned);

    private sealed record Call(string FileName, ChildProcessResult? Result);

    private sealed record Discovered(
        IReadOnlyList<(string Path, string Layout)> Specifications,
        IReadOnlyList<string> Items,
        IReadOnlyList<string> Epics,
        IReadOnlyList<string> Companions);

    private sealed class RecordingChildProcess(string? program = null) : IChildProcess
    {
        public List<Call> Calls { get; } = [];

        public ChildProcessResult? Run(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
        {
            var result = new ChildProcess().Run(program ?? fileName, arguments, workingDirectory);
            Calls.Add(new Call(fileName, result));
            return result;
        }
    }

    private sealed class RecordingFileSystem : FileSystem
    {
        public RecordingFileSystem() => _directory = new RecordingDirectory(this);

        public override IDirectory Directory => _directory ?? base.Directory;

        public List<Listing> Recording => _directory?.Recording ?? [];

        private readonly RecordingDirectory? _directory;
    }

    private sealed class RecordingDirectory(IFileSystem fileSystem) : DirectoryWrapper(fileSystem)
    {
        public List<Listing> Recording { get; } = [];

        public override IEnumerable<string> EnumerateFiles(string path) => Record(path, base.EnumerateFiles(path));

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern) =>
            Record(path, base.EnumerateFiles(path, searchPattern));

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) =>
            Record(path, base.EnumerateFiles(path, searchPattern, searchOption));

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
            Record(path, base.EnumerateFiles(path, searchPattern, enumerationOptions));

        public override IEnumerable<string> EnumerateDirectories(string path) => Record(path, base.EnumerateDirectories(path));

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern) =>
            Record(path, base.EnumerateDirectories(path, searchPattern));

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption) =>
            Record(path, base.EnumerateDirectories(path, searchPattern, searchOption));

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
            Record(path, base.EnumerateDirectories(path, searchPattern, enumerationOptions));

        public override IEnumerable<string> EnumerateFileSystemEntries(string path) => Record(path, base.EnumerateFileSystemEntries(path));

        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern) =>
            Record(path, base.EnumerateFileSystemEntries(path, searchPattern));

        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption) =>
            Record(path, base.EnumerateFileSystemEntries(path, searchPattern, searchOption));

        public override IEnumerable<string> EnumerateFileSystemEntries(
            string path, string searchPattern, EnumerationOptions enumerationOptions) =>
            Record(path, base.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions));

        public override string[] GetFiles(string path) => Record(path, base.GetFiles(path)).ToArray();

        public override string[] GetFiles(string path, string searchPattern) => Record(path, base.GetFiles(path, searchPattern)).ToArray();

        public override string[] GetFiles(string path, string searchPattern, SearchOption searchOption) =>
            Record(path, base.GetFiles(path, searchPattern, searchOption)).ToArray();

        public override string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
            Record(path, base.GetFiles(path, searchPattern, enumerationOptions)).ToArray();

        public override string[] GetDirectories(string path) => Record(path, base.GetDirectories(path)).ToArray();

        public override string[] GetDirectories(string path, string searchPattern) =>
            Record(path, base.GetDirectories(path, searchPattern)).ToArray();

        public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption) =>
            Record(path, base.GetDirectories(path, searchPattern, searchOption)).ToArray();

        public override string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
            Record(path, base.GetDirectories(path, searchPattern, enumerationOptions)).ToArray();

        public override string[] GetFileSystemEntries(string path) => Record(path, base.GetFileSystemEntries(path)).ToArray();

        public override string[] GetFileSystemEntries(string path, string searchPattern) =>
            Record(path, base.GetFileSystemEntries(path, searchPattern)).ToArray();

        public override string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption) =>
            Record(path, base.GetFileSystemEntries(path, searchPattern, searchOption)).ToArray();

        public override string[] GetFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
            Record(path, base.GetFileSystemEntries(path, searchPattern, enumerationOptions)).ToArray();

        private IReadOnlyList<string> Record(string path, IEnumerable<string> listed)
        {
            var returned = listed.ToList();
            Recording.Add(new Listing(path, returned));
            return returned;
        }
    }

    private const string EpicsGlob = "epics/**/spec.md";

    private const string FeaturesGlob = "**/.spec/README.md";

    private const string Companion = "src/area/.spec/thing.gherkin";

    private const string FirstEpic = "epics/0001-epic/epic.md";

    private const string SecondEpic = "portfolio/0002-epic/epic.md";

    private const string SecondSpecification = "src/other/.spec/README.md";

    private const string SecondCompanion = "src/other/.spec/thing.gherkin";

    private const string AbsentProgram = "specht-git-that-does-not-exist";

    private const string Link = "linked";

    private const string PhantomTag =
        "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n  @B-404\n  Scenario: Phantom\n    Given nothing\n";

    private readonly RecordingFileSystem _fileSystem = new();
    private RecordingChildProcess _childProcess = new();
    private SpechtTree? _tree;
    private string? _underEpics;
    private string? _besideCode;
    private string? _rootReadme;
    private string? _tracked;
    private string? _untracked;
    private string? _layoutFolder;
    private string? _first;
    private string? _epic;
    private IReadOnlyList<string> _pair = [];
    private IReadOnlyList<string> _folders = [];
    private string? _checkoutParent;
    private string? _checkout;
    private string? _outside;
    private RecordingChildProcess? _gitRun;
    private RecordingChildProcess? _walkRun;
}
