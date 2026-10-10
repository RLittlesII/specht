using System.Globalization;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using AwesomeAssertions;
using Reqnroll;
using Specht.Discovery;
using Specht.Tests;
using Specht.Tests.Baseline;

namespace Specht.Acceptance.Engine;

/// <summary>
/// Steps for <c>src/specht/.spec/engine.feature</c> (0001-F1). The Background is a <see cref="SpecTree"/> holding the live
/// version 0.1.0 schema set (A-3), which the Background reads back from the root it built. B-001 builds the baseline
/// tree on it and reads the rule ids the engine reports; B-004 holds the engine to the golden report on the same tree;
/// B-010 reads the one violation a missing traceability row gives.
/// B-002 writes one specification in each layout without its traceability section, B-003 one co-located specification
/// under a folder its identity does not name, and B-011 two co-located specifications of one identity. B-005 reads the built library itself: its assembly name,
/// and the namespace of every type it declares, which is where a root namespace shows once compiled. It reads the file
/// <c>src/specht</c> builds, from its metadata, and never the loaded assembly: a coverage run instruments the copy the
/// test process loads (0055-F3 B-001) and adds a type of its own to it. B-006 moves the baseline tree to a nested root,
/// runs under the <c>tr-TR</c> culture with an extra environment variable set, and reads the same library for a clock,
/// machine-name, environment or current-culture reference outside the one exempt site (decision 0006). B-007 replaces
/// the root with a deeply nested one, and B-008 writes a specification whose § 9 is a grid table.
/// </summary>
[Binding]
[Scope(Feature = "The engine, extracted unchanged")]
public sealed class EngineSteps
{
    [Given("a repository root holding a manifest and the three frontmatter schemas of schema version {word}")]
    public void GivenARepositoryRootHoldingAManifestAndTheThreeFrontmatterSchemasOfSchemaVersion(string version)
    {
        _tree = new SpecTree();
        SpecModel.Load(_tree.Root).Schemas.Version.Number.ToString().Should().Be(version);
    }

    [Given("the baseline tree the tests build, which breaks each of the twenty-one version 0.1.0 rules in each layout the rule applies to")]
    public void GivenTheBaselineTree() => BaselineTree.Write(Tree);

    [Given("the golden report the engine gave on that tree at the commit the copy landed on main")]
    public void GivenTheGoldenReport() => _golden = GoldenReport.Read();

