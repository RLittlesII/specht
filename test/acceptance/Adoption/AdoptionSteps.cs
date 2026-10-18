using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Reqnroll;
using Specht.Analysis.Bindings;
using Specht.Analysis.Policy;

namespace Specht.Acceptance.Adoption;

[Binding]
[Scope(Tag = "0201-F6")]
public sealed class AdoptionSteps
{
    [Given("a consumer selecting a subset of projects and declarations for enforcement")]
    public void GivenSelectedScope() => _declaration = new AnalysisDeclaration("test/tests.csproj", "net10.0", "test/Run.cs", "method", "M:Run", false, true);

    [Given("a selected analysis scope with missing or invalid manifest or specification inputs")]
    public void GivenMissingInputs() => _records.RemoveAll(static record => record.Kind == "feature");

    [Given("an analyzed project with an invalid explicit binding")]
    public void GivenInvalidExplicitBinding() => _reference = new ClaimReference(null, "F1", "invalid");

    [Given("the declaration is outside the scope requiring authored bindings")]
    public void GivenOutsideEnforcement() => _declaration = new AnalysisDeclaration("test/tests.csproj", "net10.0", "src/Shared.cs", "method", "M:Shared");

    [Given("generated code with no authored binding")]
    public void GivenGenerated() => _declaration = new AnalysisDeclaration("test/tests.csproj", "net10.0", "test/Run.cs", "method", "M:Run", true, true);

    [Given("shared infrastructure outside explicitly selected authoring enforcement scope")]
    public void GivenSharedInfrastructure() => GivenOutsideEnforcement();

    [Given("a requested coverage exemption with no explicit reason")]
    public void GivenReasonlessExemption() => _configuration["codeAnalysis"]!["exemptions"] = JsonNode.Parse("""
        [{"target":{"feature":"F1","claim":"B-001"},"links":["test"],"reason":" "}]
        """);

    [Given("enabled analysis with actual consumer specification and ADR record values")]
    public void GivenConsumerRecords() => _reference = new ClaimReference(null, "F1", "B-001");

    [Given("build integration supplies the selected manifest, records and configured schemas through AdditionalFiles")]
    public void GivenAdditionalFiles() => _supplied = true;

    [Given("enabled analysis with {string} required inputs")]
    public void GivenInputDefect(string defect)
    {
        if (defect == "missing")
        {
            _records.RemoveAll(static record => record.Kind == "feature");
        }
        else if (defect == "invalid")
        {
            _records.RemoveAll(static record => record.Kind == "feature");
            _records.Add(new AdoptionFile("src/read/.spec/README.md", "invalid frontmatter", "feature"));
        }
        else
        {
            _configuration["codeAnalysis"]!["mappings"]!["/identifiers"] = "/absent";
        }
    }

    [Given("no severity override for the input-failure rule")]
    public void GivenDefaultSeverity() => _defaultSeverity = true;

    [Given("an existing analysis verdict")]
    public void GivenExistingVerdict() => _before = Run();

    [Given("existing consumer configuration, its schemas and explicit analysis mappings")]
    public void GivenExistingConfiguration() => _supplied = true;

    [Given("a consumer supplying {string} at its own version {string}")]
    public void GivenConsumerSchema(string schema, string version)
    {
        _schemaOrigin = schema;
        _version = version;
        _configuration["revision"] = version;
    }

    [When("the declared analysis is requested")]
    [When("binding analysis runs")]
    [When("missing-binding requirements are evaluated")]
    [When("coverage policy is validated")]
    [When("bindings are resolved")]
    [When("analysis runs")]
    [When("analysis policy is read")]
    [When("analysis consumes the supplied configuration and records")]
    public void WhenRead() => _snapshot = Run();

    [When("{string} changes")]
    public void WhenInputChanges(string input)
    {
        if (input == "source")
        {
            _source = "class Changed { }";
        }
        else if (input == "specification")
        {
            var index = _records.FindIndex(static record => record.Kind == "feature");
            _records[index] = new AdoptionFile("src/read/.spec/README.md", FeatureText.Replace("B-001", "B-002"), "feature");
        }
        else
        {
            _configuration["revision"] = "consumer-r2";
        }

        _snapshot = Run();
    }

    [Then("only that selected scope is required to carry bindings and the scope is visible in evidence")]
    public void ThenScopeIsExplicit()
    {
        _snapshot!.Available.Should().BeTrue();
        _snapshot.Policy!.Projects.Should().ContainSingle().Which.Path.Should().Be("test/tests.csproj");
        _snapshot.Policy.Features.Should().ContainSingle().Which.Feature.Should().Be("F1");
        _snapshot.Policy.Select(_declaration!).RequiredRoles.Should().Equal("verifies");
        _snapshot.Policy.Select(_declaration! with { RelativePath = "src/Other.cs" }).RequiredRoles.Should().BeEmpty();
    }

    [Then("analysis reports the input failure rather than silently reporting clean coverage")]
    [Then("analysis is unavailable or incomplete rather than clean")]
    [Then("that exemption is rejected")]
    public void ThenUnavailable()
    {
        _snapshot!.Available.Should().BeFalse();
        _snapshot.Failures.Should().NotBeEmpty();
        _snapshot.Catalog.Should().BeNull();
    }

    [Then("the explicit binding is still validated")]
    public void ThenExplicitValidation()
    {
        _snapshot!.Available.Should().BeTrue();
        var selection = _snapshot.Policy!.Select(_declaration!);
        selection.ValidateExplicit.Should().BeTrue();
        selection.RequiredRoles.Should().BeEmpty();
        new BindingResolver(_snapshot.Catalog!, _snapshot.Policy.Bindings).Resolve(_reference!).Status.Should().Be(BindingResolution.Invalid);
    }

