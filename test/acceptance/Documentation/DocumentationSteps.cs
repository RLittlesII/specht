using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Reqnroll;
using Specht.Analysis.Bindings;
using Specht.Analysis.Documentation;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Acceptance.Documentation;

[Binding]
[Scope(Tag = "0201-F2")]
public sealed class DocumentationSteps
{
    [Given("a supported declaration carrying a complete explicit Feature and claim binding")]
    public void GivenExplicit() => _typeXml = "<spec feature=\"F2\" />";

    [Given("a member carrying only a claim under a type default")]
    public void GivenClaimOnly()
    {
        _typeXml = "<spec feature=\"F1\" />";
        _xml = "<implements claim=\"B-001\" />";
    }

    [Given("missing, conflicting or ambiguous default targets")]
    public void GivenUnsafeDefault() => _xml = "<implements claim=\"B-001\" />";

    [Given("a malformed, unresolved or ambiguous reference on a supported declaration")]
    public void GivenUnresolved() => _xml = "<implements feature=\"F1\" claim=\"B-999\" />";

    [Given("a supported declaration with two repeated {string} elements")]
    public void GivenRepeated(string role)
    {
        _recognized = role == "verifies";
        _xml = role == "follows" ? "<follows adr=\"ADR-0012\" />\n    /// <follows adr=\"ADR-0013\" />"
            : "<" + role + " feature=\"F1\" claim=\"B-001\" />\n    /// <" + role + " feature=\"F1\" claim=\"B-002\" />";
    }

    [Given("each element carries one complete distinct target identity")]
    public void GivenCompleteTargets() => _expectedCount = 2;

    [Given("a supported declaration with a test verification binding")]
    public void GivenVerification()
    {
        _recognized = true;
        _xml = "<verifies feature=\"F1\" claim=\"B-001\" />";
    }

    [Given("a supported declaration with an implementation binding")]
    public void GivenImplementation() => _xml = "<implements feature=\"F1\" claim=\"B-001\" />";

    [Given("a supported declaration with an architectural-decision binding")]
    public void GivenAdr() => _xml = "<follows adr=\"ADR-0012\" />";

    [Given("a {string} binding on a {string}")]
    public void GivenPlacement(string role, string declaration)
    {
        if (role == "spec")
        {
            _xml = "<spec feature=\"F1\" />";
        }
        else if (role == "verifies")
        {
            GivenVerification();
        }
        else if (role == "follows")
        {
            GivenAdr();
        }

        _onType = declaration == "XML-documentable type";
    }

    [Given("a type with an explicit {string} binding")]
    public void GivenTypeBinding(string role)
    {
        _typeXml = role == "follows" ? "<follows adr=\"ADR-0012\" />" : "<implements feature=\"F1\" claim=\"B-001\" />";
        _xml = string.Empty;
    }

    [Given("a member of that type with no explicit binding")]
    public void GivenUnboundMember() => _xml = string.Empty;

    [Given("a relationship element with {string} in an identity attribute")]
    public void GivenScalar(string value)
    {
        _scalar = value == "a delimiter-bearing ID" ? "B-001,B-002" : "B-001 B-002";
        _claimGrammar = "^B-[0-9]{3}(,B-[0-9]{3})?$";
        _xml = "<implements feature=\"F1\" claim=\"" + _scalar + "\" />";
    }

    [Given("a reserved XML element with {string}")]
    public void GivenInvalidXml(string defect) => _xml = defect switch
    {
        "a missing required attribute" => "<implements claim=\"B-001\" />",
        "an unknown attribute" => "<implements feature=\"F1\" claim=\"B-001\" extra=\"x\" />",
        "body text" => "<implements feature=\"F1\" claim=\"B-001\">body</implements>",
        "a nested element" => "<implements feature=\"F1\" claim=\"B-001\"><other /></implements>",
        "malformed XML" => "<implements feature=\"F1\" claim=\"B-001\"",
        _ => "<spec feature=\"F1\" />",
    };

    [Given("an invalid {string} element")]
    public void GivenInvalidRole(string role)
    {
        _recognized = role == "verifies";
        _xml = "<" + role + " extra=\"invalid\" />";
    }

    [Given("an empty {string} element with its defined attributes on a supported declaration")]
    public void GivenEmptyRole(string role) => GivenPlacement(role, role == "spec" ? "XML-documentable type" : "recognized test method");

    [Given("valid relationship elements with the same code symbol, role and resolved target")]
    public void GivenDuplicates() => _xml += "\n    /// " + _xml;

    [Given("those elements appear in {string}")]
    public void GivenDuplicateLocation(string location)
    {
        if (location == "different parts of a partial type")
        {
            _partial = true;
            _onType = true;
        }
    }

    [Given("duplicate valid {string} elements on one code symbol")]
    public void GivenRoleDuplicates(string role)
    {
        GivenPlacement(role, "recognized test method");
        GivenDuplicates();
    }

