using System.IO.Abstractions;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Analysis.Bindings;
using Specht.Manifest;
using Specht.Model;
using Specht.Versioning;

namespace Specht.Analysis.Tests.Bindings;

[Trait("Tier", "Integration")]
public sealed class EngineTextCompatibilityIntegrationTests
{
    [Theory]
    [ClassData(typeof(FeatureTextLineEndingCases))]
    public void ScalarFrontmatter_WhenReadThroughBothComponents_ShouldPreserveTypedValuesAndSourceLines(string newline)
    {
        // Given
        var text = """
            ---
            id: 'F7'
            epic: 0042
            spec_status: "approved"
            created: 2026-02-03
            updated: '2026-02-04'
            enabled: true
            count: 27
            quoted_count: "27"
            missing: null
            children: [F8, F9]
            metadata:
              domain: delivery
            ---
            ## 3. Acceptance Criteria
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-007 | A claim | agreement | Active |
            """.Replace("\n", newline, StringComparison.Ordinal);
        using var directory = new BindingTextDirectory();
        var fileSystem = new FileSystem();
        var path = Path.Combine(directory.FullName, "README.md");
        File.WriteAllText(path, text);

        // When
        var frontmatter = new FrontmatterReader(fileSystem).Read(path);
        var document = SpecDocument.Parse(File.ReadAllText(path), "src/sample/.spec/README.md");
        var projected = new BindingFeatureReader().Read(text, "src/sample/.spec/README.md", new BindingPolicyFixture());

        // Then
        frontmatter.Text("id").Should().Be("F7");
        frontmatter.Text("epic").Should().Be("0042");
        frontmatter.Text("created").Should().Be("2026-02-03");
        frontmatter.Text("updated").Should().Be("2026-02-04");
        frontmatter.Node!["enabled"]!.GetValue<bool>().Should().BeTrue();
        frontmatter.Node["count"]!.GetValue<long>().Should().Be(27);
        frontmatter.Text("count").Should().BeNull();
        frontmatter.Text("quoted_count").Should().Be("27");
        frontmatter.Node["missing"].Should().BeNull();
        frontmatter.Node["children"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("F8", "F9");
        frontmatter.Node["metadata"]!["domain"]!.GetValue<string>().Should().Be("delivery");
        frontmatter.LineOf("id").Should().Be(2);
        frontmatter.LineOf("epic").Should().Be(3);
        frontmatter.LineOf("created").Should().Be(5);
        frontmatter.LineOf("metadata").Should().Be(12);
        JsonNode.DeepEquals(document.Frontmatter.Node, frontmatter.Node).Should().BeTrue();
        document.Section("3. Acceptance Criteria")!.RowLines.Should().Equal(18);
        projected.Identity.Should().Be(new FeatureIdentity("0042", "F7"));
        projected.State.Should().Be("approved");
        projected.RelativePath.Should().Be("src/sample/.spec/README.md");
        projected.Claims.Should().ContainSingle().Which.Should().Be(new BindingClaim("B-007", "Active", 18));
    }

    [Fact]
    public void ConfiguredClaimsSection_WhenProjected_ShouldRetainFirstTableVisibleIdsDuplicatesAndFourthCellLifecycle()
    {
        // Given
        var text = """
            ---
            id: F7
            epic: 0042
            spec_status: approved
            ---
            ## Other section
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-900 | Ignore this section | source | Active |
            > ## Promises
            > | ID | Claim | Source | Status |
            > | --- | --- | --- | --- |
            > | B-901 | Ignore nested heading | source | Active |
            ## Promises
            ### Details
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | `B-007` | Has **visible** and `code` text | source | **Active** |
            | [B-007](reference) | Duplicate identity | source | `Superseded` |
            | **B-008** | Another claim | source | Amended |

            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-902 | Ignore second table | source | Active |
            ## Next
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-903 | Ignore next section | source | Active |
            """;
        BindingPolicy policy = new BindingPolicyFixture().WithClaimsSection("Promises");
        using var directory = new BindingTextDirectory();
        var path = Path.Combine(directory.FullName, "README.md");
        File.WriteAllText(path, text);
        var structure = new SpecStructure([], new Dictionary<string, IReadOnlyList<string>>(), policy.Identifiers, new SemanticVersion(0, 5, 0))
        {
            Roles = new Dictionary<string, string> { ["claims"] = "Promises" },
        };

        // When
        var document = SpecDocument.Parse(File.ReadAllText(path), "src/sample/.spec/README.md");
        var engineClaims = new SpecClaims(document, structure, new Regex("^B-[0-9]{3}$"));
        var projected = new BindingFeatureReader().Read(text, "src/sample/.spec/README.md", policy);

        // Then
        document.Sections.Select(static section => section.Title).Should()
            .Equal("id: F7\nepic: 0042\nspec_status: approved", "Other section", "Promises", "Next");
        engineClaims.Rows.Select(static claim => claim.Id).Should().Equal("B-007", "B-007", "B-008");
        engineClaims.Rows.Select(static claim => claim.Line).Should().Equal(18, 19, 20);
        engineClaims.Section!.Rows[0].Should().Equal("B-007", "Has visible and code text", "source", "Active");
        engineClaims.Section.RowCodeSpans[0][0].Should().Equal("B-007");
        engineClaims.Section.Rows.Select(static row => row[3]).Should().Equal("Active", "Superseded", "Amended");
        projected.Claims.Should().Equal(new BindingClaim("B-007", "Active", 18),
            new BindingClaim("B-007", "Superseded", 19), new BindingClaim("B-008", "Amended", 20));
    }

    [Fact]
    public void NoEpicTextWithCustomMarkers_WhenResolved_ShouldIgnoreIncidentalEpicAndRetainRawLifecycle()
    {
        // Given
        var text = """
            ---
            id: F7
            epic: incidental-metadata
            spec_status: ratified
            ---
            ## Promises
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-007 | Current claim | agreement | Active |
            | B-008 | Retired claim | agreement | Removed |
            """;
        BindingPolicy policy = new BindingPolicyFixture().WithClaimsSection("Promises").WithDeclaresEpicGrammar(false)
            .WithMarkers(new Dictionary<string, string>
            {
                ["approved"] = "ratified",
                ["withdrawn"] = "Removed",
                ["superseded"] = "Replaced",
            });
        using var directory = new BindingTextDirectory();
        var path = Path.Combine(directory.FullName, "README.md");
        File.WriteAllText(path, text);
        var engineDocument = SpecDocument.Parse(File.ReadAllText(path), "src/sample/.spec/README.md");

        // When
        var projected = new BindingFeatureReader().Read(text, "src/sample/.spec/README.md", policy);
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new[] { projected });
        var resolver = new BindingResolver(catalog, policy);
        var current = resolver.Resolve(new ClaimReference(null, "F7", "B-007"));
        var retired = resolver.Resolve(new ClaimReference(null, "F7", "B-008"));

        // Then
        engineDocument.Frontmatter.Text("epic").Should().Be("incidental-metadata");
        engineDocument.Frontmatter.Text("spec_status").Should().Be("ratified");
        current.Status.Should().Be(BindingResolution.Resolved);
        current.IsEligible.Should().BeTrue();
        current.Target!.Epic.Should().BeNull();
        current.Target.FeatureState.Should().Be("ratified");
        current.Target.Line.Should().Be(9);
        retired.Status.Should().Be(BindingResolution.Resolved);
        retired.IsEligible.Should().BeFalse();
        retired.Target!.State.Should().Be("Removed");
        retired.Target.Line.Should().Be(10);
    }
}
