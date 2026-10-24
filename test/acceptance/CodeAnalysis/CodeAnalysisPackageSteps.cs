using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml.Linq;
using AwesomeAssertions;
using Reqnroll;
using YamlDotNet.RepresentationModel;

namespace Specht.Acceptance.CodeAnalysis;

[Binding]
[Scope(Feature = "Consumer analysis adoption")]
public sealed class CodeAnalysisPackageSteps(PackedTool tool) : IDisposable
{
    public void Dispose()
    {
        if (_consumer is not null && Directory.Exists(_consumer))
        {
            Directory.Delete(_consumer, recursive: true);
        }
    }

    [Given("a consumer installing Specht.CodeAnalysis through a private PackageReference")]
    public void GivenPrivatePackageReference() => _privateReference = true;

    [When("the package is consumed")]
    public async Task WhenPackageIsConsumed()
    {
        _privateReference.Should().BeTrue();
        _analysisPackage = await tool.CodeAnalysisPackageAsync();
    }

    [Then("analyzers, code fixes and build integration are available")]
    public void ThenPackageAssetsAreAvailable()
    {
        using var archive = OpenAnalysisPackage();
        var entries = archive.Entries.Select(static entry => entry.FullName).ToArray();
        entries.Should().Contain("analyzers/dotnet/cs/Specht.Analysis.dll");
        entries.Should().Contain("analyzers/dotnet/cs/Specht.Codefixes.dll");
        entries.Should().Contain("build/Specht.CodeAnalysis.props");
        entries.Should().Contain("build/Specht.CodeAnalysis.targets");
    }

    [Then("no application runtime dependency is introduced")]
    public void ThenNoApplicationRuntimeDependency()
    {
        using var archive = OpenAnalysisPackage();
        var metadata = Metadata(archive);
        metadata.Descendants().Where(static element => element.Name.LocalName == "dependency").Should().BeEmpty();
        archive.Entries.Should().NotContain(static entry => entry.FullName.StartsWith("lib/", StringComparison.Ordinal));
    }

    [Given("a repository release")]
    public void GivenRepositoryRelease() => _repositoryRelease = true;

    [When("analysis and tool packages are produced")]
    public async Task WhenBothPackagesAreProduced()
    {
        _repositoryRelease.Should().BeTrue();
        _analysisPackage = await tool.CodeAnalysisPackageAsync();
        _toolPackage = await tool.PackageAsync();
    }

    [Then("both use the repository release version")]
    public void ThenPackageVersionsMatch()
    {
        VersionOf(_analysisPackage!, "Specht.CodeAnalysis.").Should().Be(VersionOf(_toolPackage!, "specht.tool."));
    }

    [Then("both are attached to the GitHub Release")]
    public void ThenBothPackagesAreAttached()
    {
        var stream = new YamlStream();
        using var workflow = File.OpenText(Path.Combine(tool.Repository, ".github", "workflows", "release.yml"));
        stream.Load(workflow);
        var root = (YamlMappingNode)stream.Documents[0].RootNode;
        var jobs = (YamlMappingNode)root["jobs"];
        var release = (YamlMappingNode)jobs["release"];
        var steps = (YamlSequenceNode)release["steps"];
        var create = steps.Cast<YamlMappingNode>().Single(static step => step["name"].ToString() == "Create GitHub Release");
        var command = create["run"].ToString().TrimEnd();
        command.Should().Be("./build.cmd --target CreateGitHubRelease");
    }

    [Given("different installed package versions with compatible schema and evidence contracts")]
    public void GivenCompatibleContracts() => _compatibleContracts = true;

    [When("interoperability is evaluated")]
    public async Task WhenInteroperabilityIsEvaluated()
    {
        _compatibleContracts.Should().BeTrue();
        _analysisPackage = await tool.CodeAnalysisPackageAsync();
    }

    [Then("exact package-version equality is not required")]
    public void ThenPackageVersionEqualityIsNotRequired()
    {
        using var archive = OpenAnalysisPackage();
        Metadata(archive).Descendants().Where(static element => element.Name.LocalName == "dependency").Should().BeEmpty();
        archive.Entries.Select(static entry => entry.FullName).Should().Contain("analyzers/dotnet/cs/JsonSchema.Net.dll");
    }

    [Given(@"MSBuild root\/input configuration and standard Roslyn reporting configuration")]
    public void GivenRootAndReportingConfiguration() => _reportingConfiguration = true;

    [When("analysis is configured")]
    public async Task WhenAnalysisIsConfigured()
    {
        _reportingConfiguration.Should().BeTrue();
        _analysisPackage = await tool.CodeAnalysisPackageAsync();
    }

