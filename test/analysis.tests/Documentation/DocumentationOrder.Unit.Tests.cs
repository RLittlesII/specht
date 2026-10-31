using AwesomeAssertions;
using Microsoft.CodeAnalysis.CodeFixes;
using Specht.Codefixes.Documentation;

namespace Specht.Analysis.Tests.Documentation;

[Trait("Tier", "Unit")]
public sealed class DocumentationOrderUnitTests
{
    [Theory]
    [ClassData(typeof(OrderedReplacementCases))]
    public async Task SelectedReplacement_WhenWrittenAmongSameRoleBindings_ShouldTakeItsSortedPosition(string existing, string defective,
        string replacement)
    {
        // Given
        var source = "public class Example {\n    /// <summary>Keep</summary>\n    /// " + existing + "\n    /// " + defective + "\n"
            + "    public void Run() {}\n}";
        var harness = new RepairWorkspace();
        harness.Inputs.Clear();
        harness.Inputs.AddRange(new OrderedRecordInputs().Inputs);
        var project = await harness.Create(("test/Example.cs", source));
        var diagnostic = (await harness.Diagnostics(project)).Single(static diagnostic => diagnostic.Id == "SPCT0004");

        // When
        var action = (await harness.Actions(project, diagnostic))
            .Single(action => action.EquivalenceKey!.EndsWith(":" + replacement, StringComparison.Ordinal));
        var fixedProject = (await harness.Apply(action)).GetProject(project.Id)!;

        // Then
        (await fixedProject.Documents.Single().GetTextAsync(TestContext.Current.CancellationToken)).ToString().Should().Be(
            "public class Example {\n    /// <summary>Keep</summary>\n    /// " + replacement + "\n    /// " + existing + "\n"
            + "    public void Run() {}\n}");
    }

    [Fact]
    public async Task ClaimOnlyBindings_WhenFixAllMaterializes_ShouldEndInClaimOrder()
    {
        // Given
        var source = "/// <spec epic=\"0001\" feature=\"F2\" />\npublic class Example {\n"
            + "    /// <implements claim=\"B-010\" />\n    /// <implements claim=\"B-002\" />\n    public void Run() {}\n}";
        var harness = new RepairWorkspace();
        harness.Inputs.Clear();
        harness.Inputs.AddRange(new OrderedRecordInputs().Inputs);
        var project = await harness.Create(("test/Example.cs", source));
        var provider = new DocumentationCodeFixProvider();
        var context = new FixAllContext(project.Documents.Single(), provider, FixAllScope.Document, "Specht.MaterializeDefault",
            provider.FixableDiagnosticIds, new RepairDiagnostics(), TestContext.Current.CancellationToken);

        // When
        var action = await provider.GetFixAllProvider().GetFixAsync(context);
        action.Should().NotBeNull();
        var fixedProject = (await harness.Apply(action!)).GetProject(project.Id)!;

        // Then
        (await fixedProject.Documents.Single().GetTextAsync(TestContext.Current.CancellationToken)).ToString().Should().Be(
            "/// <spec epic=\"0001\" feature=\"F2\" />\npublic class Example {\n"
            + "    /// <implements epic=\"0001\" feature=\"F2\" claim=\"B-002\" />\n"
            + "    /// <implements epic=\"0001\" feature=\"F2\" claim=\"B-010\" />\n    public void Run() {}\n}");
    }

