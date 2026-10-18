using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.Bindings;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Tests.Policy;

[Trait("Tier", "Unit")]
public sealed class AnalysisInputReaderUnitTests
{
    [Theory]
    [ClassData(typeof(InvalidPolicyValues))]
    public void InvalidPolicy_WhenRead_ShouldReportUnavailableAnalysis(string field, string value)
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!;
        configuration["codeAnalysis"]![field] = JsonNode.Parse(value);
        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().BeFalse();
        snapshot.Failures.Should().ContainSingle();
        snapshot.Catalog.Should().BeNull();
    }

    [Fact]
    public void MissingSuppliedSchema_WhenRead_ShouldFailInsteadOfUsingAnEmbeddedSchema()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        inputs.RemoveAll(static input => input.Kind == "schema:feature");

        // When
        var result = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        result.Available.Should().BeFalse();
        result.Failures.Should().ContainSingle().Which.RelativePath.Should().Be("src/read/.spec/README.md");
    }

    [Fact]
    public void ChangedClaim_WhenReadAgain_ShouldUseNewContentsAndRetainTheOriginalCatalog()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var reader = new AnalysisInputReader();
        var before = reader.Read(inputs, TestContext.Current.CancellationToken);
        var index = inputs.FindIndex(static input => input.Kind == "feature");
        inputs[index] = inputs[index] with { Text = inputs[index].Text!.Replace("need/7", "need/8") };

        // When
        var after = reader.Read(inputs, TestContext.Current.CancellationToken);

        // Then
        after.Available.Should().BeTrue();
        var oldClaim = new ClaimReference(null, "cap:read", "need/7");
        new BindingResolver(before.Catalog!, before.Policy!.Bindings).Resolve(oldClaim).Status.Should().Be(BindingResolution.Resolved);
        new BindingResolver(after.Catalog!, after.Policy!.Bindings).Resolve(oldClaim).Status.Should().Be(BindingResolution.Unresolved);
        new BindingResolver(after.Catalog!, after.Policy.Bindings).Resolve(new ClaimReference(null, "cap:read", "need/8"))
            .Status.Should().Be(BindingResolution.Resolved);
    }

    [Fact]
    public void ManifestSectionMappings_WhenSupplied_ShouldReadExistingMetadataWithoutChangingItsSchemaVersion()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!;
        var mappings = inputs.Single(static input => input.Kind == "mappings");
        configuration["codeAnalysis"]!["mappings"] = JsonNode.Parse(mappings.Text!);
        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };
        inputs.Remove(mappings);

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().BeTrue();
        snapshot.Policy!.Bindings.SchemaVersion.Should().Be("0.1.0");
        snapshot.Policy.Bindings.Identifiers["claim"].Should().Be("^need/[0-9]+$");
    }

    [Theory]
    [InlineData("0.1.0")]
    [InlineData("0.2.0")]
    [InlineData("7.4.2")]
    [InlineData("consumer-contract-r7")]
    public void ConsumerSchemaVersion_WhenSupplied_ShouldPreserveItWithoutRequiringASpechtPin(string version)
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var original = inputs[0].Text!.Replace("0.1.0", version);
        inputs[0] = inputs[0] with { Text = original };

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().BeTrue();
        snapshot.Policy!.Bindings.SchemaVersion.Should().Be(version);
        inputs[0].Text.Should().Be(original);
        new BindingResolver(snapshot.Catalog!, snapshot.Policy.Bindings).Resolve(new ClaimReference(null, "cap:read", "need/7"))
            .IsEligible.Should().BeTrue();
    }

    [Fact]
    public void SuppliedRecords_WhenRead_ShouldUseActualIdentityAndPreserveSelectedScope()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Failures.Should().BeEmpty();
        snapshot.Available.Should().BeTrue();
        snapshot.Policy!.Projects.Should().ContainSingle().Which.Path.Should().Be("test/tests.csproj");
        snapshot.Policy.Features.Should().ContainSingle().Which.Feature.Should().Be("cap:read");
        var resolver = new BindingResolver(snapshot.Catalog!, snapshot.Policy.Bindings);
        resolver.Resolve(new ClaimReference(null, "cap:read", "need/7")).IsEligible.Should().BeTrue();
        resolver.Resolve(new AdrReference(null, null, "ADR-0012")).IsEligible.Should().BeTrue();
        resolver.Resolve(new ClaimReference(null, "cap:read", "need/8")).Status.Should().Be(BindingResolution.Unresolved);
    }

    [Theory]
    [ClassData(typeof(AuthoringSelections))]
    public void DeclarationFacts_WhenSelected_ShouldSeparateValidationFromRequiredAuthoring(
        string project, string framework, string path, bool generated, bool recognized, bool validate, bool require)
    {
        // Given
        var snapshot = new AnalysisInputReader().Read(new AnalysisInputs().Inputs, TestContext.Current.CancellationToken);
        AnalysisDeclaration declaration = new AnalysisDeclarationFixture().WithProject(project).WithTargetFramework(framework)
            .WithRelativePath(path).WithSymbolId("M:Read.Run").WithGenerated(generated).WithRecognizedTest(recognized);

        // When
        var result = snapshot.Policy!.Select(declaration);

        // Then
        result.ValidateExplicit.Should().Be(validate);
        result.RequiredRoles.Should().BeEquivalentTo(require ? new[] { "verifies" } : Array.Empty<string>());
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("invalid")]
    [InlineData("incompatible")]
    public void RequiredInputDefects_WhenRead_ShouldMakeAnalysisUnavailable(string defect)
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        if (defect == "missing")
        {
            inputs.RemoveAll(static input => input.Kind == "feature");
        }
        else if (defect == "invalid")
        {
            inputs[0] = inputs[0] with { Text = "{" };
        }
        else
        {
            var index = inputs.FindIndex(static input => input.Kind == "mappings");
            inputs[index] = inputs[index] with { Text = "{\"/policy\":\"/absent\"}" };
        }

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().BeFalse();
        snapshot.Failures.Should().NotBeEmpty();
        snapshot.Catalog.Should().BeNull();
    }

    [Theory]
    [InlineData("source")]
    [InlineData("feature")]
    [InlineData("configuration")]
    [InlineData("schema:feature")]
    [InlineData("mappings")]
    public void ChangedInput_WhenReadAgain_ShouldInvalidateContentIdentityAndRetainOriginalSnapshot(string kind)
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var reader = new AnalysisInputReader();
        var before = reader.Read(inputs, TestContext.Current.CancellationToken);
        var index = inputs.FindIndex(input => input.Kind == kind);
        inputs[index] = inputs[index] with { Text = inputs[index].Text + "\n" };

        // When
        var after = reader.Read(inputs, TestContext.Current.CancellationToken);

        // Then
        after.Available.Should().BeTrue();
        after.ContentKey.Should().NotBe(before.ContentKey);
        reader.Read(inputs.AsEnumerable().Reverse().ToArray(), TestContext.Current.CancellationToken).ContentKey.Should().Be(after.ContentKey);
        before.Catalog!.Features.Single().Claims.Single().Id.Should().Be("need/7");
    }

    [Fact]
    public void ReasonedExemption_WhenSelected_ShouldRemainVisibleWithoutCreatingRequiredLinks()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var manifest = JsonNode.Parse(inputs[0].Text!)!;
        manifest["codeAnalysis"]!["exemptions"] = JsonNode.Parse("""
            [{"target":{"feature":"cap:read","claim":"need/7"},"links":["test"],"reason":"External verification"}]
            """);
        inputs[0] = inputs[0] with { Text = manifest.ToJsonString() };
        var claim = new ClaimReference(null, "cap:read", "need/7");

        // When
        var policy = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken).Policy!;

        // Then
        policy.ExemptionReasons(claim).Should().Equal("External verification");
        policy.RequiredLinks(claim).Should().BeEmpty();
        policy.RequiredLinks(new ClaimReference(null, "cap:read", "need/8")).Should().Equal("test");
    }

    [Fact]
    public void ConfiguredColumns_WhenRead_ShouldUseTheirNamesInsteadOfColumnPositions()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        inputs[0] = inputs[0] with { Text = inputs[0].Text!.Replace("\"records\":{", "\"records\":{\"claimStatusColumn\":\"State\",") };
        var index = inputs.FindIndex(static input => input.Kind == "feature");
        inputs[index] = inputs[index] with { Text = """
            ---
            id: cap:read
            spec_status: draft
            ---
            ## 3. Acceptance Criteria
            | Claim | State | ID |
            | --- | --- | --- |
            | A need | Withdrawn | need/7 |
            """ };

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().BeTrue();
        new BindingResolver(snapshot.Catalog!, snapshot.Policy!.Bindings).Resolve(new ClaimReference(null, "cap:read", "need/7"))
            .IsEligible.Should().BeFalse();
    }
}