    [Then("no authored binding is required on that generated code")]
    [Then("its methods are not required to carry bindings merely because they are shared infrastructure")]
    public void ThenNoMandatoryAuthoring()
    {
        _snapshot!.Available.Should().BeTrue();
        _snapshot.Policy!.Select(_declaration!).RequiredRoles.Should().BeEmpty();
        _snapshot.Policy.Select(_declaration!).ValidateExplicit.Should().BeTrue();
    }

    [Then("analysis reads the consumer record contents rather than treating schemas as the record inventory")]
    public void ThenReadsRecords()
    {
        _supplied.Should().BeTrue();
        _snapshot!.Available.Should().BeTrue();
        var resolver = new BindingResolver(_snapshot.Catalog!, _snapshot.Policy!.Bindings);
        resolver.Resolve(_reference!).IsEligible.Should().BeTrue();
        resolver.Resolve(new AdrReference(null, null, "ADR-0027")).IsEligible.Should().BeTrue();
        resolver.Resolve(new ClaimReference(null, "F1", "B-002")).Status.Should().Be(BindingResolution.Unresolved);
    }

    [Then("an error diagnostic is reported")]
    public void ThenInputError()
    {
        _defaultSeverity.Should().BeTrue();
        _snapshot!.Failures.Should().NotBeEmpty();
        var descriptor = new AnalysisInputAnalyzer().SupportedDiagnostics.Should().ContainSingle().Subject;
        descriptor.Id.Should().Be("SPCT0001");
        descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
    }

    [Then("the affected analysis verdict is invalidated")]
    public void ThenInvalidated()
    {
        _snapshot!.Available.Should().BeTrue();
        _snapshot.ContentKey.Should().NotBe(_before!.ContentKey);
    }

    [Then("selected scopes, required links, exemptions and record discovery use that policy")]
    public void ThenMappedPolicy()
    {
        _snapshot!.Available.Should().BeTrue();
        _snapshot.Policy!.Projects.Should().ContainSingle();
        _snapshot.Policy.RequiredLinks(new ClaimReference(null, "F1", "B-001")).Should().Equal("test");
        _snapshot.Policy.ExemptionReasons(new ClaimReference(null, "F1", "B-001")).Should().BeEmpty();
        _snapshot.Catalog!.Adrs.Should().ContainSingle().Which.Id.Should().Be("ADR-0027");
    }

    [Then("existing consumer ID grammars remain authoritative")]
    public void ThenConsumerGrammars() => _snapshot!.Policy!.Bindings.Identifiers["claim"].Should().Be("^B-[0-9]{3}$");

    [Then("the supplied consumer schema and version are used")]
    public void ThenConsumerSchema()
    {
        _schemaOrigin.Should().NotBeNullOrEmpty();
        _snapshot!.Available.Should().BeTrue();
        _snapshot.Policy!.Bindings.SchemaVersion.Should().Be(_version);
        _snapshot.Catalog!.Features.Should().ContainSingle().Which.Identity.Feature.Should().Be("F1");
    }

    [Then("no Specht schema adoption, schema upgrade or pin change is required")]
    public void ThenNoUpgrade() => _configuration["revision"]!.GetValue<string>().Should().Be(_version);

    private AnalysisSnapshot Run()
    {
        var files = _records.Cast<AdditionalText>().Append(new AdoptionFile("config/consumer.json", _configuration.ToJsonString(), "configuration"))
            .ToImmutableArray();
        return new AnalysisInputAdapter().Read(files, new AdoptionOptions(),
            new[] { CSharpSyntaxTree.ParseText(_source, path: "src/Consumer.cs") }, Xunit.TestContext.Current.CancellationToken);
    }

    private const string FeatureText = """
        ---
        id: F1
        spec_status: draft
        ---
        ## Consumer claims
        | ID | Status |
        | --- | --- |
        | B-001 | Active |
        """;
    private readonly JsonNode _configuration = JsonNode.Parse("""
        {
          "revision":"0.1.0","consumerPatterns":{"feature":"^F[0-9]+$","claim":"^B-[0-9]{3}$"},
          "heading":"Consumer claims","stateMarkers":{"approved":"approved","withdrawn":"Withdrawn","superseded":"Superseded"},
          "codeAnalysis":{
            "projects":[{"path":"test/tests.csproj","targetFrameworks":["net10.0"]}],"features":[{"feature":"F1"}],
            "enforcement":[{"project":"test/tests.csproj","include":["test/**/*.cs"],"exclude":[],
              "role":"verifies","declarationKinds":["testMethod"]}],
            "records":{"adrProfiles":[{"glob":"decisions/*.md","owner":"repository",
              "identity":{"headingLevel":1,"separator":": "},"lifecycle":{"section":"Status","firstWord":true}}]},
            "mappings":{"/schemaVersion":"/revision","/identifiers":"/consumerPatterns","/claimsSection":"/heading","/markers":"/stateMarkers"}
          }
        }
        """)!;
    private readonly List<AdoptionFile> _records = new()
    {
        new AdoptionFile("src/read/.spec/README.md", FeatureText, "feature"),
        new AdoptionFile("contracts/feature.json", "{\"type\":\"object\",\"required\":[\"id\",\"spec_status\"]}", "schema:feature"),
        new AdoptionFile("decisions/0027.md", "# ADR-0027: A consumer decision\n\n## Status\n\nAccepted.\n", "adr"),
    };
    private string _source = "class Consumer { }";
    private AnalysisDeclaration? _declaration;
    private ClaimReference? _reference;
    private AnalysisSnapshot? _snapshot;
    private AnalysisSnapshot? _before;
    private bool _supplied;
    private bool _defaultSeverity;
    private string? _schemaOrigin;
    private string? _version;
}
