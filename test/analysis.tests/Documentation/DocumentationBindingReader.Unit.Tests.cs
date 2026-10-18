using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Specht.Analysis.Bindings;
using Specht.Analysis.Documentation;
using Specht.Analysis.Policy;
using Specht.Analysis.Tests.Policy;

namespace Specht.Analysis.Tests.Documentation;

[Trait("Tier", "Unit")]
public sealed class DocumentationBindingReaderUnitTests
{
    [Theory]
    [ClassData(typeof(DistinctRelationshipCases))]
    public void DistinctTargets_WhenRepeated_ShouldRetainEachCompleteRelationship(string role)
    {
        // Given
        var xml = role == "follows" ? "<follows adr=\"ADR-0012\" />\n/// <follows adr=\"ADR-0013\" />"
            : "<" + role + " feature=\"cap:read\" claim=\"need/7\" />\n/// <"
                + role + " feature=\"cap:read\" claim=\"need/8\" />";
        var tree = CSharpSyntaxTree.ParseText("public class Example\n{\n/// " + xml + "\npublic void Run() {}\n}",
            path: "src/Example.cs", cancellationToken: TestContext.Current.CancellationToken);
        var compilation = CSharpCompilation.Create("Example", [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var snapshot = new AnalysisInputReader().Read(new AnalysisInputs().Inputs, TestContext.Current.CancellationToken);
        var owner = snapshot.Catalog!.Features.Single();
        var catalog = new BindingCatalog([new BindingFeature(owner.Identity, owner.State,
            [new BindingClaim("need/7", "Active"), new BindingClaim("need/8", "Active")], owner.RelativePath)],
            [new BindingAdr("ADR-0012", null, "accepted", ".spec/adr/0012.md"),
                new BindingAdr("ADR-0013", null, "accepted", ".spec/adr/0013.md")]);

        // When
        var result = new DocumentationBindingReader().Read(compilation.GetTypeByMetadataName("Example")!.GetMembers("Run").Single(),
            new BindingResolver(catalog, snapshot.Policy!.Bindings), role == "verifies", TestContext.Current.CancellationToken);

        // Then
        result.Relationships.Should().HaveCount(2);
        result.Relationships.Select(static binding => binding.Resolution!.Target!.Id).Distinct().Should().HaveCount(2);
        result.Findings.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(ScalarIdentifierCases))]
    public void ScalarAttribute_WhenRead_ShouldValidateTheEntireConsumerIdentifier(string value, bool valid)
    {
        // Given
        var xml = "<implements feature=\"cap:read\" claim=\"" + value + "\" />";
        var tree = CSharpSyntaxTree.ParseText("public class Example\n{\n/// " + xml + "\npublic void Run() {}\n}",
            path: "src/Example.cs", cancellationToken: TestContext.Current.CancellationToken);
        var compilation = CSharpCompilation.Create("Example", [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var snapshot = new AnalysisInputReader().Read(new AnalysisInputs().Inputs, TestContext.Current.CancellationToken);
        var owner = snapshot.Catalog!.Features.Single();
        var catalog = new BindingCatalog([new BindingFeature(owner.Identity, owner.State,
            [new BindingClaim("need/7,need/8", "Active")], owner.RelativePath)], []);
        var identifiers = snapshot.Policy!.Bindings.Identifiers.ToDictionary(static entry => entry.Key, static entry => entry.Value);
        identifiers["claim"] = "need/[0-9]+(?:,need/[0-9]+)?";
        var policy = new BindingPolicy("0.1.0", identifiers, snapshot.Policy.Bindings.Markers, false,
            snapshot.Policy.Bindings.ClaimsSection, snapshot.Policy.Bindings.Omissions);

        // When
        var result = new DocumentationBindingReader().Read(compilation.GetTypeByMetadataName("Example")!.GetMembers("Run").Single(),
            new BindingResolver(catalog, policy), cancellationToken: TestContext.Current.CancellationToken);

        // Then
        result.Elements.Should().ContainSingle().Which.Attributes["claim"].Should().Be(value);
        result.Relationships.Should().HaveCount(valid ? 1 : 0);
        result.Findings.Select(static finding => finding.Id).Should().Equal(valid ? Array.Empty<string>() : new[] { "SPCT0003" });
    }

    [Theory]
    [ClassData(typeof(DocumentedDeclarations))]
    public void DocumentableDeclaration_WhenRead_ShouldBindItsOwnSymbol(string source, string symbolId, string role, bool testMethod)
    {
        // Given
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(documentationMode: DocumentationMode.Diagnose),
            "src/Example.cs", cancellationToken: TestContext.Current.CancellationToken);
        var compilation = CSharpCompilation.Create("Example", [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var symbol = DocumentationCommentId.GetFirstSymbolForDeclarationId(symbolId, compilation)!;
        var snapshot = new AnalysisInputReader().Read(new AnalysisInputs().Inputs, TestContext.Current.CancellationToken);

        // When
        var result = new DocumentationBindingReader().Read(symbol, new BindingResolver(snapshot.Catalog!, snapshot.Policy!.Bindings),
            testMethod, TestContext.Current.CancellationToken);

        // Then
        compilation.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        result.Findings.Should().BeEmpty();
        var element = result.Elements.Should().ContainSingle().Subject;
        element.ShapeValid.Should().BeTrue();
        element.Role.Should().Be(role);
        element.SymbolId.Should().Be(symbolId);
        if (role == "spec")
        {
            result.Relationships.Should().BeEmpty();
            element.Resolution.Should().BeNull();
        }
        else
        {
            result.Relationships.Should().ContainSingle().Which.IsEvidence.Should().BeTrue();
        }
    }

    [Fact]
    public void RepeatedPartialRelationships_WhenRead_ShouldDeduplicateBySymbolRoleAndResolvedTarget()
    {
        // Given
        var source = """
            /// <implements feature="cap:read" claim="need/7" />
            public partial class Example { }
            /// <implements claim="need/7" feature="cap:read" />
            /// <follows adr="ADR-0012" />
            public partial class Example
            {
                /// <verifies feature="cap:read" claim="need/7" />
                /// <implements feature="cap:read" claim="need/7" />
                public void Run() { }
                /// <verifies feature="cap:read" claim="need/7" />
                public void Other() { }
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(source, path: "src/Example.cs", cancellationToken: TestContext.Current.CancellationToken);
        var compilation = CSharpCompilation.Create("Example", [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var type = compilation.GetTypeByMetadataName("Example")!;
        var snapshot = new AnalysisInputReader().Read(new AnalysisInputs().Inputs, TestContext.Current.CancellationToken);
        var reader = new DocumentationBindingReader();
        var resolver = new BindingResolver(snapshot.Catalog!, snapshot.Policy!.Bindings);

        // When
        var typeResult = reader.Read(type, resolver, cancellationToken: TestContext.Current.CancellationToken);
        var methodResult = reader.Read(type.GetMembers("Run").Single(), resolver, true, TestContext.Current.CancellationToken);
        var otherResult = reader.Read(type.GetMembers("Other").Single(), resolver, true, TestContext.Current.CancellationToken);

        // Then
        typeResult.Elements.Should().HaveCount(3);
        typeResult.Relationships.Should().HaveCount(2);
        typeResult.Findings.Should().ContainSingle().Which.Id.Should().Be("SPCT0009");
        methodResult.Relationships.Select(static binding => binding.Role).Should().BeEquivalentTo("verifies", "implements");
        otherResult.Relationships.Should().ContainSingle();
        otherResult.Relationships[0].SymbolId.Should().NotBe(methodResult.Relationships[0].SymbolId);
    }

    [Fact]
    public void TypeBindingsAndDefaults_WhenReadingMembers_ShouldNeverSupplyImplicitIdentity()
    {
        // Given
        var source = """
            /// <spec feature="cap:read" />
            /// <implements feature="cap:read" claim="need/7" />
            public class Example
            {
                /// <implements claim="need/7" />
                public void Incomplete() { }
                public void Missing() { }
                /// <implements feature="cap:read" claim="need/7" />
                public void Explicit() { }
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(source, path: "src/Example.cs", cancellationToken: TestContext.Current.CancellationToken);
        var compilation = CSharpCompilation.Create("Example", [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var type = compilation.GetTypeByMetadataName("Example")!;
        var snapshot = new AnalysisInputReader().Read(new AnalysisInputs().Inputs, TestContext.Current.CancellationToken);
        var reader = new DocumentationBindingReader();
        var resolver = new BindingResolver(snapshot.Catalog!, snapshot.Policy!.Bindings);

        // When
        var incomplete = reader.Read(type.GetMembers("Incomplete").Single(), resolver, cancellationToken: TestContext.Current.CancellationToken);
        var missing = reader.Read(type.GetMembers("Missing").Single(), resolver, cancellationToken: TestContext.Current.CancellationToken);
        var complete = reader.Read(type.GetMembers("Explicit").Single(), resolver, cancellationToken: TestContext.Current.CancellationToken);

        // Then
        incomplete.Relationships.Should().BeEmpty();
        incomplete.Findings.Should().ContainSingle().Which.Id.Should().Be("SPCT0002");
        missing.Elements.Should().BeEmpty();
        missing.Relationships.Should().BeEmpty();
        complete.Relationships.Should().ContainSingle().Which.Resolution!.Target!.Id.Should().Be("need/7");
        tree.ToString().Should().Be(source);
    }

    [Theory]
    [ClassData(typeof(XmlResolutionCases))]
    public void TargetVerdict_WhenRead_ShouldRemainSeparateFromValidXmlShape(string claimState, int copies, string expected)
    {
        // Given
        var tree = CSharpSyntaxTree.ParseText("""
            public class Example
            {
                /// <implements feature="cap:read" claim="need/7" />
                public void Run() { }
            }
            """, path: "src/Example.cs", cancellationToken: TestContext.Current.CancellationToken);
        var compilation = CSharpCompilation.Create("Example", [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var snapshot = new AnalysisInputReader().Read(new AnalysisInputs().Inputs, TestContext.Current.CancellationToken);
        var feature = snapshot.Catalog!.Features.Single();
        var catalog = new BindingCatalog(Enumerable.Range(0, copies).Select(_ => new BindingFeature(feature.Identity, feature.State,
            [new BindingClaim("need/7", claimState)], feature.RelativePath)).ToArray(), []);

        // When
        var result = new DocumentationBindingReader().Read(compilation.GetTypeByMetadataName("Example")!.GetMembers("Run").Single(),
            new BindingResolver(catalog, snapshot.Policy!.Bindings), cancellationToken: TestContext.Current.CancellationToken);

        // Then
        result.Elements.Should().ContainSingle().Which.ShapeValid.Should().BeTrue();
        result.Relationships.Should().BeEmpty();
        result.Findings.Should().ContainSingle().Which.Id.Should().Be(expected);
    }
}
