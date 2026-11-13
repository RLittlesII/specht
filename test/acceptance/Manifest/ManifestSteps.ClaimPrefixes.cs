using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Evaluation;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Manifest;

/// <summary>
/// Steps for a Feature's own claim prefix (<c>0001-F5</c> B-004, B-037, B-049, B-076 to B-080; decision 0022). A schema
/// supporting Feature claim prefixes is the pin <c>0.3.1</c> (A-7). CI, Release and the other Feature are co-located
/// specifications under <c>ci</c>,
/// <c>release</c> and <c>other</c>, each rewritten whenever a step changes it: its frontmatter carries the prefix under the
/// key the step names, its claims section the claims in order, its matrix the cited ids, or else one row per claim, and
/// its companion one scenario per tag, or else one tagged with each claim. "The claim ID check" is the rule the pin
/// reports as <c>SPEC4003</c>. Where the manifest and the selected Feature schema name the prefix key, the root's Feature
/// schema is the one read, and its prefix property is renamed, or added as non-blank text when it has none.
/// Activity, under <c>activity</c>, is the Feature the schema 0.4.0 scenarios name (item 0436); where it is written, the
/// steps that read one Feature's claims, matrix or declaration line read Activity, and CI otherwise.
/// </summary>
public sealed partial class ManifestSteps
{
    [Given("the schema supports Feature claim prefixes")]
    public void GivenTheSchemaSupportsFeatureClaimPrefixes() => Manifest["schemaVersion"] = ClaimPrefixPin;

    [Given("the repository uses its default claim grammar")]
    public void GivenTheRepositoryUsesItsDefaultClaimGrammar() => Named("identifiers").Remove("claim");

    [Given("the repository's claim-ID regex is {string}")]
    public void GivenTheRepositorysClaimIdRegexIs(string grammar) => Named("identifiers")["claim"] = grammar;

    [Given("the manifest uses all shipped defaults")]
    public void GivenTheManifestUsesAllShippedDefaults()
    {
        foreach (var key in Manifest.Select(static entry => entry.Key).Where(static key => !key.StartsWith('$') && key != "schemaVersion").ToList())
        {
            Manifest.Remove(key);
        }
    }

    [Given("the manifest and the selected Feature schema name the claim prefix key {string}")]
    public void GivenTheManifestAndTheSelectedFeatureSchemaNameTheClaimPrefixKey(string key)
    {
        KeysOf("feature")["claimPrefix"] = key;
        _fromRoot = true;
        EditSchema(SpechtTree.FeatureSchemaPath, schema =>
        {
            var properties = schema["properties"]!.AsObject();
            var property = properties[ClaimPrefixKey]?.DeepClone() ?? new JsonObject { ["type"] = "string", ["pattern"] = "\\S" };
            properties.Remove(ClaimPrefixKey);
            properties[key] = property;
        });
    }

    [Given("the root's Feature schema permits a text field named {string}")]
    public void GivenTheRootsFeatureSchemaPermitsATextFieldNamed(string key) =>
        EditSchema(SpechtTree.FeatureSchemaPath, schema => schema["properties"]![key] = new JsonObject { ["type"] = "string" });

    [Given("^(CI|Release|Activity) declares (\\w+) \"(.*)\" and claim \"(.*)\"$")]
    public void GivenAFeatureDeclaresAPrefixAndAClaim(string name, string key, string prefix, string claim)
    {
        var feature = Prefixed(name);
        feature.Frontmatter[key] = Quoted(prefix);
        feature.Claims = [claim];
        WritePrefixed(feature);
    }

    [Given("CI declares claim_prefix {string} and claims {string}, {string} and {string}")]
    public void GivenCiDeclaresAPrefixAndThreeClaims(string prefix, string first, string second, string third)
    {
        var feature = Prefixed("CI");
        feature.Frontmatter[ClaimPrefixKey] = Quoted(prefix);
        feature.Claims = [first, second, third];
        WritePrefixed(feature);
    }

    [Given("CI declares claim_prefix {string}")]
    public void GivenCiDeclaresAPrefix(string prefix)
    {
        var feature = Prefixed("CI");
        feature.Frontmatter[ClaimPrefixKey] = Quoted(prefix);
        WritePrefixed(feature);
    }

    [Given("Release omits the prefix")]
    public void GivenReleaseOmitsThePrefix() => WritePrefixed(Prefixed("Release"));

    [Given("another Feature declares no prefix and has claims {string} and {string}")]
    public void GivenAnotherFeatureDeclaresNoPrefixAndHasClaims(string first, string second)
    {
        var feature = Prefixed("Other");
        feature.Claims = [first, second];
        WritePrefixed(feature);
    }

    [Given("the CI Feature frontmatter contains the line {string}")]
    public void GivenTheCiFeatureFrontmatterContainsTheLine(string line)
    {
        var separator = line.IndexOf(": ", StringComparison.Ordinal);
        var feature = Prefixed("CI");
        feature.Frontmatter[line[..separator]] = line[(separator + 2)..];
        _declaredKey = line[..separator];
        WritePrefixed(feature);
    }

