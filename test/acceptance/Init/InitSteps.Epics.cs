using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Init;

/// <summary>
/// The epic tier of <c>init</c> (0001-F4 B-002, B-018, B-020 to B-023; C-5, C-6). The runs launch the built tool with
/// <c>--epics</c> beside the part options. A manifest declaring no epic grammar is the embedded manifest of the newest version
/// with <c>identifiers.epic</c> removed, written before the run so its bytes can be compared after it.
/// </summary>
public sealed partial class InitSteps
{
    [Given("a root directory holding only a manifest pinning the newest version the tool ships and declaring no epic grammar")]
    [Given("a root directory holding a manifest declaring no epic grammar")]
    public void GivenARootDirectoryHoldingAManifestDeclaringNoEpicGrammar()
    {
        Prepare();
        var version = NewestVersion();
        var embedded = Read(Embedded(Manifest, version));
        embedded.Should().NotBeNull($"the tool embeds {Embedded(Manifest, version)}");
        var manifest = JsonNode.Parse(embedded)!.AsObject();
        manifest["schemaVersion"] = EngineServices.Versions().Versions[^1].Number.ToString();
        manifest["identifiers"]?.AsObject().Remove("epic");
        (manifest["identifiers"]?["epic"]).Should().BeNull("the manifest declares no epic grammar");
        _manifest = System.Text.Encoding.UTF8.GetBytes(manifest.ToJsonString());
        Directory.CreateDirectory(Path.GetDirectoryName(Written(Manifest))!);
        File.WriteAllBytes(Written(Manifest), _manifest);
    }

    [When("init runs against it with epics")]
    public void WhenInitRunsAgainstItWithEpics() => Run("--epics");

    [When("init runs against it with schemas and epics")]
    public void WhenInitRunsAgainstItWithSchemasAndEpics() => Run("--schemas", "--epics");

    [When("init runs against it with every part and epics")]
    public void WhenInitRunsAgainstItWithEveryPartAndEpics() => Run("--all", "--epics");

    [Then("the manifest written declares no epic grammar")]
    public void ThenTheManifestWrittenDeclaresNoEpicGrammar() =>
        (WrittenManifest()["identifiers"] as JsonObject ?? new JsonObject()).Should().NotContainKey("epic", _stdout + _stderr);

    [Then("the manifest written has a task grammar with no epic part")]
    public void ThenTheManifestWrittenHasATaskGrammarWithNoEpicPart()
    {
        var task = WrittenManifest()["identifiers"]?["task"]?.GetValue<string>();
        task.Should().NotBeNull($"the manifest written declares a task grammar; {_stdout}{_stderr}");
        new Regex(task!).GetGroupNames().Should().NotContain("epic");
    }

    private JsonObject WrittenManifest()
    {
        File.Exists(Written(Manifest)).Should().BeTrue($"init writes .specht/{Manifest}; {_stdout}{_stderr}");

        return JsonNode.Parse(File.ReadAllText(Written(Manifest)))!.AsObject();
    }
}
