using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Specht.Composition;
using Specht.Tests.Shared;
using Specht.Tool;
using Specht.Tool.Features.Init;
using Specht.Tool.Features.Upgrade;
using Spectre.Console;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [Given("the root's local copy does not match that hash")]
    public void GivenTheRootsLocalCopyDoesNotMatchThatHash() => Diverge("feature", "task", "epic");

    [Given("the root's local copies of two schemas do not match their hashes")]
    public void GivenTheRootsLocalCopiesOfTwoSchemasDoNotMatchTheirHashes() => Diverge("feature", "task");

    [Given("the root's local copy matches that hash")]
    public void GivenTheRootsLocalCopyMatchesThatHash()
    {
        var hashes = ManifestAfter()["spechtSchema"]!["hashes"]!.AsObject();
        foreach (var (kind, name) in SchemaNames())
        {
            hashes[kind]!.GetValue<string>().Should().Be(HashOf($"{SpecTree.FrontmatterSchemaFolder}/{name}"));
        }
    }

    [Given("the source serves content matching that hash")]
    public void GivenTheSourceServesContentMatchingThatHash() => Upstream.Served.Should().NotBeEmpty();

    [Given("the source serves content that does not match that hash")]
    public void GivenTheSourceServesContentThatDoesNotMatchThatHash() => Upstream.Refused["*"] = "mismatch";

    [Given("the source serves one of those schemas matching its recorded hash and the other not")]
    public void GivenTheSourceServesOneOfThoseSchemasMatchingItsRecordedHashAndTheOtherNot() =>
        Upstream.Refused[SchemaNames()["task"]] = "mismatch";

    [Given("the source cannot be reached")]
    public void GivenTheSourceCannotBeReached() => Upstream.Refused["*"] = "unreachable";

    [Given("the source asks for a credential")]
    public void GivenTheSourceAsksForACredential() => Upstream.Refused["*"] = "401";

    [Given("the source does not serve the feature schema")]
    public void GivenTheSourceDoesNotServeTheFeatureSchema() => Upstream.Refused[SchemaNames()["feature"]] = "404";

    [Given("the root has no copy of the feature schema")]
    public void GivenTheRootHasNoCopyOfTheFeatureSchema()
    {
        var path = Path.Combine(Tree.Root, SpecTree.FrontmatterSchemaFolder, SchemaNames()["feature"]);
        _absentFeatureSchema = File.ReadAllBytes(path);
        File.Delete(path);
    }

    [Given("the source serves a feature schema matching that hash")]
    public void GivenTheSourceServesAFeatureSchemaMatchingThatHash() =>
        Upstream.Served[SchemaNames()["feature"]] = _absentFeatureSchema
            ?? throw new InvalidOperationException("The root's feature schema was not removed.");

    [Given("the manifest names the file of each frontmatter schema")]
    public void GivenTheManifestNamesTheFileOfEachFrontmatterSchema() =>
        EditManifest(manifest =>
        {
            var names = new JsonObject();
            foreach (var (kind, name) in SchemaNames())
            {
                var renamed = $"consumer-{kind}.schema.json";
                File.Move(
                    Path.Combine(Tree.Root, SpecTree.FrontmatterSchemaFolder, name),
                    Path.Combine(Tree.Root, SpecTree.FrontmatterSchemaFolder, renamed));
                names[kind] = renamed;
            }

            manifest["frontmatterSchemas"] = names;
        });

    [Given("the machine holds a token the source would accept")]
    public void GivenTheMachineHoldsATokenTheSourceWouldAccept() => _machineToken = true;

    [Given(@"^the root has no (feature frontmatter schema|feature template)$")]
    public void GivenTheRootHasNo(string file)
    {
        var path = Path.Combine(Tree.Root, AbsentFiles[file]);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    [Given("the root is pinned to version {word} and records an upstream schema source at a version of its own")]
    public void GivenTheRootIsPinnedAndRecordsAnUpstreamSchemaSourceAtAVersionOfItsOwn(string version)
    {
        GivenTheManifestPinsVersion(version);
        WriteUpstreamSchemaSource(static source => source["version"] = "consumer-2026.10");
        _recordedSource = ManifestAfter()["spechtSchema"]!.DeepClone();
    }

    [When("upgrade runs")]
    public void WhenUpgradeRuns()
    {
        _filesBefore ??= SnapshotFiles();
        using var client = new HttpClient(Upstream, disposeHandler: false);
        using var errors = new StringWriter();
        var console = new TestConsole().Width(200);
        var services = new ServiceCollection();
        services.AddSingleton<IAnsiConsole>(console);
        services.AddSingleton(client);
        services.AddSingleton<TextWriter>(errors);
        var app = new CommandAppTester(new TypeRegistrar(services.AddSpechtEngine().AddUpgrade()), console: console);
        app.Configure(static configuration => configuration.AddUpgrade());
        var token = Environment.GetEnvironmentVariable(TokenVariable);
        try
        {
            if (_machineToken)
            {
                Environment.SetEnvironmentVariable(TokenVariable, MachineToken);
            }

            var result = app.Run("upgrade", "--root", Tree.Root);
            (_stdout, _stderr, _exitCode) = (result.Output, errors.ToString(), result.ExitCode);
        }
        finally
        {
            Environment.SetEnvironmentVariable(TokenVariable, token);
        }

        _upgradedInProcess = true;
    }

    [Then("the fetched schemas are written under the root's schema folder")]
    public void ThenTheFetchedSchemasAreWrittenUnderTheRootsSchemaFolder()
    {
        foreach (var (_, name) in SchemaNames())
        {
            File.ReadAllBytes(Path.Combine(Tree.Root, SpecTree.FrontmatterSchemaFolder, name))
                .Should().Equal(Upstream.Served[name], "{0} is fetched and verified: {1}", name, _stderr);
        }
    }

    [Then("each fetched schema is in the root's schema folder under the file name the manifest gives it")]
    public void ThenEachFetchedSchemaIsInTheRootsSchemaFolderUnderTheFileNameTheManifestGivesIt()
    {
        ThenTheFetchedSchemasAreWrittenUnderTheRootsSchemaFolder();
        foreach (var path in SchemaPaths.Values)
        {
            File.Exists(Path.Combine(Tree.Root, path)).Should().BeFalse("no schema is written under a name the manifest does not give it");
        }
    }

    [Then("the root still has no copy of the feature schema")]
    public void ThenTheRootStillHasNoCopyOfTheFeatureSchema() =>
        File.Exists(Path.Combine(Tree.Root, SpecTree.FrontmatterSchemaFolder, SchemaNames()["feature"])).Should().BeFalse(_stderr);

    [Then(@"^the root still has no (feature frontmatter schema|feature template)$")]
    public void ThenTheRootStillHasNo(string file)
    {
        _exitCode.Should().Be(0, _stderr);
        File.Exists(Path.Combine(Tree.Root, AbsentFiles[file])).Should().BeFalse(_stderr);
    }

    [Then("nothing the tool asked the source for carried a credential")]
    public void ThenNothingTheToolAskedTheSourceForCarriedACredential()
    {
        Upstream.Requests.Should().NotBeEmpty("the differing copies are fetched: {0}", _stderr);
        Upstream.Requests.Should().AllSatisfy(static request =>
        {
            request.Address.UserInfo.Should().BeEmpty();
            request.Headers.Should().NotContain("Authorization").And.NotContain(MachineToken);
        });
    }

    [Then("each schema was asked for at the source's address followed by that schema's file name")]
    public void ThenEachSchemaWasAskedForAtTheSourcesAddressFollowedByThatSchemasFileName()
    {
        _exitCode.Should().Be(0, _stderr);
        Upstream.Requests.Select(static request => request.Address.AbsoluteUri)
            .Should().BeEquivalentTo(SchemaNames().Values.Select(static name => SourceAddress + name));
    }

    [Then("neither the manifest nor a template was asked for at the source")]
    public void ThenNeitherTheManifestNorATemplateWasAskedForAtTheSource()
    {
        _exitCode.Should().Be(0, _stderr);
        var templates = InitWriter.ShippingCopy(typeof(InitWriter).Assembly).Keys
            .Where(static resource => resource.StartsWith("templates/", StringComparison.Ordinal))
            .Select(static resource => resource[(resource.LastIndexOf('/') + 1)..])
            .ToHashSet(StringComparer.Ordinal);
        templates.Should().NotBeEmpty();
        Upstream.Requests.Should().NotBeEmpty("the differing copies are fetched: {0}", _stderr);
        Upstream.Requests.Select(static request => request.Address.Segments[^1])
            .Should().NotContain(Path.GetFileName(SpecTree.ManifestPath)).And.NotContain(name => templates.Contains(name));
    }

    [Then("the standard error names the source")]
    public void ThenTheStandardErrorNamesTheSource() => _stderr.Should().Contain(SourceAddress).And.NotContain(Tree.Root);

    [Then("the manifest still records the upstream source at that version with the same content hash")]
    public void ThenTheManifestStillRecordsTheUpstreamSourceAtThatVersionWithTheSameContentHash()
    {
        _exitCode.Should().Be(0, _stderr);
        JsonNode.DeepEquals(ManifestAfter()["spechtSchema"], _recordedSource).Should().BeTrue();
    }

    private UpstreamSource Upstream => _upstream ??= new UpstreamSource();

    private void Diverge(params string[] kinds)
    {
        var names = SchemaNames();
        EditManifest(manifest =>
        {
            var hashes = manifest["spechtSchema"]!["hashes"]!.AsObject();
            foreach (var kind in kinds)
            {
                var served = Encoding.UTF8.GetBytes($"{{\"title\":\"A synthetic upstream {kind} schema\"}}\n");
                Upstream.Served[names[kind]] = served;
                hashes[kind] = "sha256:" + Convert.ToHexStringLower(SHA256.HashData(served));
            }
        });
        foreach (var kind in names.Keys.Except(kinds))
        {
            Upstream.Served[names[kind]] = File.ReadAllBytes(Path.Combine(Tree.Root, SpecTree.FrontmatterSchemaFolder, names[kind]));
        }
    }

    private Dictionary<string, string> SchemaNames()
    {
        var names = ManifestAfter()["frontmatterSchemas"]?.AsObject();
        return SchemaPaths.Keys.ToDictionary(
            static kind => kind,
            kind => names?[kind]?.GetValue<string>() ?? $"{kind}.schema.json",
            StringComparer.Ordinal);
    }

    private sealed class UpstreamSource : HttpMessageHandler
    {
        public Dictionary<string, byte[]> Served { get; } = new(StringComparer.Ordinal);

        public Dictionary<string, string> Refused { get; } = new(StringComparer.Ordinal);

        public List<(Uri Address, string Headers)> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var name = request.RequestUri!.Segments[^1];
            Requests.Add((request.RequestUri, request.Headers.ToString()));
            var refusal = Refused.GetValueOrDefault(name) ?? Refused.GetValueOrDefault("*");
            return Task.FromResult(refusal switch
            {
                "unreachable" => throw new HttpRequestException("Synthetic network unavailable."),
                "mismatch" => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent("{}"u8.ToArray()) },
                not null => new HttpResponseMessage((HttpStatusCode)int.Parse(refusal, CultureInfo.InvariantCulture)),
                _ when Served.TryGetValue(name, out var served) => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(served) },
                _ => new HttpResponseMessage(HttpStatusCode.NotFound),
            });
        }
    }

    private const string SourceAddress = "https://schemas.example/specht/";

    private const string TokenVariable = "SPECHT_SCHEMA_TOKEN";

    private const string MachineToken = "synthetic-machine-token";

    private static readonly Dictionary<string, string> AbsentFiles = new(StringComparer.Ordinal)
    {
        ["feature frontmatter schema"] = SpecTree.FeatureSchemaPath,
        ["feature template"] = SpecTree.TemplatesFolder + "/feature.md",
    };

    private UpstreamSource? _upstream;
    private bool _upgradedInProcess;
    private bool _machineToken;
    private byte[]? _absentFeatureSchema;
    private JsonNode? _recordedSource;
}