    [Given("one recognized test method with valid verification and implementation relationships to the same claim")]
    public void GivenDistinctRoles()
    {
        GivenVerification();
        _xml += "\n    /// <implements feature=\"F1\" claim=\"B-001\" />";
    }

    [Given("two different test methods each with a valid verification relationship to the same claim")]
    public void GivenDistinctSymbols()
    {
        GivenVerification();
        _secondMethod = true;
    }

    [Given("an analyzer finding")]
    public void GivenFinding() => GivenClaimOnly();

    [Given("a published diagnostic and a later analyzer release")]
    public void GivenPublished() => _prior = new DiagnosticPublication("SPCT0004", "Absent");

    [Given("a diagnostic ID retired by a previous analyzer release")]
    public void GivenRetired() => _prior = new DiagnosticPublication("SPCT0004", "Absent", true);

    [Given("a repository pinned to a checker schema version")]
    public void GivenCheckerPin()
    {
        _pin = EngineServices.Versions().Versions[0];
        _checkerIds = _pin.RuleIds.OrderBy(static id => id, StringComparer.Ordinal).ToArray();
    }

    [When("the declared analysis is requested")]
    [When("its binding role is interpreted")]
    [When("its declaration scope is checked")]
    [When("checked traceability is requested")]
    [When("that attribute is interpreted under the consumer grammar")]
    [When("binding documentation is analyzed")]
    [When("the XML shape of the binding is validated")]
    [When("relationships are collected")]
    [When("its diagnostic is reported")]
    public void Analyze()
    {
        _source = _partial
            ? "/// <implements feature=\"F1\" claim=\"B-001\" />\npublic partial class Example {}\n"
                + "/// <implements claim=\"B-001\" feature=\"F1\" />\npublic partial class Example {}"
            : (_typeXml.Length > 0 ? "/// " + _typeXml + "\n" : string.Empty)
                + (_onType ? "/// " + _xml + "\n" : string.Empty)
                + "public class Example\n{\n" + (!_onType && _xml.Length > 0 ? "    /// " + _xml + "\n" : string.Empty)
                + "    public void Run() {}\n" + (_secondMethod ? "/// " + _xml + "\npublic void Other() {}\n" : string.Empty) + "}";
        var tree = CSharpSyntaxTree.ParseText(_source, path: "src/Example.cs");
        var compilation = CSharpCompilation.Create("Example", new[] { tree }, new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        var feature = new BindingFeature(new FeatureIdentity(null, "F1"), "draft",
            new[] { new BindingClaim("B-001", "Active"), new BindingClaim("B-002", "Active"), new BindingClaim("B-001,B-002", "Active") },
            "src/read/.spec/README.md");
        var catalog = new BindingCatalog(new[] { feature }, new[]
        {
            new BindingAdr("ADR-0012", null, "accepted", ".spec/adr/0012.md"),
            new BindingAdr("ADR-0013", null, "accepted", ".spec/adr/0013.md"),
        });
        var policy = new BindingPolicy("0.1.0", new Dictionary<string, string> { ["feature"] = "^F[0-9]+$", ["claim"] = _claimGrammar },
            new Dictionary<string, string>(), false, "3. Acceptance Criteria", new LifecycleOmissions(false, false, false));
        var type = compilation.GetTypeByMetadataName("Example")!;
        var reader = new DocumentationBindingReader();
        var resolver = new BindingResolver(catalog, policy);
        _type = reader.Read(type, resolver);
        _result = _onType ? _type : reader.Read(type.GetMembers("Run").Single(), resolver, _recognized);
        _other = _secondMethod ? reader.Read(type.GetMembers("Other").Single(), resolver, true) : null;
        tree.ToString().Should().Be(_source);
    }

    [When("the same diagnostic is reported in that later release")]
    public void CheckPublication() => _registry = new DocumentationDiagnostics(new[] { _prior! });

    [When("another diagnostic is introduced")]
    public void CheckRetirement()
    {
        Action publish = () => _ = new DocumentationDiagnostics(new[] { _prior! });
        publish.Should().Throw<ArgumentException>();
    }

    [When("an analyzer release adds an analyzer-only rule")]
    public void AddAnalyzerRule() => _registry = new DocumentationDiagnostics();

    [Then("checked traceability uses that declaration identity regardless of its containing type default")]
    public void ThenExplicit() => _result!.Relationships.Should().ContainSingle().Which.Resolution!.Target!.Feature.Should().Be("F1");

    [Then("analysis reports that the member lacks explicit identity and does not count it as checked evidence")]
    public void ThenIncomplete()
    {
        _result!.Relationships.Should().BeEmpty();
        _result.Findings.Should().ContainSingle().Which.Id.Should().Be("SPCT0002");
    }

    [Then("no repair silently selects a target")]
    public void ThenNoSilentRepair() => _result!.Relationships.Should().BeEmpty();

    [Then("analysis reports a diagnostic at that declaration with its target reference")]
    public void ThenDiagnostic()
    {
        var finding = _result!.Findings.Should().ContainSingle().Subject;
        finding.Id.Should().Be("SPCT0004");
        finding.Reference.Should().Contain("B-999");
        finding.Location.SourceTree!.FilePath.Should().Be("src/Example.cs");
    }

    [Then("both declared relationships are retained independently")]
    public void ThenMultiple() => _result!.Relationships.Should().HaveCount(_expectedCount);

    [Then("the declared relationship connects the test to its claim")]
    public void ThenVerification() => _result!.Relationships.Should().ContainSingle().Which.Role.Should().Be("verifies");

    [Then("the declared relationship connects implementation code to its claim")]
    public void ThenImplementation() => _result!.Relationships.Should().ContainSingle().Which.Role.Should().Be("implements");

    [Then("the declared relationship connects the code to its ADR")]
    public void ThenAdr() => _result!.Relationships.Should().ContainSingle().Which.Resolution!.Target!.Kind.Should().Be(BindingTargetKind.Adr);

    [Then("that placement is supported")]
    [Then("that XML shape is accepted")]
    public void ThenValidShape() => _result!.Elements.Should().ContainSingle().Which.ShapeValid.Should().BeTrue();

    [Then("that binding describes only the type itself")]
    public void ThenTypeOnly() => _type!.Relationships.Should().ContainSingle();

    [Then("the member has no implicit binding")]
    [Then("that relationship supplies no checked evidence")]
    public void ThenNoEvidence() => _result!.Relationships.Should().BeEmpty();

    [Then("it is treated as one identifier without splitting it into targets")]
    public void ThenScalar()
    {
        _result!.Elements.Should().ContainSingle().Which.Attributes["claim"].Should().Be(_scalar);
        _result.Relationships.Should().HaveCount(_scalar == "B-001,B-002" ? 1 : 0);
    }

    [Then("analysis reports a validation diagnostic")]
    public void ThenShapeFinding() => _result!.Findings.Should().ContainSingle().Which.Id.Should().Be("SPCT0002");

    [Then("target resolution and lifecycle eligibility remain separate checks")]
    public void ThenSeparateChecks() => _result!.Elements.Should().OnlyContain(static binding => binding.ShapeValid);

    [Then("the elements represent one relationship")]
    public void ThenDeduplicated() => _result!.Relationships.Should().ContainSingle();

    [Then("analysis reports a duplicate diagnostic")]
    public void ThenDuplicate() => _result!.Findings.Should().ContainSingle().Which.Id.Should().Be("SPCT0009");

    [Then("the two roles remain distinct relationships")]
    public void ThenDistinctRoles() => _result!.Relationships.Select(static binding => binding.Role).Should().BeEquivalentTo("implements", "verifies");

    [Then("each test method retains its own verification relationship")]
    public void ThenDistinctSymbols()
    {
        _result!.Relationships.Should().ContainSingle();
        _other!.Relationships.Should().ContainSingle();
        _other.Relationships[0].SymbolId.Should().NotBe(_result.Relationships[0].SymbolId);
    }

    [Then("its ID has the SPCT prefix followed by four decimal digits")]
    public void ThenNamespace() => _result!.Findings.Should().ContainSingle().Which.Id.Should().MatchRegex("^SPCT[0-9]{4}$");

    [Then("its published diagnostic ID is retained")]
    public void ThenPublished() => _registry!.Publications.Should().Contain(entry => entry.Id == _prior!.Id && entry.Finding == _prior.Finding);

    [Then("the retired ID is not assigned to it")]
    public void ThenRetired() => _prior!.Retired.Should().BeTrue();

    [Then("the pinned checker's SPEC rule vocabulary remains unchanged")]
    public void ThenCheckerUnchanged()
    {
        _registry!.Descriptors.Should().OnlyContain(static descriptor => descriptor.Id.StartsWith("SPCT", StringComparison.Ordinal));
        _pin!.RuleIds.OrderBy(static id => id, StringComparer.Ordinal).Should().Equal(_checkerIds!);
    }

    private string _xml = "<implements feature=\"F1\" claim=\"B-001\" />";
    private string _typeXml = string.Empty;
    private string _claimGrammar = "^B-[0-9]{3}$";
    private string? _scalar;
    private bool _onType;
    private bool _recognized;
    private bool _partial;
    private bool _secondMethod;
    private int _expectedCount;
    private string? _source;
    private DocumentationAnalysis? _result;
    private DocumentationAnalysis? _type;
    private DocumentationAnalysis? _other;
    private DiagnosticPublication? _prior;
    private DocumentationDiagnostics? _registry;
    private SchemaVersion? _pin;
    private string[]? _checkerIds;
}