    [Given("the root holds a specification whose claim {word} has no traceability row")]
    public void GivenTheRootHoldsASpecificationWhoseClaimHasNoTraceabilityRow(string claim) =>
        Tree.WriteFeature(
            "0001",
            "F1",
            sections: SpecTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + $"| B-001 | It does the thing. | brd | Active |\n| {claim} | It does another. | brd | Active |\n"));

    [Given("the root holds one specification in the epics layout missing its traceability section")]
    public void GivenTheRootHoldsOneSpecificationInTheEpicsLayoutMissingItsTraceabilitySection() =>
        _specifications.Add(SpecDiscovery.Relative(Tree.Root, Tree.WriteFeature("0001", "F1", sections: WithoutTraceability)));

    [Given("one specification in the features layout missing its traceability section")]
    public void GivenOneSpecificationInTheFeaturesLayoutMissingItsTraceabilitySection() =>
        _specifications.Add(
            SpecDiscovery.Relative(Tree.Root, Tree.WriteCoLocatedFeature("src/area", "0002", "F1", sections: WithoutTraceability)));

    [Given("the root holds a co-located specification declaring epic {string} and id {string} under a folder named nothing like it")]
    public void GivenTheRootHoldsACoLocatedSpecificationUnderAFolderNamedNothingLikeIt(string epic, string id) =>
        _specifications.Add(SpecDiscovery.Relative(Tree.Root, Tree.WriteCoLocatedFeature("src/unrelated/area", epic, id)));

    [Given("the root holds two specifications both declaring epic {string} and id {string}")]
    public void GivenTheRootHoldsTwoSpecificationsBothDeclaring(string epic, string id)
    {
        _identity = $"{epic}-{id}";
        _specifications.Add(SpecDiscovery.Relative(Tree.Root, Tree.WriteCoLocatedFeature("src/first", epic, id)));
        _specifications.Add(SpecDiscovery.Relative(Tree.Root, Tree.WriteCoLocatedFeature("src/second", epic, id)));
    }

    [When("the engine runs")]
    [When("the engine runs on that tree")]
    public void WhenTheEngineRuns() => _report = Tree.Run();

    [Then("a violation is reported under each of the twenty-one rule ids")]
    public void ThenAViolationIsReportedUnderEachOfTheTwentyOneRuleIds() =>
        Report.Violations.Select(static violation => violation.RuleId).Should().Contain(Vocabulary);

    [Then("no violation is reported under any other rule id")]
    public void ThenNoViolationIsReportedUnderAnyOtherRuleId() =>
        Report.Violations.Should().OnlyContain(static violation => Vocabulary.Contains(violation.RuleId));

    [Then("each specification is reported for the missing section")]
    public void ThenEachSpecificationIsReportedForTheMissingSection() =>
        _specifications.Should().HaveCount(2).And.AllSatisfy(path => Report.Violations.Should().Contain(violation =>
            violation.RuleId == "SPEC010" && violation.File == path && violation.Identifier == Traceability));

    [Then("neither is reported for the layout it is in")]
    public void ThenNeitherIsReportedForTheLayoutItIsIn() =>
        Report.Violations.Should().NotContain(violation => violation.RuleId == "SPEC011" && _specifications.Contains(violation.File));

    [Then("the specification is read as {string}")]
    public void ThenTheSpecificationIsReadAs(string identity) =>
        SpecModel.Load(Tree.Root).Features.Should().ContainSingle().Which.Identity.Should().Be(identity);

    [Then("no identity violation is reported")]
    public void ThenNoIdentityViolationIsReported() =>
        Report.Violations.Should().NotContain(static violation => violation.RuleId == "SPEC011" || violation.RuleId == "SPEC012");

    [Then("the duplicate identity is reported once on each specification, under one identifier")]
    public void ThenTheDuplicateIdentityIsReportedOnceOnEachSpecificationUnderOneIdentifier()
    {
        var duplicates = Report.Violations.Where(static violation => violation.RuleId == "SPEC012").ToList();
        duplicates.Select(static violation => violation.File).Should().BeEquivalentTo(_specifications);
        duplicates.Should().AllSatisfy(violation => violation.Identifier.Should().Be(_identity));
    }

    [Then("each report names both paths")]
    public void ThenEachReportNamesBothPaths() =>
        Report.Violations.Where(static violation => violation.RuleId == "SPEC012").Should().HaveCount(2).And.AllSatisfy(violation =>
            _specifications.Should().AllSatisfy(path => violation.Message.Should().Contain(path)));

    [Then("it reports the same violations as the golden report, with the same rule, severity, file, line, identifier and message")]
    public void ThenItReportsTheSameViolationsAsTheGoldenReport() => GoldenReport.Of(Report).Should().BeEquivalentTo(Golden);

    [Then("in the same order")]
    public void ThenInTheSameOrder() => GoldenReport.Of(Report).Should().Equal(Golden);

    [Then("the violation carries a rule id, a severity, a file, a line, an identifier and a message")]
    public void ThenTheViolationCarriesTheSixFields()
    {
        var violation = Report.Violations.Should().ContainSingle().Subject;
        violation.RuleId.Should().NotBeNullOrWhiteSpace();
        violation.Severity.Should().BeDefined();
        violation.File.Should().NotBeNullOrWhiteSpace();
        violation.Line.Should().BePositive();
        violation.Identifier.Should().NotBeNullOrWhiteSpace();
        violation.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Given("the library is built")]
    public void GivenTheLibraryIsBuilt()
    {
        _library = EngineAssembly.Built;
        File.Exists(_library).Should().BeTrue(_library);
    }

    [When("its assembly name and root namespace are read")]
    public void WhenItsAssemblyNameAndRootNamespaceAreRead()
    {
        using var file = new PEReader(File.OpenRead(Library));
        var metadata = file.GetMetadataReader();
        _assemblyName = metadata.GetString(metadata.GetAssemblyDefinition().Name);
        _namespaces = metadata.TypeDefinitions
            .Select(metadata.GetTypeDefinition)
            .Where(type => !IsCompilerGenerated(metadata, type))
            .Select(type => NamespaceOf(metadata, type))
            .Where(static ns => ns.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    [Then("both are {string}")]
    public void ThenBothAre(string name)
    {
        _assemblyName.Should().Be(name);
        _namespaces.Should()
            .NotBeEmpty()
            .And.AllSatisfy(ns => (ns.Equals(name,
                                       StringComparison.OrdinalIgnoreCase) ||
                                   ns.StartsWith($"{name}.",
                                       StringComparison.OrdinalIgnoreCase)).Should()
                .BeTrue(ns));
    }

    [Then("neither contains {string}")]
    public void ThenNeitherContains(string name)
    {
        _assemblyName.Should().NotContain(name);
        _namespaces.Should().AllSatisfy(ns => ns.Should().NotContain(name));
    }

    [Given("the baseline tree and the golden report")]
    public void GivenTheBaselineTreeAndTheGoldenReport()
    {
        BaselineTree.Write(Tree);
        _golden = GoldenReport.Read();
    }

    [When("the engine runs again at a different root location, under a different locale and with an extra environment variable set")]
    public void WhenTheEngineRunsAgainAtADifferentRootLocationUnderADifferentLocaleAndWithAnExtraEnvironmentVariableSet()
    {
        Tree.Dispose();
        _tree = new SpecTree(["moved", "elsewhere"]);
        BaselineTree.Write(_tree);
        var variable = "SPECHT_B006_" + Guid.NewGuid().ToString("N");
        var culture = CultureInfo.CurrentCulture;
        var uiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");
            Environment.SetEnvironmentVariable(variable, "set");
            _report = _tree.Run();
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = uiCulture;
        }
    }

    [Then("it reports the same violations as the golden report, in the same order")]
    public void ThenItReportsTheSameViolationsAsTheGoldenReportInTheSameOrder() => GoldenReport.Of(Report).Should().Equal(Golden);

    [Then("the engine reads no clock and no machine name")]
    public void ThenTheEngineReadsNoClockAndNoMachineName() =>
        EngineAssembly.Unexempted(EngineAssembly.ForbiddenReferencesIn(EngineAssembly.Built)).Should().BeEmpty();

    [Given("the root is a deeply nested directory on this machine")]
    public void GivenTheRootIsADeeplyNestedDirectoryOnThisMachine()
    {
        Tree.Dispose();
        _tree = new SpecTree(["a", "deeply", "nested", "folder", "on", "this", "machine"]);
    }

    [Given("the root holds a specification with one violation")]
    public void GivenTheRootHoldsASpecificationWithOneViolation() =>
        Tree.WriteFeature(
            "0001",
            "F1",
            sections: SpecTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another. | brd | Active |\n"));

    [Then("the violation's file is relative to the root")]
    public void ThenTheViolationsFileIsRelativeToTheRoot()
    {
        var file = Report.Violations.Should().ContainSingle().Subject.File;
        file.Should().Be("epics/0001-epic/F1-feature/spec.md");
        Path.IsPathRooted(file).Should().BeFalse(file);
    }

    [Then("it uses a forward slash where the platform's separator differs")]
    public void ThenItUsesAForwardSlashWhereThePlatformsSeparatorDiffers() =>
        Report.Violations.Should().ContainSingle().Which.File.Should().Contain("/").And.NotContain("\\");

    [Given("the root holds a specification whose traceability section holds a grid table")]
    public void GivenTheRootHoldsASpecificationWhoseTraceabilitySectionHoldsAGridTable() =>
        Tree.WriteFeature(
            "0001",
            "F1",
            sections: SpecTree.SectionsWith(
                Traceability,
                $"## {Traceability}\n\n"
                    + "+----------+-------------------+---------+---------+\n"
                    + "| Claim ID | Scenario          | Test    | Status  |\n"
                    + "+==========+===================+=========+=========+\n"
                    + "| B-001    | It does the thing | Missing | Missing |\n"
                    + "+----------+-------------------+---------+---------+\n"
                    + "| B-999    | A grid-only row   | Missing | Missing |\n"
                    + "+----------+-------------------+---------+---------+\n"));

    [Then("the section is reported as carrying no table")]
    public void ThenTheSectionIsReportedAsCarryingNoTable() =>
        Report.Violations.Should().ContainSingle(static violation => violation.RuleId == "SPEC013")
            .Which.Identifier.Should().Be(Traceability);

    [Then("no row of the grid is read as a claim")]
    public void ThenNoRowOfTheGridIsReadAsAClaim()
    {
        Report.Violations.Should().ContainSingle(static violation => violation.RuleId == "SPEC031").Which.Identifier.Should().Be("B-001");
        Report.Violations.Should().NotContain(static violation => violation.Identifier == "B-999");
    }

    [AfterScenario]
    public void DeleteRoot() => _tree?.Dispose();

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No repository root was prepared.");

    private SpechtReport Report => _report ?? throw new InvalidOperationException("The engine has not run.");

    private IReadOnlyList<GoldenReport.Verdict> Golden => _golden ?? throw new InvalidOperationException("No golden report was read.");

    private string Library => _library ?? throw new InvalidOperationException("No library was built.");

    private static string NamespaceOf(MetadataReader metadata, TypeDefinition type) =>
        type.GetDeclaringType().IsNil
            ? metadata.GetString(type.Namespace)
            : NamespaceOf(metadata, metadata.GetTypeDefinition(type.GetDeclaringType()));

    private static bool IsCompilerGenerated(MetadataReader metadata, TypeDefinition type) =>
        type.GetCustomAttributes()
            .Select(metadata.GetCustomAttribute)
            .Any(attribute => AttributeTypeName(metadata, attribute) == typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute).FullName);

    private static string? AttributeTypeName(MetadataReader metadata, CustomAttribute attribute)
    {
        var parent = attribute.Constructor.Kind switch
        {
            HandleKind.MemberReference => metadata.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent,
            HandleKind.MethodDefinition => metadata.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).GetDeclaringType(),
            _ => default(EntityHandle),
        };

        return parent.Kind switch
        {
            HandleKind.TypeReference => metadata.GetTypeReference((TypeReferenceHandle)parent) is var reference
                ? $"{metadata.GetString(reference.Namespace)}.{metadata.GetString(reference.Name)}"
                : null,
            HandleKind.TypeDefinition => metadata.GetTypeDefinition((TypeDefinitionHandle)parent) is var definition
                ? $"{metadata.GetString(definition.Namespace)}.{metadata.GetString(definition.Name)}"
                : null,
            _ => null,
        };
    }

    private const string Traceability = "9. Traceability Matrix";

    private static readonly string[] Vocabulary =
    [
        "SPEC001", "SPEC002", "SPEC003", "SPEC004", "SPEC010", "SPEC011", "SPEC012", "SPEC013", "SPEC020", "SPEC021", "SPEC030",
        "SPEC031", "SPEC040", "SPEC041", "SPEC043", "SPEC044", "SPEC050", "SPEC051", "SPEC052", "SPEC060", "SPEC061",
    ];

    private static readonly IReadOnlyList<string> WithoutTraceability =
        [.. SpecTree.Sections.Where(static section => !section.StartsWith($"## {Traceability}\n", StringComparison.Ordinal))];

    private readonly List<string> _specifications = [];
    private SpecTree? _tree;
    private SpechtReport? _report;
    private IReadOnlyList<GoldenReport.Verdict>? _golden;
    private string? _library;
    private string? _assemblyName;
    private string[] _namespaces = [];
    private string? _identity;
}