    [Fact]
    public async Task RepairedComment_WhenOrdered_ShouldLeaveOtherContentRolesAndCommentsInPlace()
    {
        // Given
        var untouched = "    /// <implements epic=\"0201\" feature=\"F2\" claim=\"B-001\" />\n"
            + "    /// <implements epic=\"0001\" feature=\"F2\" claim=\"B-001\" />\n    public void Other() {}\n}";
        var source = "public class Example {\n    /// <summary>Keep</summary>\n"
            + "    /// <verifies epic=\"0201\" feature=\"F2\" claim=\"B-001\" />\n"
            + "    /// <implements epic=\"0001\" feature=\"F2\" claim=\"B-002\" />\n"
            + "    /// <remarks>Keep</remarks><verifies epic=\"0001\" feature=\"F10\" claim=\"B-001\" />\n"
            + "    /// <verifies epic=\"0001\" feature=\"F2\" claim=\"B-999\" />\n    public void Run() {}\n\n" + untouched;
        var harness = new RepairWorkspace();
        harness.Inputs.Clear();
        harness.Inputs.AddRange(new OrderedRecordInputs().Inputs);
        var project = await harness.Create(("test/Example.cs", source));
        var diagnostic = (await harness.Diagnostics(project)).Single(static diagnostic => diagnostic.Id == "SPCT0004");

        // When
        var action = (await harness.Actions(project, diagnostic)).Single(static action =>
            action.EquivalenceKey == "Specht.Select:verifies:<verifies epic=\"0001\" feature=\"F2\" claim=\"B-001\" />");
        var fixedProject = (await harness.Apply(action)).GetProject(project.Id)!;

        // Then
        (await fixedProject.Documents.Single().GetTextAsync(TestContext.Current.CancellationToken)).ToString().Should().Be(
            "public class Example {\n    /// <summary>Keep</summary>\n"
            + "    /// <verifies epic=\"0001\" feature=\"F2\" claim=\"B-001\" />\n"
            + "    /// <implements epic=\"0001\" feature=\"F2\" claim=\"B-002\" />\n"
            + "    /// <remarks>Keep</remarks><verifies epic=\"0001\" feature=\"F10\" claim=\"B-001\" />\n"
            + "    /// <verifies epic=\"0201\" feature=\"F2\" claim=\"B-001\" />\n    public void Run() {}\n\n" + untouched);
    }

    [Fact]
    public async Task DuplicateRemoval_WhenInvoked_ShouldReorderNothing()
    {
        // Given
        var kept = "public class Example {\n    /// <implements epic=\"0201\" feature=\"F2\" claim=\"B-001\" />\n"
            + "    /// <implements epic=\"0001\" feature=\"F2\" claim=\"B-001\" />\n    /// ";
        var source = kept + "<implements epic=\"0001\" feature=\"F2\" claim=\"B-001\" />\n    public void Run() {}\n}";
        var harness = new RepairWorkspace();
        harness.Inputs.Clear();
        harness.Inputs.AddRange(new OrderedRecordInputs().Inputs);
        var project = await harness.Create(("test/Example.cs", source));
        var diagnostic = (await harness.Diagnostics(project)).Single(static diagnostic => diagnostic.Id == "SPCT0009");

        // When
        var action = (await harness.Actions(project, diagnostic)).Single(static action => action.EquivalenceKey == "Specht.RemoveDuplicates");
        var fixedProject = (await harness.Apply(action)).GetProject(project.Id)!;

        // Then
        (await fixedProject.Documents.Single().GetTextAsync(TestContext.Current.CancellationToken)).ToString()
            .Should().Be(kept + "\n    public void Run() {}\n}");
    }

    [Fact]
    public async Task TargetChoices_WhenListed_ShouldFollowEpicFeatureAndClaimOrder()
    {
        // Given
        var harness = new RepairWorkspace();
        harness.Inputs.Clear();
        harness.Inputs.AddRange(new OrderedRecordInputs().Inputs);
        var project = await harness.Create(("test/Example.cs", "public class Example {\n    public void Run() {}\n}"));
        var diagnostic = (await harness.Diagnostics(project)).Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("SPCT0007");

        // When
        var actions = await harness.Actions(project, diagnostic);

        // Then
        actions.Select(static action => action.Title).Should().Equal(
            "Bind verifies to 0001 / F2 / B-001",
            "Bind verifies to 0001 / F2 / B-002",
            "Bind verifies to 0001 / F2 / B-010",
            "Bind verifies to 0001 / F10 / B-001",
            "Bind verifies to 0201 / F2 / B-001");
    }
}
