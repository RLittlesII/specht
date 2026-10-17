using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Manifest;
using Specht.Model;
using Specht.Report;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.DocumentFrontmatter;

/// <summary>One document is checked through the tree's reader, selected schemas and frontmatter rule, entirely in memory.</summary>
[Trait("Tier", "Unit")]
public sealed class SpechtRunnerFrontmatterUnitTests
{
    [Theory]
    [ClassData(typeof(FrontmatterKinds))]
    public void AManifestKind_WhenParsed_ShouldBeAvailableWithoutReadingARoot(string name, FrontmatterKind kind, string ruleId)
    {
        // Given
        var runner = Runner(new MockFileSystem());

        // When
        var parsed = runner.ParseFrontmatterKind(name);

        // Then
        parsed.Should().Be(kind, ruleId);
    }

    [Fact]
    public void AnUnknownKind_WhenParsed_ShouldNameTheTypedKindAndAllKindsBeforeReadingARoot()
    {
        // Given
        var runner = Runner(new MockFileSystem());

        // When
        var parsing = () => runner.ParseFrontmatterKind("Story");

        // Then
        parsing.Should().Throw<ArgumentException>().WithMessage("*Story*feature*task*epic*");
    }

    [Theory]
    [ClassData(typeof(FrontmatterKinds))]
    public void ADocumentFailingItsKindSchema_WhenChecked_ShouldReportItsKindRuleAtTheKeysLine(
        string name, FrontmatterKind kind, string ruleId)
    {
        // Given
        var files = Files();
        var runner = Runner(files);

        // When
        var report = runner.CheckFrontmatter("repo", kind, "drafts/issue.md", "---\n\ntitle: invalid\n---\n");

        // Then
        report.Violations.Should().HaveCount(2);
        report.Violations.Should().OnlyContain(v => v.RuleId == ruleId && v.File == "drafts/issue.md", name);
        report.Violations.Where(static v => v.Identifier == "title").Should().ContainSingle().Which.Line.Should().Be(3);
    }

    [Theory]
    [ClassData(typeof(FrontmatterKinds))]
    public void ADocumentWithoutFrontmatter_WhenChecked_ShouldReportSpec001AtLineOne(string name, FrontmatterKind kind, string ruleId)
    {
        // Given
        var runner = Runner(Files());

        // When
        var report = runner.CheckFrontmatter("repo", kind, "<stdin>", "# A document\n");

        // Then
        report.Violations.Should().ContainSingle().Which.Should().Match<SpecViolation>(static v =>
            v.RuleId == "SPEC001" && v.File == "<stdin>" && v.Line == 1, name + ruleId);
    }

    [Theory]
    [ClassData(typeof(FrontmatterLevels))]
    public void AManifestRuleLevel_WhenDocumentChecked_ShouldGradeOrDropTheViolationAndCountIt(string manifest, int errors, int warnings)
    {
        // Given
        var runner = Runner(Files(manifest));

        // When
        var report = runner.CheckFrontmatter("repo", FrontmatterKind.Task, "<stdin>", "---\ntitle: invalid\n---\n");

        // Then
        report.ErrorCount.Should().Be(errors);
        report.WarningCount.Should().Be(warnings);
        report.Violations.Should().HaveCount(errors + warnings);
        SpecReportDocument.From(report).SummaryLines()[^1].Should().Be($"errors: {errors}, warnings: {warnings}");
    }

    [Fact]
    public void AValidDocumentBesideInvalidRootSpecifications_WhenChecked_ShouldIgnoreTheTreeAndEveryOtherRule()
    {
        // Given
        var files = Files();
        files.AddFile("repo/src/sample/.spec/README.md", new MockFileData("invalid frontmatter and no sections"));
        var runner = Runner(files, extraRule: new ForbiddenRule());
        var before = files.AllFiles.ToDictionary(path => path, path => files.File.ReadAllText(path));

        // When
        var report = runner.CheckFrontmatter("repo", FrontmatterKind.Feature, "draft.md", "---\ntitle: feature\n---\n");

        // Then
        report.Violations.Should().BeEmpty();
        report.SpecificationCount.Should().Be(0);
        report.ItemCount.Should().Be(0);
        files.AllFiles.ToDictionary(path => path, path => files.File.ReadAllText(path)).Should().BeEquivalentTo(before);
    }