    [Then("MSBuild determines the repository root and supplied inputs")]
    public void ThenMsBuildSuppliesInputs()
    {
        using var archive = OpenAnalysisPackage();
        using var reader = new StreamReader(archive.GetEntry("build/Specht.CodeAnalysis.props")!.Open());
        var props = reader.ReadToEnd();
        props.Should().Contain("SpechtRoot");
        props.Should().Contain("<AdditionalFiles Include=");
        props.Should().Contain("CompilerVisibleItemMetadata");
    }

    [Then("severity and suppression use standard Roslyn controls")]
    public void ThenSeverityAndSuppressionAreStandard()
    {
        using var archive = OpenAnalysisPackage();
        using var reader = new StreamReader(archive.GetEntry("README.md")!.Open());
        reader.ReadToEnd().Should().Contain("standard Roslyn configuration");
    }

    [Given("a consumer installing Specht.CodeAnalysis")]
    public async Task GivenConsumerInstallingThePackage()
    {
        _analysisPackage = await tool.CodeAnalysisPackageAsync();
        var created = Directory.CreateTempSubdirectory("specht-codeanalysis-consumer-").FullName;
        _consumer = Path.GetPathRoot(created)!;
        foreach (var segment in created[_consumer.Length..].Split(Path.DirectorySeparatorChar))
        {
            _consumer = Path.Combine(_consumer, segment);
            var target = new DirectoryInfo(_consumer).LinkTarget;
            _consumer = target is null ? _consumer : Path.GetFullPath(target, Path.GetDirectoryName(_consumer)!);
        }

        Directory.CreateDirectory(Path.Combine(_consumer, ".specht", "schema", "frontmatter"));
        Directory.CreateDirectory(Path.Combine(_consumer, "src", "Feature", ".spec"));
        Directory.CreateDirectory(Path.Combine(_consumer, "test"));
        await File.WriteAllTextAsync(Path.Combine(_consumer, ".specht", "manifest.json"), """
            {
              "revision":"consumer-r7",
              "idPatterns":{"feature":"^F[0-9]+$","claim":"^B-[0-9]+$","adr":"^ADR-[0-9]+$"},
              "claimHeading":"Claims",
              "lifecycleMarkers":{"approved":"approved","withdrawn":"Withdrawn","superseded":"Superseded"},
              "codeAnalysis":{
                "projects":[{"path":"Consumer.csproj","targetFrameworks":["net10.0"]}],
                "features":[{"feature":"F1"}],
                "enforcement":[{"project":"Consumer.csproj","include":["test/**/*.cs"],"exclude":[],
                  "role":"verifies","declarationKinds":["testMethod"]}],
                "records":{
                  "adrProfiles":[{
                    "glob":".spec/adr/*.md","owner":"repository",
                    "identity":{"headingLevel":1,"separator":": "},
                    "lifecycle":{"section":"Status","firstWord":true}
                  }]
                },
                "mappings":{"/schemaVersion":"/revision","/identifiers":"/idPatterns","/claimsSection":"/claimHeading","/markers":"/lifecycleMarkers"}
              }
            }
            """);
        await File.WriteAllTextAsync(Path.Combine(_consumer, ".specht", "schema", "frontmatter", "feature.schema.json"), """
            {"type":"object","required":["id","spec_status"],"properties":{"id":{"type":"string"},"spec_status":{"type":"string"}}}
            """);
        await File.WriteAllTextAsync(Path.Combine(_consumer, "src", "Feature", ".spec", "README.md"), """
            ---
            id: F1
            spec_status: draft
            ---
            ## Claims
            | ID | Status |
            | --- | --- |
            | B-001 | Active |
            """);
        await File.WriteAllTextAsync(Path.Combine(_consumer, "test", "Sample.cs"), "public sealed class Sample { public void Run() { } }");
    }

    [Given("a consumer keeping its Gherkin scenarios in a central specification tree under its root")]
    public async Task GivenCentralScenarioTree()
    {
        await GivenConsumerInstallingThePackage();
        Directory.CreateDirectory(Path.Combine(_consumer!, ".spec", "features"));
        await File.WriteAllTextAsync(Path.Combine(_consumer!, ".spec", "features", "central.feature"), "Feature: Central scenarios\n");
    }

    [Given("the consumer sets no scenario input property")]
    public void GivenNoScenarioInputProperty()
    {
    }