    [Given("CI declares the following claims:")]
    public void GivenCiDeclaresTheFollowingClaims(DataTable claims)
    {
        var feature = Prefixed("CI");
        feature.Claims = [.. claims.Rows.Select(static row => row["Claim ID"])];
        WritePrefixed(feature);
    }

    [Given("each Feature declares {string}, {string} and {string}")]
    public void GivenEachFeatureDeclares(string first, string second, string third)
    {
        foreach (var feature in _prefixed.Values)
        {
            feature.Claims = [first, second, third];
            WritePrefixed(feature);
        }
    }

    [Given("^(CI|Activity)'s matrix cites \"(.*)\" and \"(.*)\"$")]
    public void GivenCisMatrixCites(string name, string first, string second)
    {
        var feature = Prefixed(name);
        feature.Matrix = [first, second];
        WritePrefixed(feature);
    }

    [Given("CI's companion tags its scenarios with {string} and {string}")]
    [Given("its companion tags scenarios with {string} and {string}")]
    public void GivenCisCompanionTagsItsScenariosWith(string first, string second) => GivenAFeaturesCompanionTagsItsScenariosWith("CI", first, second);

    [Given("^(Activity)'s companion tags its scenarios with \"(.*)\" and \"(.*)\"$")]
    public void GivenAFeaturesCompanionTagsItsScenariosWith(string name, string first, string second)
    {
        var feature = Prefixed(name);
        feature.Tags = [first, second];
        WritePrefixed(feature);
    }

    [Given("^(CI|Release)'s matrix and companion cite \"(.*)\"$")]
    public void GivenAFeaturesMatrixAndCompanionCite(string name, string claim)
    {
        var feature = Prefixed(name);
        feature.Matrix = [claim];
        feature.Tags = ["@" + claim];
        WritePrefixed(feature);
    }

    [When("CI also declares {string}")]
    public void WhenCiAlsoDeclares(string claim)
    {
        var feature = Prefixed("CI");
        feature.Claims = [.. feature.Claims, claim];
        WritePrefixed(feature);
    }

    [Then("both Features' claims pass the claim ID check")]
    public void ThenBothFeaturesClaimsPassTheClaimIdCheck()
    {
        ClaimIdViolations("CI").Should().BeEmpty();
        ClaimIdViolations("Release").Should().BeEmpty();
    }

    [Then("Release's claim still passes the claim ID check")]
    public void ThenReleasesClaimStillPassesTheClaimIdCheck() => ClaimIdViolations("Release").Should().BeEmpty();

    [Then("the CI claim passes the claim ID check")]
    public void ThenTheCiClaimPassesTheClaimIdCheck() => ClaimIdViolations("CI").Should().BeEmpty();

    [Then("SPEC4003 reports the new CI claim at its declaration, naming {string} and the required prefix {string}")]
    [Then("SPEC4003 reports the CI claim at its declaration, naming {string} and the required prefix {string}")]
    [Then("SPEC4003 reports the claim at its declaration, naming {string} and the required prefix {string}")]
    public void ThenSpec4003ReportsTheCiClaimAtItsDeclaration(string claim, string prefix)
    {
        var feature = Prefixed(Focus);
        var lines = File.ReadAllLines(Path.Combine(Tree.Root, feature.Path));
        var line = Array.FindLastIndex(lines, text => text.StartsWith($"| {claim} | It holds. |", StringComparison.Ordinal)) + 1;
        var violation = ClaimIdViolations(Focus).Should().ContainSingle(violation => violation.Identifier == claim).Subject;
        violation.RuleId.Should().Be(RuleIds.Named(Manifest["schemaVersion"]?.GetValue<string>(), "SPEC4003"));
        violation.Line.Should().Be(line);
        violation.Message.Should().Contain($"'{claim}'").And.Contain($"'{prefix}'");
    }

    [Then("the claim ID check reports {string} and {string} only")]
    public void ThenTheClaimIdCheckReportsOnly(string first, string second)
    {
        _rejection.Should().BeNull();
        _report!.Violations.Where(violation => violation.RuleId == RuleIds.Under(Manifest, "SPEC030"))
            .Select(static violation => violation.Identifier)
            .Should()
            .BeEquivalentTo([first, second]);
    }

    [Then("the claim ID check reports only {string} in the Feature without a prefix")]
    public void ThenTheClaimIdCheckReportsOnlyInTheFeatureWithoutAPrefix(string claim) =>
        ClaimIdViolations("Other").Select(static violation => violation.Identifier).Should().Equal(claim);

    [Then("SPEC1002 reports the invalid claim prefix in the CI specification")]
    public void ThenSpec1002ReportsTheInvalidClaimPrefixInTheCiSpecification()
    {
        _rejection.Should().BeNull();
        var key = _declaredKey ?? throw new InvalidOperationException("No frontmatter line was written.");
        var lines = File.ReadAllLines(Path.Combine(Tree.Root, Prefixed("CI").Path));
        var line = Array.FindIndex(lines, text => text.StartsWith($"{key}:", StringComparison.Ordinal)) + 1;
        _report!.Violations.Should().Contain(violation =>
            violation.RuleId == RuleIds.Named(Manifest["schemaVersion"]!.GetValue<string>(), "SPEC1002")
            && violation.File == Prefixed("CI").Path
            && violation.Identifier == key
            && violation.Line == line);
    }

