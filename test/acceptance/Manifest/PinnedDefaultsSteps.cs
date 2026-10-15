using System;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Resources;
using Specht.Versioning;

namespace Specht.Acceptance.Manifest;

/// <summary>Loads synthetic version defaults for the B-048 omission and override scenarios.</summary>
[Binding]
[Scope(Feature = "The manifest carries the roles", Tag = "B-048")]
public sealed class PinnedDefaultsSteps
{
    [Given("the tool ships two schema versions with different missing-test markers")]
    public void GivenDifferentMarkers()
    {
        _older["markers"]!["missing"] = "OlderMissing";
        _newer["markers"]!["missing"] = "NewerMissing";
    }

    [Given("the tool ships schema version {string} and a newer version with different defaults")]
    public void GivenAnOriginalVersion(string original)
    {
        original.Should().Be("0.1.0");
        GivenDifferentMarkers();
    }

    [Given("the tool ships two schema versions with different table defaults")]
    public void GivenDifferentTables()
    {
        _older["tables"] = new JsonObject { ["claims"] = new JsonArray("Old ID"), ["matrix"] = new JsonArray("Old test") };
        _newer["tables"] = new JsonObject { ["matrix"] = new JsonArray("New test") };
    }

    [Given("the tool ships two schema versions with different role titles")]
    public void GivenDifferentRoles()
    {
        foreach (var (defaults, label) in new[] { (_older, "Older"), (_newer, "Newer") })
        {
            defaults["sections"] = new JsonArray("3. Claims", $"9. {label} matrix", $"12. {label} sign-off");
            defaults["roles"] = new JsonObject
            {
                ["claims"] = "3. Claims",
                ["matrix"] = $"9. {label} matrix",
                ["signOff"] = $"12. {label} sign-off",
            };
        }
    }

    [Given("the manifest pins the older version and omits the missing-test marker")]
    public void GivenAnOlderPin() => _manifest["schemaVersion"] = "0.1.0";

    [Given("the manifest pins the older version and sets its missing-test marker to {string}")]
    public void GivenAnExplicitMarker(string marker)
    {
        GivenAnOlderPin();
        _manifest["markers"] = new JsonObject { ["missing"] = marker };
    }

    [Given("the manifest omits its schema version and missing-test marker")]
    public void GivenAnOmittedPin() => _manifest = new JsonObject();

    [Given("the manifest pins the older version and declares only its own matrix table headers")]
    public void GivenExplicitTables()
    {
        GivenAnOlderPin();
        _manifest["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Scenario", "Checks", "Status") };
    }

    [Given("the manifest pins the older version and renames only the claims role to {string}")]
    public void GivenAnExplicitRole(string title)
    {
        GivenAnOlderPin();
        _manifest["sections"] = _older["sections"]!.DeepClone();
        _manifest["sections"]![0] = title;
        _manifest["roles"] = new JsonObject { ["claims"] = title };
    }

    [Given("the older version's default sets the missing-test rule to warning")]
    public void GivenDefaultRuleLevels() => _older["rules"] = new JsonObject { ["SPEC060"] = "warning" };

    [Given("the manifest pins the older version and declares an empty rule map")]
    public void GivenEmptyRuleLevels()
    {
        GivenAnOlderPin();
        _manifest["rules"] = new JsonObject();
    }

    [When("the manifest is loaded")]
    public void WhenTheManifestIsLoaded()
    {
        var resources = new SpechtResources();
        var texts = resources.Texts.ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal);
        texts["manifest/v0.1.0/manifest.json"] = _older.ToJsonString();
        texts["manifest/v0.2.0/manifest.json"] = _newer.ToJsonString();
        var parser = new SpecManifestParser(new SchemaVersions(texts));
        _structure = parser.Parse(_manifest.ToJsonString());
    }

    [Then("its missing-test marker is the older version's marker")]
    public void ThenTheOlderMarkerIsUsed() => Structure.Markers["missing"].Should().Be("OlderMissing");

    [Then("its missing-test marker is version {string}'s marker")]
    public void ThenTheOriginalMarkerIsUsed(string original)
    {
        original.Should().Be("0.1.0");
        ThenTheOlderMarkerIsUsed();
    }

    [Then("its missing-test marker is {string}")]
    public void ThenTheMarkerIs(string marker) => Structure.Markers["missing"].Should().Be(marker);

    [Then("its schema version is {string}")]
    public void ThenThePinIs(string pin) => Structure.SchemaVersion.ToString().Should().Be(pin);

    [Then("its table map contains exactly the declared matrix entry and headers")]
    public void ThenOnlyDeclaredTablesAreUsed()
    {
        Structure.Tables.Should().ContainSingle().Which.Key.Should().Be("matrix");
        Structure.Tables["matrix"].Should().Equal("Claim ID", "Scenario", "Checks", "Status");
    }

    [Then("the claims role is {string}")]
    public void ThenTheRoleIs(string title) => Structure.Roles["claims"].Should().Be(title);

    [Then("the other roles use the older version's titles")]
    public void ThenOtherRolesUseOlderDefaults()
    {
        Structure.Roles["matrix"].Should().Be("9. Older matrix");
        Structure.Roles["signOff"].Should().Be("12. Older sign-off");
    }

    [Then("its rule map is empty")]
    public void ThenRulesAreNotInherited() => Structure.Rules.Should().BeEmpty();

    private SpecStructure Structure => _structure ?? throw new InvalidOperationException("No manifest was loaded.");

    private readonly JsonObject _older = JsonNode.Parse(new SpechtResources().DefaultManifest)!.AsObject();
    private readonly JsonObject _newer = JsonNode.Parse(new SpechtResources().DefaultManifest)!.AsObject();
    private JsonObject _manifest = new();
    private SpecStructure? _structure;
}
