using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Tool.Features.Evidence;

namespace Specht.Tests.Evidence;

/// <summary>
/// The normalized consumer policy decisions used by 0201-F5 B-008 to B-010, native claim input and its claim file set paths (B-028, B-035), the
/// malformed native claim keys and claim file set entries (B-042, B-043), Markdown claim input with no claims section (B-044) and the declared
/// projects that need an export (B-036).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class EvidencePolicyUnitTests
{
    [Fact]
    public void SelectedPolicy_WhenProjected_ShouldRequireTestByDefaultAndOptionalKindsOnlyWhenEnabled()
    {
        // Given
        var configuration = Configuration();
        configuration["codeAnalysis"]!["requiredLinks"]!["scenario"] = true;

        // When
        var policy = EvidencePolicy.Read(configuration);

        // Then
        policy.RequiredKinds.Should().ContainInOrder("test", "scenario");
        policy.RequiredKinds.Should().NotContain("implementation");
    }

    [Fact]
    public void ClaimExemption_WhenProjected_ShouldKeepItsExactReasonAndLinkKind()
    {
        // Given
        var configuration = Configuration();
        configuration["codeAnalysis"]!["exemptions"]!.AsArray().Add(new JsonObject
        {
            ["target"] = new JsonObject { ["feature"] = "cap:read", ["claim"] = "need/7" },
            ["links"] = new JsonArray("test"),
            ["reason"] = "A reviewed exception.",
        });

        // When
        var policy = EvidencePolicy.Read(configuration);

        // Then
        policy.Exemption(null, "cap:read", "need/7", "test").Should().Be("A reviewed exception.");
        policy.Exemption(null, "cap:read", "need/7", "scenario").Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ExemptionWithoutReason_WhenProjected_ShouldBeRejected(string reason)
    {
        // Given
        var configuration = Configuration();
        configuration["codeAnalysis"]!["exemptions"]!.AsArray().Add(new JsonObject
        {
            ["target"] = new JsonObject { ["feature"] = "cap:read", ["claim"] = "need/7" },
            ["links"] = new JsonArray("test"),
            ["reason"] = reason,
        });

        // When
        var read = () => EvidencePolicy.Read(configuration);

        // Then
        read.Should().Throw<EvidenceConfigurationException>();
    }

    [Theory]
    [ClassData(typeof(UnsafeProjectPaths))]
    public void AProjectPathThatIsNotRepositoryRelative_WhenProjected_ShouldBeRejected(string path)
    {
        // Given
        var configuration = Configuration();
        configuration["codeAnalysis"]!["projects"]![0]!["path"] = path;

        // When
        var read = () => EvidencePolicy.Read(configuration);

        // Then
        read.Should().Throw<EvidenceConfigurationException>();
    }

    [Theory]
    [ClassData(typeof(ExportRequirements))]
    public void ADeclaredProject_WhenProjected_ShouldRequireAnExportOnlyWithARequiredCaseScopeOrAVerifiesEntryNamingIt(
        string? requiredCaseScope, string? role, string? enforcedProject, bool requiresExport)
    {
        // Given
        var configuration = Configuration();
        if (requiredCaseScope is not null)
        {
            configuration["codeAnalysis"]!["projects"]![0]!["requiredCaseScope"] = requiredCaseScope;
        }

        if (role is not null || enforcedProject is not null)
        {
            configuration["codeAnalysis"]!["enforcement"] = new JsonArray(new JsonObject
            {
                ["project"] = enforcedProject,
                ["include"] = new JsonArray("test/**/*.cs"),
                ["exclude"] = new JsonArray(),
                ["role"] = role,
                ["declarationKinds"] = new JsonArray("testMethod"),
            });
        }

        // When
        var policy = EvidencePolicy.Read(configuration);

        // Then
        policy.Projects.Should().ContainSingle().Which.RequiresExport.Should().Be(requiresExport);
    }

    [Fact]
    public void ANativeClaimManifestWithNoMappedClaimsSection_WhenProjected_ShouldSelectItsFeatures()
    {
        // Given
        var configuration = JsonNode.Parse(NativeManifest)!.AsObject();

        // When
        var policy = EvidencePolicy.Read(configuration);

        // Then
        policy.Features.Should().Equal(new EvidencePolicy.FeatureEntry(null, "cap:read"));
    }

    [Theory]
    [ClassData(typeof(ClaimFileSetSpellings))]
    public void AClaimFileSetSpelledAsTheAnalyzerReadsIt_WhenProjected_ShouldHoldItsGlobAndRecordNormalised(string glob, string record)
    {
        // Given
        var configuration = JsonNode.Parse(NativeManifest)!.AsObject();
        configuration["codeAnalysis"]!["records"]!["claimFileSets"]![0]!["glob"] = glob;
        configuration["codeAnalysis"]!["records"]!["claimFileSets"]![0]!["record"] = record;

        // When
        var policy = EvidencePolicy.Read(configuration);

        // Then
        policy.ClaimFileSets.Should().Equal(new EvidencePolicy.ClaimFileSet("src/read/.spec/*.feature", "cap:read", null, "src/read/.spec/README.md"));
    }

    [Theory]
    [ClassData(typeof(UnsafeClaimFileSetPaths))]
    public void AClaimFileSetPathRootedEscapingOrNamingNothing_WhenProjected_ShouldBeRejected(string glob, string record)
    {
        // Given
        var configuration = JsonNode.Parse(NativeManifest)!.AsObject();
        configuration["codeAnalysis"]!["records"]!["claimFileSets"]![0]!["glob"] = glob;
        configuration["codeAnalysis"]!["records"]!["claimFileSets"]![0]!["record"] = record;

        // When
        var read = () => EvidencePolicy.Read(configuration);

        // Then
        read.Should().Throw<EvidenceConfigurationException>();
    }

    [Fact]
    public void AMarkdownClaimManifestWithNoMappedClaimsSection_WhenProjected_ShouldBeRejected()
    {
        // Given
        var configuration = Configuration();
        configuration["codeAnalysis"]!["mappings"]!.AsObject().Remove("/claimsSection");

        // When
        var read = () => EvidencePolicy.Read(configuration);

        // Then
        read.Should().Throw<EvidenceConfigurationException>();
    }

    [Fact]
    public void ANativeClaimManifestNamingEveryTagKey_WhenProjected_ShouldReadTheTagFormsAndTagListsItNames()
    {
        // Given
        var configuration = JsonNode.Parse(NativeManifest)!.AsObject();
        configuration["claimTag"]!["tagIgnoreCase"] = true;
        configuration["codeAnalysis"]!["scenarioScopes"] = new JsonObject { ["featureTag"] = "@feature:{feature}", ["epicTag"] = "@epic:{epic}" };
        configuration["codeAnalysis"]!["records"]!["plannedTags"] = new JsonArray("@wip");
        configuration["codeAnalysis"]!["records"]!["withdrawnTags"] = new JsonArray("@withdrawn");
        configuration["codeAnalysis"]!["records"]!["supersededTags"] = new JsonArray("@superseded");

        // When
        var tags = EvidencePolicy.Read(configuration).ClaimTags!;

        // Then
        tags.Claim.Should().Be(new GherkinClaimTagForm("@{claim}", "claim", true, "^need/[0-9]+$"));
        tags.FeatureScope.Should().Be(new GherkinClaimTagForm("@feature:{feature}", "feature", false, null));
        tags.EpicScope.Should().Be(new GherkinClaimTagForm("@epic:{epic}", "epic", false, null));
        tags.PlannedTags.Should().Equal("@wip");
        tags.WithdrawnTags.Should().Equal("@withdrawn");
        tags.SupersededTags.Should().Equal("@superseded");
    }

    [Fact]
    public void ANativeClaimManifestNamingNoOptionalTagKey_WhenProjected_ShouldReadACaseSensitiveClaimTagNoScopeTagsAndOnlyIgnoreAsPlanned()
    {
        // Given
        var configuration = JsonNode.Parse(NativeManifest)!.AsObject();
        configuration["claimTag"]!.AsObject().Remove("tagIgnoreCase");

        // When
        var tags = EvidencePolicy.Read(configuration).ClaimTags!;

        // Then
        tags.Claim.IgnoreCase.Should().BeFalse();
        tags.FeatureScope.Should().BeNull();
        tags.EpicScope.Should().BeNull();
        tags.PlannedTags.Should().Equal("@ignore");
        tags.WithdrawnTags.Should().BeEmpty();
        tags.SupersededTags.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(MalformedNativeClaimKeys))]
    public void AMalformedNativeClaimKey_WhenProjected_ShouldBeRejectedAsInvalidConfiguration(string parent, string key, string? value)
    {
        // Given
        var configuration = JsonNode.Parse(NativeManifest)!.AsObject();
        var owner = parent.Split('/').Aggregate((JsonNode) configuration, static (node, name) => node[name]!).AsObject();
        if (value is null)
        {
            owner.Remove(key);
        }
        else
        {
            owner[key] = JsonNode.Parse(value);
        }

        // When
        var read = () => EvidencePolicy.Read(configuration);

        // Then
        read.Should().Throw<EvidenceConfigurationException>();
    }

    [Theory]
    [ClassData(typeof(AbsentClaimFileSets))]
    public void ANativeClaimManifestWithNoClaimFileSet_WhenProjected_ShouldBeRejectedAsInvalidConfiguration(string? sets)
    {
        // Given
        var configuration = JsonNode.Parse(NativeManifest)!.AsObject();
        var records = configuration["codeAnalysis"]!["records"]!.AsObject();
        records.Remove("claimFileSets");
        if (sets is not null)
        {
            records["claimFileSets"] = JsonNode.Parse(sets);
        }

        // When
        var read = () => EvidencePolicy.Read(configuration);

        // Then
        read.Should().Throw<EvidenceConfigurationException>();
    }

    [Theory]
    [ClassData(typeof(MalformedClaimFileSets))]
    public void AMalformedClaimFileSetEntry_WhenProjected_ShouldBeRejectedAsInvalidConfiguration(string sets, string? epicGrammar)
    {
        // Given
        var configuration = JsonNode.Parse(NativeManifest)!.AsObject();
        configuration["codeAnalysis"]!["records"]!["claimFileSets"] = JsonNode.Parse(sets);
        if (epicGrammar is not null)
        {
            configuration["idPatterns"]!["epic"] = epicGrammar;
            configuration["codeAnalysis"]!["features"]![0]!["epic"] = "0201";
        }

        // When
        var read = () => EvidencePolicy.Read(configuration);

        // Then
        read.Should().Throw<EvidenceConfigurationException>();
    }

    private static JsonObject Configuration() => JsonNode.Parse("""
        {
          "revision":"0.1.0",
          "idPatterns":{"feature":"^cap:[a-z]+$","claim":"^need/[0-9]+$"},
          "claimHeading":"3. Acceptance Criteria",
          "codeAnalysis":{
            "mappings":{"/schemaVersion":"/revision","/identifiers":"/idPatterns",
              "/claimsSection":"/claimHeading","/policy":"/codeAnalysis"},
            "projects":[{"path":"test/tests.csproj","targetFrameworks":["net10.0"]}],
            "features":[{"feature":"cap:read"}],
            "requiredLinks":{"test":true},
            "exemptions":[]
          }
        }
        """)!.AsObject();

    private const string NativeManifest = """
        {
          "revision":"0.1.0",
          "idPatterns":{"feature":"^cap:[a-z]+$","claim":"^need/[0-9]+$"},
          "claimTag":{"tag":"@{claim}","tagIgnoreCase":false},
          "codeAnalysis":{
            "mappings":{"/schemaVersion":"/revision","/identifiers":"/idPatterns","/claim":"/claimTag","/policy":"/codeAnalysis"},
            "projects":[{"path":"test/tests.csproj","targetFrameworks":["net10.0"]}],
            "features":[{"feature":"cap:read"}],
            "requiredLinks":{"test":true},
            "exemptions":[],
            "records":{
              "claimSource":"gherkin",
              "claimFileSets":[{"glob":"src/read/.spec/*.feature","feature":"cap:read","record":"src/read/.spec/README.md"}]
            }
          }
        }
        """;
}