    [Then("the claim ID check accepts only {string} and {string}")]
    public void ThenTheClaimIdCheckAcceptsOnly(string first, string second) =>
        ClaimIdViolations(Focus).Select(static violation => violation.Identifier)
            .Should()
            .Equal(Prefixed(Focus).Claims.Where(claim => claim != first && claim != second));

    [Then("the only unresolved matrix reference is {string}")]
    public void ThenTheOnlyUnresolvedMatrixReferenceIs(string claim)
    {
        _rejection.Should().BeNull();
        _report!.Violations.Where(violation => violation.RuleId == RuleIds.Under(Manifest, "SPEC031"))
            .Select(static violation => (violation.File, violation.Identifier))
            .Should()
            .Equal((Prefixed(Focus).Path, claim));
    }

    [Then("the only unresolved claim tag is {string}, with identifier {string}")]
    public void ThenTheOnlyUnresolvedClaimTagIs(string tag, string identifier) => ThenTheUnknownClaimTag(tag, identifier);

    [Then("the only well-formed CI claim is {string}")]
    public void ThenTheOnlyWellFormedCiClaimIs(string claim) => WellFormed("CI").Should().Equal(claim);

    [Then("the well-formed Release claims are {string} and {string}")]
    public void ThenTheWellFormedReleaseClaimsAre(string first, string second) => WellFormed("Release").Should().Equal(first, second);

    [Then("both Features' matrix references and companion claim tags resolve")]
    public void ThenBothFeaturesMatrixReferencesAndCompanionClaimTagsResolve()
    {
        _rejection.Should().BeNull();
        foreach (var feature in _prefixed.Values)
        {
            _report!.Violations.Should().NotContain(violation =>
                violation.RuleId == RuleIds.Under(Manifest, "SPEC031")
                && violation.File == feature.Path
                && (feature.Matrix ?? feature.Claims).Contains(violation.Identifier));
        }

        _report!.Violations.Should().NotContain(violation => violation.RuleId == RuleIds.Under(Manifest, "SPEC021"));
    }

    private string Focus => _prefixed.ContainsKey("Activity") ? "Activity" : "CI";

    private static string Quoted(string text) =>
        "\"" + text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

    private IEnumerable<SpechtViolation> ClaimIdViolations(string name)
    {
        _rejection.Should().BeNull();
        var path = Prefixed(name).Path;
        return _report!.Violations.Where(violation => violation.RuleId == RuleIds.Under(Manifest, "SPEC030") && violation.File == path);
    }

    private IEnumerable<string> WellFormed(string name)
    {
        var reported = ClaimIdViolations(name).Select(static violation => violation.Identifier).ToHashSet(StringComparer.Ordinal);
        return Prefixed(name).Claims.Where(claim => !reported.Contains(claim));
    }

    private PrefixedFeature Prefixed(string name)
    {
        if (!_prefixed.TryGetValue(name, out var feature))
        {
            feature = new PrefixedFeature(name.ToLowerInvariant(), $"F{_prefixed.Count + 1}");
            _prefixed[name] = feature;
        }

        return feature;
    }

    private void WritePrefixed(PrefixedFeature feature)
    {
        var claims = "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + string.Concat(feature.Claims.Select(static claim => $"| {claim} | It holds. | owner | Active |\n"));
        var matrix = "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
            + string.Concat((feature.Matrix ?? feature.Claims).Select(static claim => $"| {claim} | It holds | Missing | Missing |\n"));
        var sections = SpechtTree.SectionsWith(DefaultTitles["claims"], claims).ToList();
        sections[sections.FindIndex(static section => section.StartsWith($"## {DefaultTitles["matrix"]}\n", StringComparison.Ordinal))] = matrix;
        var path = Tree.WriteCoLocatedFeature(feature.Area, "0001", feature.Id, feature.Frontmatter, sections);
        Tree.WriteFeatureFile(
            path,
            "Feature: it\n\n" + string.Concat((feature.Tags ?? [.. feature.Claims.Select(static claim => "@" + claim)])
                .Select(static (tag, index) => $"  {tag}\n  Scenario: scenario {index + 1}\n    Given a thing\n\n")));
        feature.Path = Relative(path);
    }

    private sealed class PrefixedFeature(string area, string id)
    {
        public string Area { get; } = area;

        public string Id { get; } = id;

        public Dictionary<string, string> Frontmatter { get; } = new(StringComparer.Ordinal);

        public IReadOnlyList<string> Claims { get; set; } = [];

        public IReadOnlyList<string>? Matrix { get; set; }

        public IReadOnlyList<string>? Tags { get; set; }

        public string Path { get; set; } = string.Empty;
    }

    private const string ClaimPrefixPin = "0.3.1";

    private const string ClaimPrefixKey = "claim_prefix";

    private readonly Dictionary<string, PrefixedFeature> _prefixed = new(StringComparer.Ordinal);
    private string? _declaredKey;
}
