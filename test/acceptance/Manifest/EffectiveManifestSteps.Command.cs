using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Manifest;

public sealed partial class EffectiveManifestSteps
{
    [Given("a root path that is not a directory")]
    public void GivenMissingRoot() => _rootArgument = "missing-root";

    [Given("a root with no manifest")]
    public void GivenNoManifest() => File.Delete(Path.Combine(Tree.Root, SpecTree.ManifestPath));

    public void GivenMalformedManifest() => Tree.WriteRaw(SpecTree.ManifestPath, "{");

    [Given("a root whose manifest (.*)")]
    public void GivenRejectedManifest(string reason)
    {
        var manifest = Manifest;
        switch (reason)
        {
            case "is not well-formed":
                GivenMalformedManifest();
                return;
            case "carries a key the tool does not know":
                manifest["unknown"] = true;
                break;
            case "pins a version the tool does not ship":
                manifest["schemaVersion"] = "9.0.0";
                break;
            case "records an upstream schema source and explicitly selects the embedded source":
                manifest["schemaVersion"] = "0.2.0";
                manifest["schemaSource"] = "embedded";
                manifest["spechtSchema"] = JsonNode.Parse("""
                    {"location":"https://schemas.example/invented/","version":"synthetic",
                    "hashes":{"feature":"sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                    "task":"sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"}}
                    """);
                break;
            default:
                throw new InvalidOperationException(reason);
        }
        Write(manifest);
    }

    [Given("a snapshot of every file under the root")]
    public void GivenSnapshot() => _before = Snapshot();

    [When("the effective manifest is printed")]
    public void WhenPrinted() => Print();

    [When("the effective manifest is printed against that root")]
    public void WhenPrintedAgainstRoot() => Print();

    [When("the effective manifest is printed against that root given as an absolute path")]
    public void WhenPrintedAgainstAbsoluteRoot()
    {
        _rootArgument = Tree.Root;
        Print();
    }

    [When("the effective manifest is printed twice")]
    public void WhenPrintedTwice()
    {
        Print();
        _first = _result;
        Print();
    }

    [Then("the standard output carries the effective manifest of that root")]
    public void ThenEffectiveDocument() => JsonNode.DeepEquals(_document, EngineServices.EffectiveManifest(Tree.Root)).Should().BeTrue();

    [Then("the exit code is {int}")]
    public void ThenExitCode(int code) => _result.ExitCode.Should().Be(code, _result.Stderr);

    [Then("the standard error names the root as it was given")]
    public void ThenNamesRoot() => _result.Stderr.Should().Contain($"'{_rootArgument}'");

    [Then("the standard error names the manifest path")]
    public void ThenNamesManifest() => _result.Stderr.Should().Contain(SpecTree.ManifestPath);

    [Then("the standard error names what the check names for that manifest")]
    public void ThenMatchesCheck() => _result.Stderr.Should().Be(AcceptanceTool.Launch(Tree.Root).Stderr);

    [Then("the standard output is empty")]
    public void ThenEmptyOutput() => _result.Stdout.Should().BeEmpty();

    [Then("the standard output carries only the manifest")]
    public void ThenProductOnly()
    {
        JsonNode.Parse(_result.Stdout).Should().NotBeNull();
        _result.Stderr.Should().BeEmpty();
    }

    [Then("no file under the root was created, modified or deleted")]
    public void ThenUnchanged() => Snapshot().Should().BeEquivalentTo(_before);

    [Then("every path the output names other than those typed is relative to the root and uses forward slashes")]
    public void ThenRelativePaths() => _result.Stderr.Should().Contain(SpecTree.ManifestPath).And.NotContain(Tree.Root).And.NotContain("\\");

    [Then("both runs print the same standard output and exit with the same code")]
    public void ThenDeterministic() => _result.Should().Be(_first);

    [When("the effective manifest is printed as text and as JSON")]
    public void WhenPrintedInBothForms()
    {
        Print();
        _text = _result.Stdout;
        Print("--json");
        _json = _result.Stdout;
    }

    [Then("the text is the document the JSON form prints, indented")]
    public void ThenSameDocument()
    {
        JsonNode.DeepEquals(JsonNode.Parse(_text), JsonNode.Parse(_json)).Should().BeTrue();
        _text.Should().Contain("\n  ");
    }

    [Then("its keys are in the order the default manifest holds them")]
    public void ThenKeyOrder()
    {
        var document = JsonNode.Parse(_text)!.AsObject();
        var defaults = JsonNode.Parse(EngineServices.Versions().Versions[0].Manifest)!.AsObject();
        document.Select(static entry => entry.Key).Where(defaults.ContainsKey).Should()
            .Equal(defaults.Select(static entry => entry.Key).Where(document.ContainsKey));
    }

    [Then("its rule settings are in ascending order of rule id")]
    public void ThenRuleOrder() => JsonNode.Parse(_text)!["rules"]!.AsObject().Select(static entry => entry.Key).Should()
        .BeInAscendingOrder(StringComparer.Ordinal);

    [Given("the manifest the tool embeds for the newest version it ships")]
    public void GivenEmbeddedDefault() => _embedded = EngineServices.Versions().Versions[^1].Manifest;

    [When("the default manifest is printed")]
    [When("the default manifest is printed against that root")]
    public void WhenDefaultsPrinted() => Print("--defaults");

    [Then("the standard output carries the default manifest of the newest version the tool ships")]
    public void ThenDefaultDocument() => JsonNode.DeepEquals(_document, JsonNode.Parse(EngineServices.Versions().Versions[^1].Manifest)).Should().BeTrue();

    [Then("the standard output is what it is against a root with a valid manifest")]
    public void ThenIndependentOfRoot()
    {
        using var valid = new SpecTree();
        _result.Stdout.Should().Be(AcceptanceTool.Launch(valid.Root, "manifest", "--defaults").Stdout);
    }

    [Then("every key and value printed equals that embedded manifest")]
    public void ThenEmbeddedValues() => JsonNode.DeepEquals(_document, JsonNode.Parse(_embedded)).Should().BeTrue();

    [When("^(the effective|the default) manifest is printed as JSON$")]
    public void WhenJsonPrinted(string which)
    {
        if (which == "the default")
        {
            Print("--defaults", "--json");
        }
        else
        {
            Print("--json");
        }
    }

    [Then("the standard output parses as exactly one JSON document")]
    public void ThenOneDocument()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        JsonNode.Parse(_result.Stdout).Should().BeOfType<JsonObject>();
        _result.Stderr.Should().BeEmpty();
    }

    private void Print(params string[] options)
    {
        _result = AcceptanceTool.Launch(Tree.Root, ["manifest", "--root", _rootArgument, .. options]);
        _document = _result.ExitCode == 0 ? JsonNode.Parse(_result.Stdout)!.AsObject() : null;
    }

    private Dictionary<string, string> Snapshot() => Directory.GetFiles(Tree.Root, "*", SearchOption.AllDirectories)
        .ToDictionary(path => Path.GetRelativePath(Tree.Root, path).Replace('\\', '/'), static path => Convert.ToBase64String(File.ReadAllBytes(path)));

    private string _embedded = "";
    private string _text = "";
    private string _json = "";
    private string _rootArgument = ".";
    private Dictionary<string, string> _before = [];
    private (string Stdout, string Stderr, int ExitCode) _result;
    private (string Stdout, string Stderr, int ExitCode) _first;
}