    [Theory]
    [ClassData(typeof(FrontmatterSources))]
    public void APinnedRoot_WhenDocumentChecked_ShouldUseTheSameSchemaSourceAndVerdictAsTheTree(string source, SpecSchemaSource expected)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0" };
        var local = """{ "type": "object", "properties": { "title": { "const": "local" } } }""";
        if (source == "disk")
        {
            manifest["schemaSource"] = "disk";
        }
        else if (source == "upstream")
        {
            var hash = "sha256:" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(local)));
            manifest["spechtSchema"] = new JsonObject
            {
                ["location"] = "https://schemas.example/frontmatter/",
                ["version"] = "fixture",
                ["hashes"] = new JsonObject { ["feature"] = hash, ["task"] = hash },
            };
        }

        var files = Files(manifest.ToJsonString());
        files.AddFile("repo/" + SpecTree.FeatureSchemaPath, new MockFileData(local));
        files.AddFile("repo/" + SpecTree.ItemSchemaPath, new MockFileData(local));
        const string path = "src/sample/.spec/README.md";
        const string text = "---\ntitle: feature\n---\n";
        files.AddFile("repo/" + path, new MockFileData(text));
        var one = Version(new SemanticVersion(0, 1, 0));
        var two = Version(new SemanticVersion(0, 2, 0)) with
        {
            FeatureSchema = """{ "type": "object", "properties": { "title": { "const": "pinned" } } }""",
        };
        var runner = Runner(files, new SchemaVersions([one, two]));

        // When
        var document = runner.CheckFrontmatter("repo", FrontmatterKind.Feature, path, text);
        var tree = runner.Run("repo");

        // Then
        document.SchemaVersion.Should().Be(new SemanticVersion(0, 2, 0));
        document.SchemaSource.Should().Be(expected);
        document.Violations.Should().HaveCount(2);
        document.Violations.Single(static v => v.Identifier == "title").Expected!["constraints"]!["const"]!.GetValue<string>()
            .Should().Be(source == "embedded" ? "pinned" : "local");
        JsonSerializer.Serialize(document.Violations).Should().Be(JsonSerializer.Serialize(
            tree.Violations.Where(static v => v.File == path && v.RuleId is "SPEC001" or "SPEC002")));
    }

    [Fact]
    public void TheSameDocumentKindAndRoot_WhenCheckedTwice_ShouldReturnTheSameReport()
    {
        // Given
        var runner = Runner(Files());

        // When
        var first = runner.CheckFrontmatter("repo", FrontmatterKind.Task, "draft.md", "---\ntitle: invalid\n---\n");
        var second = runner.CheckFrontmatter("repo", FrontmatterKind.Task, "draft.md", "---\ntitle: invalid\n---\n");

        // Then
        SpecReportDocument.From(first).ToJson().Should().Be(SpecReportDocument.From(second).ToJson());
    }

    [Theory]
    [ClassData(typeof(FrontmatterInputFailures))]
    public void AnInvalidRootOrManifest_WhenDocumentChecked_ShouldRetainTheChecksTypedFailure(bool rootExists, string? manifest, Type failure)
    {
        // Given
        var files = new MockFileSystem();
        if (rootExists)
        {
            files.AddDirectory("repo");
        }

        if (manifest is not null)
        {
            files.AddFile("repo/" + SpecTree.ManifestPath, new MockFileData(manifest));
        }

        var runner = Runner(files);

        // When
        var checking = () => runner.CheckFrontmatter("repo", FrontmatterKind.Task, "draft.md", "---\ntitle: task\n---\n");

        // Then
        checking.Should().Throw<Exception>().Which.GetType().Should().Be(failure);
    }

    private static MockFileSystem Files(string manifest = "{}") => new(new Dictionary<string, MockFileData>
    {
        ["repo/" + SpecTree.ManifestPath] = new(manifest),
    });

    private static SchemaVersion Version(SemanticVersion number)
    {
        SchemaVersion version = new SchemaVersionFixture().WithNumber(number).WithRuleIds(
            new HashSet<string>(["SPEC001", "SPEC002", "SPEC003", "SPEC004", "SPEC010"], StringComparer.Ordinal));
        return version with
        {
            FeatureSchema = """{ "properties": { "title": { "const": "feature" } } }""",
            ItemSchema = """{ "properties": { "title": { "const": "task" } } }""",
            EpicSchema = """{ "properties": { "title": { "const": "epic" } } }""",
        };
    }

    private static SpechtRunner Runner(MockFileSystem files, SchemaVersions? versions = null, ISpecRule? extraRule = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IFileSystem>(files);
        services.AddSingleton(versions ?? new SchemaVersions([Version(new SemanticVersion(0, 1, 0))]));
        services.AddSpechtEngine();
        if (extraRule is not null)
        {
            services.AddSingleton(extraRule);
        }

        return services.BuildServiceProvider().GetRequiredService<SpechtRunner>();
    }

    private sealed class ForbiddenRule : ISpecRule
    {
        public string Id => "SPEC010";

        public IReadOnlyList<string> ReportedIds => ["SPEC010"];

        public IEnumerable<SpecViolation> Evaluate(SpecModel model) => throw new InvalidOperationException("A tree-only rule was evaluated.");
    }
}
