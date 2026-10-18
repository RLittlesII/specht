using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using Specht.Analysis.Tests.Policy;
using Specht.Codefixes.Documentation;

namespace Specht.Analysis.Tests.Documentation;

[Trait("Tier", "Unit")]
public sealed class DocumentationCodeFixUnitTests
{
    [Fact]
    public async Task ClaimOnlyBinding_WhenMaterialized_ShouldPersistIdentityAndPreserveOtherXml()
    {
        // Given
        var source = "/// <spec feature=\"cap:read\" />\npublic class Example {\n"
            + "    /// <summary>Keep &amp; preserve.</summary>\n    /// <custom data=\"x\" />\n"
            + "    /// <implements claim=\"need/7\" />\n    public void Run() {}\n}";
        var harness = new RepairWorkspace();
        var project = await harness.Create(("test/Example.cs", source));
        var diagnostics = await harness.Diagnostics(project);
        var diagnostic = diagnostics.Single(static diagnostic => diagnostic.Id == "SPCT0002");

        // When
        var actions = await harness.Actions(project, diagnostic);
        var action = actions.Single(static action => action.EquivalenceKey == "Specht.MaterializeDefault");
        var fixedProject = (await harness.Apply(action)).GetProject(project.Id)!;

        // Then
        (await fixedProject.Documents.Single().GetTextAsync(TestContext.Current.CancellationToken)).ToString()
            .Should().Be(source.Replace("<implements claim=\"need/7\" />", "<implements feature=\"cap:read\" claim=\"need/7\" />"));
        (await harness.Diagnostics(fixedProject)).Where(static diagnostic => diagnostic.Id != "SPCT0007").Should().BeEmpty();
        (await project.Documents.Single().GetTextAsync(TestContext.Current.CancellationToken)).ToString().Should().Be(source);
    }

    [Theory]
    [ClassData(typeof(UnsafeRepairCases))]
    public async Task UnsafeBinding_WhenRepairsRequested_ShouldNeverGuessOrDestroyMaterial(string defaults, string xml)
    {
        // Given
        var source = "/// " + defaults + "\npublic class Example {\n/// " + xml + "\npublic void Run() {} }";
        var harness = new RepairWorkspace();
        var project = await harness.Create(("test/Example.cs", source));
        var diagnostic = (await harness.Diagnostics(project)).First(static diagnostic => diagnostic.Id != "SPCT0007");

        // When
        var actions = await harness.Actions(project, diagnostic);

        // Then
        actions.Should().OnlyContain(static action => action.EquivalenceKey!.StartsWith("Specht.Select:", StringComparison.Ordinal));
        if (xml.Contains("cliam", StringComparison.Ordinal) || xml.Contains("keep", StringComparison.Ordinal)
            || xml.Contains("custom", StringComparison.Ordinal) || !xml.EndsWith('>'))
        {
            actions.Should().BeEmpty();
        }

        (await project.Documents.Single().GetTextAsync(TestContext.Current.CancellationToken)).ToString().Should().Be(source);
    }

    [Theory]
    [ClassData(typeof(MissingBindingCases))]
    public async Task MissingBinding_WhenTargetSelected_ShouldUseOnlyAnExistingEligibleRecord(string source, string expected)
    {
        // Given
        var harness = new RepairWorkspace();
        var project = await harness.Create(("test/Example.cs", source));
        var diagnostic = (await harness.Diagnostics(project)).Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("SPCT0007");

        // When
        var actions = await harness.Actions(project, diagnostic);
        var action = actions.Should().ContainSingle().Subject;
        action.Title.Should().Contain("cap:read").And.Contain("need/7");
        var fixedProject = (await harness.Apply(action)).GetProject(project.Id)!;

        // Then
        (await fixedProject.Documents.Single().GetTextAsync(TestContext.Current.CancellationToken)).ToString()
            .Should().Be(expected);
        (await harness.Diagnostics(fixedProject)).Should().BeEmpty();
    }

