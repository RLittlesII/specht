using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>
/// The parser's reading of the frontmatter key roles, the identity and edge forms and the grammars a manifest declares
/// (<c>0001-F5</c> B-004, B-014, B-019, B-049 to B-052, B-068; decisions 0010, 0016 and 0017). From schema 0.3.0 every
/// grammar is compiled at parse, the task grammar names its <c>sequence</c> group and, where the manifest declares the
/// epic grammar, its <c>epic</c> group, each form holds its placeholders exactly once, and an omitted kind, role or form is
/// the pinned default manifest's. Below 0.3.0 the parser keeps each pin's own handling. A 0.3.0 fixture declares the
/// epic grammar unless a row says otherwise, since decision 0018 keeps a manifest without it on today's reading.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtManifestParserKeysAndFormsUnitTests
{
    [Theory]
    [ClassData(typeof(UncompilableGrammars))]
    public void AGrammarThatDoesNotCompileAtZeroThreeZero_WhenParsed_ShouldBeRejectedNamingTheGrammar(string grammar)
    {
        // Given
        var identifiers = new JsonObject { ["epic"] = "^[0-9]{4}$" };
        identifiers[grammar] = "[";
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0", ["identifiers"] = identifiers };

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        parse.Should().Throw<SpechtManifestException>().Which.Message.Should().MatchRegex($@"\b{grammar}\b");
    }

    [Theory]
    [ClassData(typeof(OlderPinGrammarHandling))]
    public void AnOlderPin_WhenParsingAGrammar_ShouldKeepThatPinsParseTimeHandling(string pin, string grammar, string value, bool rejected)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = pin, ["identifiers"] = new JsonObject { [grammar] = value } };

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        if (rejected)
        {
            parse.Should().Throw<SpechtManifestException>().Which.Message.Should().MatchRegex($@"\b{grammar}\b");
        }
        else
        {
            parse.Should().NotThrow();
        }
    }

    [Theory]
    [ClassData(typeof(NewKeysUnderOlderPins))]
    public void ANewKeyOrFormUnderAnOlderPin_WhenParsed_ShouldRemainUnknown(string pin, string key)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = pin, [key] = new JsonObject() };

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        parse.Should().Throw<SpechtManifestException>().Which.Message.Should().Contain($"does not know the key '{key}'");
    }

    [Theory]
    [ClassData(typeof(GrouplessTaskGrammars))]
    public void ATaskGrammarWithoutASequenceGroupAtZeroThreeZero_WhenParsed_ShouldBeRejectedNamingTheGrammarAndTheMissingGroup(
        string task,
        bool declaresEpic)
    {
        // Given
        var identifiers = new JsonObject { ["task"] = task };
        if (declaresEpic)
        {
            identifiers["epic"] = "^[0-9]{4}$";
        }

        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0", ["identifiers"] = identifiers };

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        parse.Should().Throw<SpechtManifestException>().Which.Message.Should().MatchRegex(@"\btask\b").And.MatchRegex(@"\bsequence\b");
    }

    [Fact]
    public void AnEpicTaskGrammarWithoutAnEpicGroupAtZeroThreeZero_WhenParsed_ShouldBeRejectedNamingTheGrammarAndTheMissingGroup()
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$", ["task"] = "^[0-9]{4}-(?<sequence>[0-9]{2})$" },
        };

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        parse.Should().Throw<SpechtManifestException>().Which.Message.Should().MatchRegex(@"\btask\b").And.MatchRegex(@"\bepic\b");
    }

    [Theory]
    [ClassData(typeof(MalformedForms))]
    public void AFormWithoutEachExpectedPlaceholderExactlyOnce_WhenParsed_ShouldBeRejectedNamingTheFormAndItsValue(
        string form,
        string value,
        bool declaresEpic)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0", ["forms"] = new JsonObject { [form] = value } };
        if (declaresEpic)
        {
            manifest["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" };
        }

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        parse.Should().Throw<SpechtManifestException>().Which.Message.Should().Contain(form).And.Contain(value);
    }

    [Theory]
    [ClassData(typeof(OmittedForms))]
    public void AZeroThreeZeroManifestOmittingAForm_WhenParsed_ShouldReadThatFormAsThePinnedDefault(string omitted, string expected)
    {
        // Given
        var declared = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["identity"] = "{epic}.{id}",
            ["localEdge"] = "~{id}",
            ["qualifiedEdge"] = "{epic}:{id}",
        };
        declared.Remove(omitted);
        var forms = new JsonObject();
        foreach (var (name, value) in declared)
        {
            forms[name] = value;
        }

        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" },
            ["forms"] = forms,
        };

        // When
        var document = EngineServices.Parser().ParseDocument(manifest.ToJsonString()).Document;

        // Then
        Texts(document["forms"]).Should().BeEquivalentTo(new Dictionary<string, string>(declared) { [omitted] = expected });
    }

    [Fact]
    public void AZeroThreeZeroManifestRenamingOneFeatureKey_WhenParsed_ShouldFillEveryOtherRoleAndTheOmittedKindFromThePinnedDefault()
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" },
            ["keys"] = new JsonObject { ["feature"] = new JsonObject { ["children"] = "tasks" } },
        };

        // When
        var document = EngineServices.Parser().ParseDocument(manifest.ToJsonString()).Document;

        // Then
        Texts(document["keys"]?["feature"]).Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["id"] = "id",
            ["epic"] = "epic",
            ["specStatus"] = "spec_status",
            ["status"] = "status",
            ["children"] = "tasks",
            ["spikes"] = "spikes",
            ["dependsOn"] = "depends_on",
            ["blocks"] = "blocks",
        });
        Texts(document["keys"]?["task"]).Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["id"] = "id",
            ["parent"] = "parent",
            ["type"] = "type",
        });
    }

    [Fact]
    public void AZeroThreeZeroManifestDeclaringNoTaskGrammar_WhenParsed_ShouldReadTheDefaultTaskGrammarNamingItsEpicAndSequenceGroups()
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" },
        };

        // When
        var structure = EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        structure.Identifiers["task"].Should().Be("^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$");
    }

    private static Dictionary<string, string> Texts(JsonNode? node) =>
        (node ?? throw new InvalidOperationException("The effective manifest holds no such object.")).AsObject()
        .ToDictionary(static entry => entry.Key, static entry => entry.Value!.GetValue<string>(), StringComparer.Ordinal);
}
