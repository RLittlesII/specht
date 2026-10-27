using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Manifest;
using Specht.Model;
using Specht.Report;
using Specht.Rules;
using Specht.Tests.Manifest;
using Specht.Tests.Shared;
using Specht.Tool;
using Specht.Tool.Features.Check;
using Specht.Versioning;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;


namespace Specht.Tests.Check;

/// <summary>
/// The command through Spectre's command tester (<c>0001-F2</c> B-001, B-002, B-003, B-004, B-005, B-006, B-007, B-008, B-009, B-013,
/// B-016; C-7; <c>0001-F5</c> B-022, B-023; <c>0001-F7</c> B-003, B-032, B-039, B-069 to B-073, B-077, B-080): over a runner returning a report
/// built in memory, and over the real runner and a synthetic tree on disk. The runner is the seam: the engine emits no
/// warning until <c>0001-F5</c>'s rule settings exist, so a warning-only report is reachable only in memory. The tester
/// captures stdout alone, so an input failure's stderr message is the acceptance tier's to pin.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class CheckCommandIntegrationTests
{
    /// <summary>Gets reports whose lines must come out one per violation, in the runner's order, unwrapped, then the summary.</summary>
    public static TheoryData<string, SpechtReport> Reports =>
        new()
        {
            { "no violation prints the summary alone", new SpechtReportFixture() },
            {
                "two violations keep the runner's order, and a violation with no line has no position",
                new SpechtReportFixture().WithViolations(
                    new SpecViolationFixture().WithFile("b/spec.md").WithLine(9),
                    new SpecViolationFixture().WithSeverity(SpecSeverity.Warning).WithLine(0))
            },
            {
                "a violation wider than the 80-column console is not wrapped",
                new SpechtReportFixture().WithViolations(
                    new SpecViolationFixture().WithFile(string.Join('/', Enumerable.Repeat("a-long-directory-name", 10)) + "/spec.md"))
            },
        };

    /// <summary>Gets a violation, and what its printed line must carry after its message.</summary>
    public static TheoryData<string, SpecViolation, string> Endings =>
        new()
        {
            {
                "a violation with an identifier ends with it in square brackets (B-001)",
                new SpecViolationFixture().WithIdentifier("B-002"),
                " [B-002]"
            },
            { "a violation with no identifier ends at its message (B-016)", new SpecViolationFixture().WithIdentifier(null), string.Empty },
        };

    /// <summary>Gets the severities a report carries, the arguments, and the exit code the claim states for them.</summary>
    public static TheoryData<SpecSeverity[], string[], int> Verdicts =>
        new()
        {
            { [SpecSeverity.Error], [], 1 },
            { [], [], 0 },
            { [SpecSeverity.Warning], [], 0 },
            { [SpecSeverity.Warning], ["--strict"], 1 },
        };

    /// <summary>Gets the arguments, and the root the runner must be handed for them.</summary>
    public static TheoryData<string[], string> Roots =>
        new()
        {
            { [], Directory.GetCurrentDirectory() },
            { ["--root", "some/where"], Path.GetFullPath("some/where") },
        };

    /// <summary>Gets input failures, each arranged on an otherwise clean tree, returning the root to name, and the claimed code.</summary>
    public static TheoryData<string, Func<SpecTree, string>, int> InputFailures =>
        new()
        {
            { "a root that does not exist (B-005)", static tree => Path.Combine(tree.Root, "no-such-root"), 2 },
            { "a root that is a file (B-005)", static tree => tree.WriteRaw("a-file", string.Empty), 2 },
            {
                "a root with no manifest (B-006)",
                static tree =>
                {
                    File.Delete(Path.Combine(tree.Root, SpecTree.ManifestPath));
                    return tree.Root;
                },
                2
            },
            {
                "a manifest at the retired manifest path and none at the manifest path (B-037)",
                static tree =>
                {
                    var retired = Path.Combine(tree.Root, SpecTree.RetiredManifestPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(retired)!);
                    File.Move(Path.Combine(tree.Root, SpecTree.ManifestPath), retired);
                    return tree.Root;
                },
                2
            },
            {
                "a manifest that is not well-formed JSON (B-007)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "{ \"sections\": ");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose top level is an array (B-007)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "[]");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose sections are not an array of strings (B-007)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "{ \"sections\": [1, 2] }");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest that is the JSON literal null (B-007)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "null");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose sections hold a null (B-007)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "{ \"sections\": [null] }");
                    return tree.Root;
                },
                3
            },
            {
                "a well-formed manifest carrying a key the engine does not know (0001-F5 B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "{ \"glossary\": \"synthetic\" }");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose claims role names a title its sections lack (0001-F5 B-015, B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "{ \"roles\": { \"claims\": \"3. Nowhere\" } }");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose table headers are keyed by a section title (0001-F5 B-039, B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "{ \"tables\": { \"9. Traceability Matrix\": [\"Claim ID\", \"Test\"] } }");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose missing marker is empty (0001-F5 B-040, B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "{ \"markers\": { \"missing\": \"\" } }");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose companion file list is empty (0001-F5 B-041, B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "{ \"companionFiles\": [] }");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose exclusion entry has a slash inside it and no leading slash (0001-F5 B-021, B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "{ \"exclusions\": [\"build/output\"] }");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest setting a rule id outside the pinned vocabulary (0001-F5 B-013, B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "{ \"rules\": { \"SPEC999\": \"warning\" } }");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest setting a rule id to a level outside the three (0001-F5 B-045, B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, "{ \"rules\": { \"SPEC010\": \"loud\" } }");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest pinning schema 0.1.0 and declaring tests, a key that version does not know (0001-F5 B-055, B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, """{ "schemaVersion": "0.1.0", "tests": { "files": ["test/**/*.cs"] } }""");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose declaration form holds no test placeholder (0001-F5 B-056, B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(
                        SpecTree.ManifestPath,
                        """{ "schemaVersion": "0.2.0", "tests": { "files": ["test/**/*.cs"], "declarations": ["void Test("] } }""");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose owners name a title its sections lack (0001-F5 B-059, B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, """{ "schemaVersion": "0.2.0", "owners": { "3. Claims": ["spec-author"] } }""");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose owners give a section an empty list of roles (0001-F5 B-059, B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(SpecTree.ManifestPath, """{ "schemaVersion": "0.2.0", "owners": { "3. Acceptance Criteria": [] } }""");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose layout declares a path identity that is not a segment position (0001-F5 B-043, B-022, B-023)",
                static tree =>
                {
                    tree.WriteRaw(
                        SpecTree.ManifestPath,
                        """{ "layouts": [{ "name": "epics", "glob": "epics/**/spec.md", "identity": { "epic": "second", "feature": 2 } }] }""");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest pinning a schema version the tool does not ship (0001-F7 B-003)",
                static tree =>
                {
                    var path = Path.Combine(tree.Root, SpecTree.ManifestPath);
                    var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
                    var newest = EngineServices.Versions().Versions.Max(static version => version.Number);
                    manifest["schemaVersion"] = new SemanticVersion(newest.Major + 1, 0, 0).ToString();
                    File.WriteAllText(path, manifest.ToJsonString());
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose schema version is the integer 1, not major.minor.patch (0001-F7 B-039)",
                static tree =>
                {
                    var path = Path.Combine(tree.Root, SpecTree.ManifestPath);
                    var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
                    manifest["schemaVersion"] = 1;
                    File.WriteAllText(path, manifest.ToJsonString());
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose upstream schema source lacks its version (0001-F7 B-069)",
                static tree => RecordingAnUpstreamSchemaSource(tree, static source => source.Remove("version")),
                3
            },
            {
                "a manifest whose upstream schema source is at an address that is not https (0001-F7 B-070)",
                static tree => RecordingAnUpstreamSchemaSource(tree, static source => source["location"] = "http://schemas.example/specht/"),
                3
            },
            {
                "a manifest whose upstream schema source has an empty version (0001-F7 B-071)",
                static tree => RecordingAnUpstreamSchemaSource(tree, static source => source["version"] = string.Empty),
                3
            },
            {
                "a manifest whose upstream schema source holds no hash for the item kind (0001-F7 B-072)",
                static tree => RecordingAnUpstreamSchemaSource(tree, static source => source["hashes"]!.AsObject().Remove("task")),
                3
            },
            {
                "a manifest whose upstream schema source holds a hash written without its algorithm (0001-F7 B-073)",
                static tree => RecordingAnUpstreamSchemaSource(tree, static source => source["hashes"]!["task"] = new string('b', 64)),
                3
            },
            {
                "an upstream schema source recorded and the item schema's file absent under the root (0001-F7 B-077)",
                static tree =>
                {
                    RecordingAnUpstreamSchemaSource(tree, static _ => { });
                    File.Delete(Path.Combine(tree.Root, SpecTree.ItemSchemaPath));
                    return tree.Root;
                },
                3
            },
            {
                "an upstream schema source recorded and the item schema's file edited after its hash was recorded (0001-F7 B-080)",
                static tree =>
                {
                    RecordingAnUpstreamSchemaSource(tree, static _ => { });
                    File.AppendAllText(Path.Combine(tree.Root, SpecTree.ItemSchemaPath), "\n");
                    return tree.Root;
                },
                3
            },
        };

    [Theory]
    [MemberData(nameof(Reports))]
    public void AReport_WhenChecked_ShouldPrintOneUnwrappedLinePerViolationInTheRunnersOrderThenTheSummary(string because, SpechtReport report)
    {
        // Given
        var expected = string.Join(
            '\n',
            report.Violations.Select(static violation => violation.ToString()).Concat(SpecReportDocument.From(report).SummaryLines()));

        // When
        var result = Check(_ => report);

        // Then
        result.Output.Should().Be(expected, because);
    }

    [Theory]
    [MemberData(nameof(Endings))]
    public void AViolation_WhenChecked_ShouldPrintItsLineEndingWithItsIdentifierInSquareBracketsOrAtItsMessage(
        string because,
        SpecViolation violation,
        string ending)
    {
        // Given
        SpechtReport report = new SpechtReportFixture().WithViolations(violation);

        // When
        var result = Check(_ => report);

        // Then
        result.Output.Split('\n')[0].Should().EndWith(violation.Message + ending, because);
    }

    [Theory]
    [MemberData(nameof(Verdicts))]
    public void AReportOfSeverities_WhenChecked_ShouldExitWithTheClaimedCode(SpecSeverity[] severities, string[] args, int expected)
    {
        // Given
        SpechtReport report = new SpechtReportFixture().WithViolations(
            [.. severities.Select(static severity => (SpecViolation)new SpecViolationFixture().WithSeverity(severity))]);

        // When
        var result = Check(_ => report, args);

        // Then
        result.ExitCode.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(Roots))]
    public void ARootOption_WhenChecked_ShouldRunTheRootItResolvesTo(string[] args, string expected)
    {
        // Given
        string? checkedRoot = null;

        // When
        Check(
            root =>
            {
                checkedRoot = root;
                return new SpechtReportFixture();
            },
            args);

        // Then
        checkedRoot.Should().Be(expected);
    }

    [Fact]
    public void ATreeWithViolations_WhenChecked_ShouldPrintTheRunnersViolationsAsRootRelativeLines()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });
        tree.WriteFeature("0001", "F2", sections: SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims));
        var report = tree.Run();
        var violations = report.Violations.Select(static violation => violation.ToString()).ToArray();

        // When
        var result = Check(EngineServices.Runner().Run, "--root", tree.Root);

        // Then
        violations.Should().HaveCountGreaterThan(1);
        result.Output.Should().Be(string.Join('\n', violations.Concat(SpecReportDocument.From(report).SummaryLines())));
        result.Output.Should().NotContain(tree.Root).And.NotContain("\\");
        result.ExitCode.Should().Be(1);
    }

    [Fact]
    public void ACleanTree_WhenChecked_ShouldPrintOnlyTheSummaryAndExitZero()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        var report = tree.Run();

        // When
        var result = Check(EngineServices.Runner().Run, "--root", tree.Root);

        // Then
        report.Violations.Should().BeEmpty();
        result.Output.Should().Be(string.Join('\n', SpecReportDocument.From(report).SummaryLines()));
        result.ExitCode.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(Reports))]
    public void AReport_WhenCheckedWithJson_ShouldPrintOnlyItsDocument(string because, SpechtReport report)
    {
        // Given
        var expected = SpecReportDocument.From(report).ToJson();

        // When
        var result = Check(_ => report, "--json");

        // Then
        result.Output.Should().Be(expected, because);
        result.Invoking(static parsed => JsonDocument.Parse(parsed.Output).Dispose()).Should().NotThrow(because);
    }

    [Fact]
    public void ATreeWithViolations_WhenCheckedWithJson_ShouldPrintTheDocumentAndNoLine()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });
        tree.WriteFeature("0001", "F2", sections: SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims));
        var report = tree.Run();
        var document = SpecReportDocument.From(report);

        // When
        var result = Check(EngineServices.Runner().Run, "--root", tree.Root, "--json");

        // Then
        report.Violations.Should().HaveCountGreaterThan(1);
        result.Output.Should().Be(document.ToJson());
        foreach (var line in report.Violations.Select(static violation => violation.ToString()).Concat(document.SummaryLines()))
        {
            result.Output.Should().NotContain(line);
        }
    }

    [Theory]
    [MemberData(nameof(InputFailures))]
    public void AnInputFailure_WhenChecked_ShouldPrintNothingOnStdoutAndExitWithTheClaimedCode(
        string because,
        Func<SpecTree, string> arrange,
        int expected)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        var root = arrange(tree);

        // When
        var result = Check(EngineServices.Runner().Run, "--root", root);

        // Then
        result.Output.Should().BeEmpty(because);
        result.ExitCode.Should().Be(expected, because);
    }

    [Fact]
    public void ATreeRecordingAnUpstreamSchemaSourceItsLocalCopyMatches_WhenCheckedWithJson_ShouldExitZeroAndSayTheSchemasCameFromTheUpstreamCopy()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        RecordingAnUpstreamSchemaSource(tree, static _ => { });

        // When
        var result = Check(EngineServices.Runner().Run, "--root", tree.Root, "--json");

        // Then
        result.ExitCode.Should().Be(0);
        JsonNode.Parse(result.Output)!["schemaSource"]!.GetValue<string>().Should().Be("upstream");
    }

    [Fact]
    public void AHelpRequest_WhenChecked_ShouldNameTheRootAndStrictOptionsAndExitZero()
    {
        // Given
        SpechtReport report = new SpechtReportFixture();

        // When
        var result = Check(_ => report, "--help");

        // Then
        result.Output.Should().Contain("--root").And.Contain("--strict");
        result.ExitCode.Should().Be(0);
    }

    [Theory]
    [InlineData(null, null, 1)]
    [InlineData("0.1.0", null, 1)]
    [InlineData("0.2.0", null, 0)]
    [InlineData("0.2.0", "OlderMissing", 1)]
    public void APinnedMissingTestDefault_WhenChecked_ShouldDetermineTheVerdictWithoutInheritingRuleLevels(
        string? pin, string? explicitMarker, int expectedExit)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "approved" },
            SpecTree.SectionsWith("9. Traceability Matrix",
                "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| --- | --- | --- | --- |\n"
                + "| B-001 | It does the thing | OlderMissing | Covered |\n")
                .Select(static section => section.Replace("🟡", "🟢", StringComparison.Ordinal)).ToArray());
        var path = Path.Combine(tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest.Remove("schemaVersion");
        if (pin is not null)
        {
            manifest["schemaVersion"] = pin;
        }

        manifest.Remove("rules");
        manifest["markers"]!.AsObject().Remove("missing");
        if (explicitMarker is not null)
        {
            manifest["markers"]!["missing"] = explicitMarker;
        }

        File.WriteAllText(path, manifest.ToJsonString());
        var data = new PinnedDefaultsData();

        // When
        var result = Check(EngineServices.Runner(data.Versions).Run, "--root", tree.Root);

        // Then
        result.ExitCode.Should().Be(expectedExit);
        if (expectedExit == 1)
        {
            result.Output.Should().Contain(": error SPEC060:");
        }
        else
        {
            result.Output.Should().NotContain("SPEC060");
        }
    }

    [Theory]
    [ClassData(typeof(ReadOnlyCheckData))]
    public void ACheckWithoutAReportPath_WhenRun_ShouldLeaveEveryFileUnderTheRootUnchanged(string input, int expectedExit)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        tree.WriteRaw(".hidden/keep.bin", "unchanged hidden content");
        tree.WriteRaw("unrelated.txt", "unchanged unrelated content");
        if (input == "violation")
        {
            tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });
        }
        else if (input == "missing manifest")
        {
            File.Delete(Path.Combine(tree.Root, SpecTree.ManifestPath));
        }
        else if (input == "invalid manifest")
        {
            tree.WriteRaw(SpecTree.ManifestPath, "{");
        }

        var before = Snapshot();

        // When
        var result = Check(EngineServices.Runner().Run, "--root", tree.Root);

        // Then
        result.ExitCode.Should().Be(expectedExit);
        Snapshot().Should().BeEquivalentTo(before);

        Dictionary<string, string> Snapshot() => Directory.EnumerateFiles(tree.Root, "*",
                new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = 0 })
            .ToDictionary(file => Path.GetRelativePath(tree.Root, file).Replace(Path.DirectorySeparatorChar, '/'),
                static file => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))), StringComparer.Ordinal);
    }

    [Fact]
    public void ARuleThatThrows_WhenCheckedWithoutStrict_ShouldReportItsFailureAndExitOne()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        tree.WriteRaw(SpecTree.ManifestPath, """{ "rules": { "SPEC030": "warning" } }""");

        // When
        var result = Check(static root => SpechtRunner.Evaluate(EngineServices.Loader().Load(root),
            [new FaultIsolatedRule(new ThrowingRule())]), "--root", tree.Root);

        // Then
        result.ExitCode.Should().Be(1);
        result.Output.Should().Contain($"{SpecTree.ManifestPath}: error SPEC030:").And.Contain(nameof(FormatException));
        result.Output.Should().NotContain("sensitive detail").And.NotContain("Exception:");
    }

    private static CommandAppResult Check(Func<string, SpechtReport> run, params string[] args)
    {
        var services = new ServiceCollection().AddSpechtEngine();
        services.AddSingleton<Func<string, SchemaSourceSelection?, SpechtReport>>((root, _) => run(root));

        var app = new CommandAppTester(new TypeRegistrar(services), console: new TestConsole().Width(80));
        app.SetDefaultCommand<CheckCommand>();

        return app.Run(args);
    }

    private static string RecordingAnUpstreamSchemaSource(SpecTree tree, Action<JsonObject> fault)
    {
        var path = Path.Combine(tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var source = new JsonObject
        {
            ["location"] = "https://schemas.example/specht/",
            ["version"] = "2026.10",
            ["hashes"] = new JsonObject
            {
                ["feature"] = HashOf(tree, SpecTree.FeatureSchemaPath),
                ["task"] = HashOf(tree, SpecTree.ItemSchemaPath),
                ["epic"] = HashOf(tree, SpecTree.EpicSchemaPath),
            },
        };
        fault(source);
        manifest["spechtSchema"] = source;
        File.WriteAllText(path, manifest.ToJsonString());

        return tree.Root;
    }

    private static string HashOf(SpecTree tree, string schemaPath) =>
        "sha256:" + Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(tree.Root, schemaPath))));

    private sealed class ThrowingRule : ISpecRule
    {
        public string Id => "SPEC030";
        public IReadOnlyList<string> ReportedIds => [Id];
        public IEnumerable<SpecViolation> Evaluate(SpecModel model) => throw new FormatException("sensitive detail");
    }

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";
}