    [Fact]
    public async Task PartialDuplicates_WhenRemoved_ShouldKeepOrdinalFirstAndPreserveOtherXml()
    {
        // Given
        var first = "/// <implements feature=\"cap:read\" claim=\"need/7\" />\npublic partial class Example {}";
        var second = "/// <summary>Keep</summary><implements claim=\"need/7\" feature=\"cap:read\" />\n"
            + "/// <custom />\npublic partial class Example {}";
        var harness = new RepairWorkspace();
        var project = await harness.Create(("src/Z.cs", second), ("src/A.cs", first));
        var diagnostic = (await harness.Diagnostics(project)).Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("SPCT0009");

        // When
        var action = (await harness.Actions(project, diagnostic)).Single(static action => action.EquivalenceKey == "Specht.RemoveDuplicates");
        var fixedProject = (await harness.Apply(action)).GetProject(project.Id)!;

        // Then
        (await fixedProject.Documents.Single(static document => document.FilePath == "src/A.cs")
            .GetTextAsync(TestContext.Current.CancellationToken)).ToString().Should().Be(first);
        (await fixedProject.Documents.Single(static document => document.FilePath == "src/Z.cs")
            .GetTextAsync(TestContext.Current.CancellationToken)).ToString()
            .Should().Be(second.Replace("<implements claim=\"need/7\" feature=\"cap:read\" />", ""));
        (await harness.Diagnostics(fixedProject)).Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(BlockedDefaultCases))]
    public async Task AmbiguousOrIneligibleDefault_WhenRepairRequested_ShouldWithholdMaterialization(string reason)
    {
        // Given
        var harness = new RepairWorkspace();
        var feature = harness.Inputs.Single(static input => input.Kind == "feature");
        if (reason == "duplicate feature")
        {
            harness.Inputs.Add(new AnalysisInputFixture().WithKind("feature").WithRelativePath("src/other/.spec/README.md").WithText(feature.Text!));
        }
        else
        {
            var text = reason == "duplicate claim" ? feature.Text + "\n| need/7 | Another declaration | Agreement | Active |\n"
                : reason == "withdrawn claim" ? feature.Text!.Replace("Active", "Withdrawn") : feature.Text!.Replace("draft", "superseded");
            harness.Inputs[harness.Inputs.IndexOf(feature)] = new AnalysisInputFixture().WithKind("feature")
                .WithRelativePath(feature.RelativePath).WithText(text);
        }

        var source = "/// <spec feature=\"cap:read\" />\npublic class Example {\n"
            + "/// <verifies claim=\"need/7\" />\npublic void Run() {} }";
        var project = await harness.Create(("test/Example.cs", source));
        var diagnostics = await harness.Diagnostics(project);

        // When
        var actions = await harness.Actions(project, diagnostics.Single(static diagnostic => diagnostic.Id == "SPCT0002"));

        // Then
        actions.Should().BeEmpty();
        (await project.Documents.Single().GetTextAsync(TestContext.Current.CancellationToken)).ToString().Should().Be(source);
    }

    [Theory]
    [ClassData(typeof(RelationshipReplacementCases))]
    public async Task ReplacementTarget_WhenExplicitlySelected_ShouldReplaceOnlyTheReservedElement(string xml, string replacement)
    {
        // Given
        var source = "public class Example {\n/// <summary>Keep</summary>" + xml + "\n"
            + "/// <custom /><remarks>Keep too</remarks>\npublic void Run() {} }";
        var harness = new RepairWorkspace();
        var project = await harness.Create(("test/Example.cs", source));
        var diagnostic = (await harness.Diagnostics(project)).Single(static diagnostic => diagnostic.Id == "SPCT0004");

        // When
        var actions = await harness.Actions(project, diagnostic);
        var action = actions.Should().ContainSingle().Subject;
        action.EquivalenceKey.Should().StartWith("Specht.Select:");
        var fixedProject = (await harness.Apply(action)).GetProject(project.Id)!;

        // Then
        (await fixedProject.Documents.Single().GetTextAsync(TestContext.Current.CancellationToken)).ToString()
            .Should().Be(source.Replace(xml, replacement));
        var remaining = await harness.Diagnostics(fixedProject);
        if (xml.StartsWith("<verifies", StringComparison.Ordinal))
        {
            remaining.Should().BeEmpty();
        }
        else
        {
            remaining.Should().ContainSingle().Which.Id.Should().Be("SPCT0007");
        }
        var provider = new DocumentationCodeFixProvider();
        var context = new FixAllContext(project.Documents.Single(), provider, FixAllScope.Solution, action.EquivalenceKey,
            provider.FixableDiagnosticIds, new RepairDiagnostics(), TestContext.Current.CancellationToken);
        (await provider.GetFixAllProvider().GetFixAsync(context)).Should().BeNull();
    }

    [Fact]
    public async Task StaleAction_WhenWorkspaceChanged_ShouldPreserveTheCurrentDocuments()
    {
        // Given
        var source = "/// <spec feature=\"cap:read\" />\npublic class Example {\n"
            + "/// <verifies claim=\"need/7\" />\npublic void Run() {} }";
        var harness = new RepairWorkspace();
        var project = await harness.Create(("test/Example.cs", source));
        var action = (await harness.Actions(project, (await harness.Diagnostics(project))
            .Single(static diagnostic => diagnostic.Id == "SPCT0002"))).Single(static action => action.EquivalenceKey == "Specht.MaterializeDefault");
        var currentSource = source.Replace("cap:read", "cap:other");
        var current = project.Solution.WithDocumentText(project.DocumentIds.Single(), SourceText.From(currentSource));
        project.Solution.Workspace.TryApplyChanges(current).Should().BeTrue();

        // When
        var fixedProject = (await harness.Apply(action)).GetProject(project.Id)!;

        // Then
        (await fixedProject.Documents.Single().GetTextAsync(TestContext.Current.CancellationToken)).ToString().Should().Be(currentSource);
    }

    [Theory]
    [ClassData(typeof(BulkRepairCases))]
    public async Task MixedBindings_WhenFixAllInvoked_ShouldRecheckEachBindingAndRespectScope(string key, FixAllScope scope)
    {
        // Given
        var xml = key == "Specht.MaterializeDefault" ? "<implements claim=\"need/7\" />"
            : "<implements feature=\"cap:read\" claim=\"need/7\" /><implements feature=\"cap:read\" claim=\"need/7\" />";
        var source = "/// <spec feature=\"cap:read\" />\npublic class Example {\n/// " + xml + "\npublic void Run() {}\n"
            + "/// " + xml + "\npublic void Second() {}\n/// <implements claim=\"need/999\" />\npublic void Unrelated() {} }";
        var other = source.Replace("Example", "Other").Replace("Run", "Third");
        var harness = new RepairWorkspace();
        var project = await harness.Create(("test/Example.cs", source), ("test/Other.cs", other));
        var provider = new DocumentationCodeFixProvider();
        var document = project.Documents.Single(static document => document.FilePath == "test/Example.cs");
        var context = new FixAllContext(document, provider, scope, key, provider.FixableDiagnosticIds,
            new RepairDiagnostics(), TestContext.Current.CancellationToken);

        // When
        var action = await provider.GetFixAllProvider().GetFixAsync(context);
        action.Should().NotBeNull();
        var fixedProject = (await harness.Apply(action!)).GetProject(project.Id)!;

        // Then
        var replacement = "<implements feature=\"cap:read\" claim=\"need/7\" />";
        foreach (var fixedDocument in fixedProject.Documents)
        {
            var before = fixedDocument.FilePath == "test/Example.cs" ? source : other;
            var expected = scope == FixAllScope.Document && fixedDocument.FilePath == "test/Other.cs" ? before : before.Replace(xml, replacement);
            (await fixedDocument.GetTextAsync(TestContext.Current.CancellationToken)).ToString().Should().Be(expected);
        }

        var findings = await harness.Diagnostics(fixedProject);
        findings.Where(static diagnostic => diagnostic.Location.SourceTree!.FilePath == "test/Example.cs")
            .Should().OnlyContain(static diagnostic => diagnostic.Id == "SPCT0002" || diagnostic.Id == "SPCT0007");
        findings.Should().Contain(static diagnostic => diagnostic.Id == "SPCT0002");
    }
}
