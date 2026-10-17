using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>Versioned sign-off headers and per-role table fill (0001-F5 B-065 to B-067).</summary>
[Trait("Tier", "Unit")]
public sealed class SpecManifestParserSignOffTablesUnitTests
{
    [Theory]
    [ClassData(typeof(SignOffTableDefaultsData))]
    public void OmittedTables_WhenParsed_ShouldUseThePinsSignOffDefault(string pin, bool signOff)
    {
        // Given
        var parser = EngineServices.Parser();

        // When
        var structure = parser.Parse(new JsonObject { ["schemaVersion"] = pin }.ToJsonString());

        // Then
        structure.Tables.ContainsKey("signOff").Should().Be(signOff);
        if (signOff)
        {
            structure.Tables["signOff"].Should().Equal("Section", "Status", "Reviewer", "Note");
        }
    }

    [Theory]
    [ClassData(typeof(DeclaredTablesData))]
    public void DeclaredTableRoles_WhenParsed_ShouldPreserveWholeListsAndFillOmittedRolesOnlyFromSchema020(
        string pin,
        string tables,
        bool fill)
    {
        // Given
        var parser = EngineServices.Parser();
        var declared = JsonNode.Parse(tables)!.AsObject();
        var manifest = new JsonObject { ["schemaVersion"] = pin, ["tables"] = declared.DeepClone() };

        // When
        var structure = parser.Parse(manifest.ToJsonString());

        // Then
        structure.Tables.Keys.Should().BeEquivalentTo(fill ? new[] { "matrix", "signOff" } : declared.Select(static entry => entry.Key));
        foreach (var (role, headers) in declared)
        {
            structure.Tables[role].Should().Equal(headers!.AsArray().Select(static header => header!.GetValue<string>()));
        }

        if (fill && !declared.ContainsKey("signOff"))
        {
            structure.Tables["signOff"].Should().Equal("Section", "Status", "Reviewer", "Note");
        }

        if (fill && !declared.ContainsKey("matrix"))
        {
            structure.Tables["matrix"].Should().Equal("Claim ID", "Scenario", "Test", "Status");
        }
    }
}