    [Given("the consumer declares a test evidence file for its build")]
    public async Task GivenConsumerDeclaresTestEvidence()
    {
        Directory.CreateDirectory(Path.Combine(_consumer!, "evidence"));
        await File.WriteAllTextAsync(Path.Combine(_consumer!, "evidence", "Sample.json"), """
            {
              "format": "specht.test-evidence",
              "formatVersion": 1,
              "project": {"path": "Consumer.csproj", "targetFramework": "net10.0"},
              "configuration": {"name": "Debug", "fingerprint": "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc"},
              "assembly": {"name": "Consumer", "sha256": "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb", "path": "bin/Consumer.dll"},
              "sourceFingerprint": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
              "policyFingerprint": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
              "adapter": {"id": "custom.runner", "version": "1.2.3", "contractVersion": "1"},
              "discovery": {
                "state": "complete",
                "sessionId": "session-1",
                "methods": [{
                  "symbolId": "M:Sample.Run", "declaringType": "Sample", "name": "Run", "genericArity": 0, "parameterTypes": [],
                  "location": {"path": "test/Sample.cs", "line": 1, "column": 42}
                }],
                "cases": [{"caseId": "case-1", "methodSymbolId": "M:Sample.Run", "rowId": null, "displayName": "Sample.Run"}]
              },
              "bindings": [{
                "symbolId": "M:Sample.Run",
                "role": "verifies",
                "attributes": {"feature": "F1", "claim": "B-001"},
                "shapeValid": true,
                "status": "resolved",
                "isEligible": true,
                "target": {
                  "kind": "claim", "epic": null, "feature": "F1", "id": "B-001", "state": "Active", "featureState": "draft",
                  "relativePath": "src/Feature/.spec/README.md", "line": 8
                },
                "matches": [],
                "location": {"path": "test/Sample.cs", "line": 1, "column": 42}
              }],
              "results": {"state": "absent"},
              "inputs": []
            }
            """);
        _evidence = "evidence/Sample.json";
    }

    [When("the consumer's project is analyzed")]
    public async Task WhenConsumerProjectIsAnalyzed()
    {
        var evidence = _evidence is null ? string.Empty : $"<ItemGroup><SpechtTestEvidence Include=\"{_evidence}\" /></ItemGroup>";
        await File.WriteAllTextAsync(Path.Combine(_consumer!, "Consumer.csproj"), $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
              <ItemGroup><PackageReference Include="Specht.CodeAnalysis" Version="{VersionOf(_analysisPackage!, "Specht.CodeAnalysis.")}" /></ItemGroup>
              {evidence}
              <Target Name="PrintSpechtInputs" BeforeTargets="CoreCompile">
                <Message Importance="high" Text="SPECHT-INPUT:%(AdditionalFiles.SpechtInputKind)|%(AdditionalFiles.SpechtRelativePath)" />
              </Target>
            </Project>
            """);
        var project = Path.Combine(_consumer!, "Consumer.csproj");
        var restore = await tool.DotNetAsync("restore", project, "--source", Path.GetDirectoryName(_analysisPackage)!,
            "--source", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages"),
            "--packages", Path.Combine(_consumer!, "packages"));
        restore.ExitCode.Should().Be(0, restore.Stdout + restore.Stderr);
        var build = await tool.DotNetAsync("build", project, "--no-restore", "-p:UseSharedCompilation=false");
        build.ExitCode.Should().Be(0, build.Stdout + build.Stderr);
        _analysis = build.Stdout + build.Stderr;
    }

    [Then("the test methods that evidence identifies are recognised as tests")]
    public void ThenEvidenceMethodsAreRecognisedAsTests()
    {
        _analysis.Should().NotContain("SPCT0001");
        _analysis.Should().Contain("warning SPCT0007: M:Sample.Run requires verifies");
    }

    [Then("the scenarios in that tree are delivered as scenario inputs")]
    public void ThenCentralScenariosAreDelivered() =>
        _analysis.Should().Contain("SPECHT-INPUT:scenario|.spec/features/central.feature");

    private static XElement Metadata(ZipArchive archive)
    {
        var manifest = archive.Entries.Single(static entry => entry.FullName.EndsWith(".nuspec", StringComparison.Ordinal));
        using var stream = manifest.Open();
        return XDocument.Load(stream).Descendants().Single(static element => element.Name.LocalName == "metadata");
    }

    private static string VersionOf(string path, string prefix) => Path.GetFileNameWithoutExtension(path)[prefix.Length..];

    private ZipArchive OpenAnalysisPackage() =>
        ZipFile.OpenRead(_analysisPackage ?? throw new InvalidOperationException("No analysis package was produced."));

    private bool _privateReference;
    private bool _repositoryRelease;
    private bool _compatibleContracts;
    private bool _reportingConfiguration;
    private string? _analysisPackage;
    private string? _toolPackage;
    private string? _consumer;
    private string? _evidence;
    private string? _analysis;
}
