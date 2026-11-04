using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Discovery;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Manifest;

/// <summary>
/// Unread names, blank values and annotations in the manifest (<c>0001-F5</c> B-072 to B-075; decision 0021). A white-space
/// marker is a single space under <c>markers</c> (B-073). A name no rule reads is written under <c>roles</c> with a title
/// the sections lack, which the engine would reject were the name read as a role, or under <c>markers</c> with a text of its
/// own or the empty text (B-072). An annotation is a <c>$</c>-prefixed member written beside the epic and Feature segments of
/// the <c>identity</c> of the <c>layouts</c> entry named <c>epics</c>, and that identity is compared, as the engine's parser
/// reads it, with the identity it reads once the annotation is removed (B-074).
/// </summary>
public sealed partial class ManifestSteps
{
    [Given("the manifest's {word} marker is white space alone")]
    public void GivenTheManifestsMarkerIsWhiteSpaceAlone(string marker) => Named("markers")[marker] = " ";

    [Given("the manifest's {word} carry the name {string}, which no rule reads")]
    public void GivenTheManifestsCarryTheNameWhichNoRuleReads(string key, string name) =>
        Named(key)[name] = key == "roles" ? "Glossary" : "Pending";

    [Given("the manifest's markers carry the name {string} with an empty text")]
    public void GivenTheManifestsMarkersCarryTheNameWithAnEmptyText(string name) => Named("markers")[name] = string.Empty;

    [Given("the manifest's epics layout declares its epic and Feature segments and an annotation {string}")]
    public void GivenTheManifestsEpicsLayoutDeclaresItsEpicAndFeatureSegmentsAndAnAnnotation(string annotation)
    {
        EpicsLayout()["identity"] = new JsonObject { ["epic"] = 1, ["feature"] = 2, [annotation] = "a note on the segments" };
        _annotation = annotation;
    }

    [Then("the epics layout's path identity is read as it is without the annotation")]
    public void ThenTheEpicsLayoutsPathIdentityIsReadAsItIsWithoutTheAnnotation()
    {
        var annotation = _annotation ?? throw new InvalidOperationException("No annotation was declared.");
        var without = Manifest.DeepClone().AsObject();
        without["layouts"]!.AsArray()
            .Select(static entry => entry!.AsObject())
            .Single(static entry => entry["name"]!.GetValue<string>() == "epics")["identity"]!.AsObject()
            .Remove(annotation);

        EpicsIdentity(Manifest).Should().Be(EpicsIdentity(without)).And.NotBeNull();
    }

    private static SpechtPathIdentity? EpicsIdentity(JsonObject manifest) =>
        EngineServices.Parser().Parse(manifest.ToJsonString()).Discovery.Layouts.Single(static layout => layout.Name == "epics").Identity;

    private string? _annotation;
}
