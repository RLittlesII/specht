using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Tool;

namespace Specht.Acceptance.Manifest;

/// <summary>
/// The tests, matrix, owners and required-specification keys and the five status markers at schema 0.2.0 (0001-F5 B-055 to
/// B-060). Each setting is written into the default manifest in memory; "the manifest is loaded" parses it with the engine's
/// parser, and "the run exits with the invalid-manifest code" launches the built tool on the root the check ran on.
/// </summary>
public sealed partial class ManifestSteps
{
    [Given("the manifest pins schema version {string}")]
    public void GivenTheManifestPinsSchemaVersion(string pin) => Manifest["schemaVersion"] = pin;

    [Given("the manifest declares the test files {string} and the declaration form {string}")]
    public void GivenTheManifestDeclaresTheTestFilesAndTheDeclarationForm(string files, string form) =>
        Manifest["tests"] = new JsonObject { ["files"] = new JsonArray(files), ["declarations"] = new JsonArray(form) };

    [Given("the manifest declares the test files {string} and no declaration forms")]
    [Given("the manifest declares the test files {string}")]
    public void GivenTheManifestDeclaresTheTestFiles(string files) => Manifest["tests"] = new JsonObject { ["files"] = new JsonArray(files) };

    [Given("the manifest says nothing of scenario title matching")]
    public void GivenTheManifestSaysNothingOfScenarioTitleMatching() => Manifest.Remove("matrix");

    [Given("the manifest turns scenario title matching on")]
    public void GivenTheManifestTurnsScenarioTitleMatchingOn() => Manifest["matrix"] = new JsonObject { ["scenarioTitles"] = true };

    [Given("the manifest names no covered, partial, waived, withdrawn or superseded marker")]
    public void GivenTheManifestNamesNoStatusMarker()
    {
        foreach (var marker in StatusMarkers)
        {
            Named("markers").Remove(marker);
        }
    }

    [Given("the manifest's {word} marker is {string}")]
    public void GivenTheManifestsMarkerIs(string marker, string text) => Named("markers")[marker] = text;

    [Given("the manifest names {word} as the owner of {string}")]
    public void GivenTheManifestNamesAsTheOwnerOf(string role, string title) => Named("owners")[title] = new JsonArray(role);

    [Given("the manifest's owners map {string} to an empty list")]
    public void GivenTheManifestsOwnersMapToAnEmptyList(string title) => Named("owners")[title] = new JsonArray();

    [Given("the manifest's owners map {string} to {word}")]
    public void GivenTheManifestsOwnersMapTo(string title, string role) => Named("owners")[title] = new JsonArray(role);

    [Given("the manifest names no owners")]
    public void GivenTheManifestNamesNoOwners() => Manifest.Remove("owners");

    [Given("the manifest names no required specification directories")]
    public void GivenTheManifestNamesNoRequiredSpecificationDirectories() => Manifest.Remove("requiredSpecifications");

    [When("the manifest is loaded")]
    public void WhenTheManifestIsLoaded() => _structure = EngineServices.Parser().Parse(Manifest.ToJsonString());

    [Then("the manifest rejection names {string}")]
    public void ThenTheManifestRejectionNames(string text) =>
        (_rejection ?? throw new InvalidOperationException("The manifest was accepted.")).Message.Should().Contain(text);

    [Then("the rejection names the tests key")]
    public void ThenTheRejectionNamesTheTestsKey() =>
        (_rejection ?? throw new InvalidOperationException("The manifest was accepted.")).Message.Should().Contain("'tests'");

    [Then("the run exits with the invalid-manifest code")]
    public void ThenTheRunExitsWithTheInvalidManifestCode()
    {
        _run = AcceptanceTool.Launch(Tree.Root, "--root", ".");
        Run.ExitCode.Should().Be(ExitCodes.InvalidManifest, Run.Stderr);
    }

    [Then("scenario title matching is {word}")]
    public void ThenScenarioTitleMatchingIs(string state) => Structure.MatrixScenarioTitles.Should().Be(state == "on");

    [Then("those markers are {string}, {string}, {string}, {string} and {string}")]
    public void ThenThoseMarkersAre(string covered, string partial, string waived, string withdrawn, string superseded) =>
        StatusMarkers.Select(marker => Structure.Markers[marker]).Should().Equal(covered, partial, waived, withdrawn, superseded);

    [Then("the {word} marker is {string}")]
    public void ThenTheMarkerIs(string marker, string text) => Structure.Markers[marker].Should().Be(text);

    [Then("no section has an owner")]
    public void ThenNoSectionHasAnOwner() => Structure.Owners.Should().BeEmpty();

    [Then("the required specification directories are an empty list")]
    public void ThenTheRequiredSpecificationDirectoriesAreAnEmptyList() => Structure.RequiredSpecifications.Should().BeEmpty();

    private SpecStructure Structure => _structure ?? throw new InvalidOperationException("No manifest was loaded.");

    private static readonly string[] StatusMarkers = ["covered", "partial", "waived", "withdrawn", "superseded"];

    private SpecStructure? _structure;
}
