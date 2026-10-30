using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Versioning;

/// <summary>
/// The templates a manifest names as the consumer's (0001-F7): item 0130 binds B-065 over the check. Item 0131 binds B-005's
/// boundary, B-006's second scenario, B-060 to B-063, B-135, B-144 and the consumer-template row of B-142: "its manifest
/// names the feature template as the consumer's" sets <c>consumerTemplates</c> to <c>feature.md</c> and expects its skip
/// line, the consumer's own feature template is synthetic text that no shipped version holds, and each upgrade is the built
/// tool's launched over the Background root, except B-135's, which is item 0093's in-process run over a synthetic source
/// that would serve the hashed template. A skip line is read as <c>skipped &lt;path&gt;: &lt;reason&gt;</c> with a reason
/// that is not empty; the reason's text is not in a claim. B-144 launches the same upgrade over a second copy of the root
/// whose manifest names no template and compares the two runs.
/// </summary>
public sealed partial class VersioningSteps
{
    [Given("the report the check gives while the manifest names no template as the consumer's")]
    public void GivenTheReportTheCheckGivesWhileTheManifestNamesNoTemplate()
    {
        WhenTheCheckRuns();
        _baselineStdout = _stdout;
        _baselineExitCode = _exitCode;
    }

    [When("the manifest names the feature template as the consumer's and the check runs again")]
    public void WhenTheManifestNamesTheFeatureTemplateAndTheCheckRunsAgain()
    {
        EditManifest(static manifest => manifest["consumerTemplates"] = new JsonArray("feature.md"));
        WhenTheCheckRuns();
    }

    [Then("the report is the same")]
    public void ThenTheReportIsTheSame()
    {
        _exitCode.Should().Be(_baselineExitCode, _stderr);
        _stdout.Should().Be(_baselineStdout);
    }

    [Given("its manifest names the feature template as the consumer's")]
    public void GivenItsManifestNamesTheFeatureTemplateAsTheConsumers()
    {
        EditManifest(static manifest => manifest["consumerTemplates"] = new JsonArray("feature.md"));
        _expectedSkips.Add(FeatureTemplatePath);
    }

    [Given("the consumer has rewritten the feature template")]
    [Given("the root's feature template is as the consumer wrote it")]
    public void GivenTheConsumerHasRewrittenTheFeatureTemplate() => WriteFeatureTemplate(ConsumerTemplate);

    [Given("the root's feature template is absent")]
    public void GivenTheRootsFeatureTemplateIsAbsent()
    {
        var path = Path.Combine(Tree.Root, FeatureTemplatePath);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    [Given("the root's feature template is version {word}'s")]
    public void GivenTheRootsFeatureTemplateIsVersion(string version) =>
        WriteFeatureTemplate(ShippedAt(version).Single(static file => file.Relative == FeatureTemplatePath).Bytes);

    [Given("the root is pinned to version {word} and its templates are version {word}'s, which differ from version {word}'s")]
    public void GivenTheRootIsPinnedAndItsTemplatesAreVersionWhichDiffer(string pin, string files, string other)
    {
        GivenTheManifestPinsVersion(pin);
        GivenTheRootsTemplatesAreVersion(files);
        var compared = ShippedAt(other).ToDictionary(static file => file.Relative, static file => file.Bytes, StringComparer.Ordinal);
        ShippedAt(files)
            .Where(static file => file.Relative.StartsWith(SpecTree.TemplatesFolder + "/", StringComparison.Ordinal))
            .Should().NotBeEmpty().And.OnlyContain(
                file => !compared.ContainsKey(file.Relative) || !compared[file.Relative].SequenceEqual(file.Bytes),
                "version {0}'s templates differ from version {1}'s",
                files,
                other);
    }

    [Given("its templates folder holds a runbook template that version {word} does not ship")]
    public void GivenItsTemplatesFolderHoldsARunbookTemplateThatTheVersionDoesNotShip(string version)
    {
        ShippedAt(version).Should().NotContain(static file => file.Relative == RunbookTemplatePath);
        File.WriteAllBytes(Path.Combine(Tree.Root, RunbookTemplatePath), RunbookTemplate);
    }

    [Given(@"^its manifest (names|does not name) the runbook template as the consumer's$")]
    public void GivenItsManifestNamesTheRunbookTemplate(string naming)
    {
        if (naming == "names")
        {
            EditManifest(static manifest => manifest["consumerTemplates"] = new JsonArray("runbook.md"));
        }
        else
        {
            ManifestAfter().ContainsKey("consumerTemplates").Should().BeFalse();
        }
    }

    [Given("two copies of a root pinned to version {word} whose frontmatter schemas and templates are version {word}'s")]
    public void GivenTwoCopiesOfARoot(string pin, string files)
    {
        GivenTheRootAndFilesHaveVersions(pin, files);
        _copy = new SpecTree();
        Directory.Delete(Path.Combine(_copy.Root, SpecTree.ConfigurationFolder), recursive: true);
        foreach (var (relative, bytes) in SnapshotFiles())
        {
            var path = Path.Combine(_copy.Root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
        }
    }

    [Given(@"^one copy's manifest names (.+) as the consumer's and the other's names no template$")]
    public void GivenOneCopysManifestNamesATemplateAndTheOthersNamesNone(string name)
    {
        var pin = ManifestAfter()["schemaVersion"]!.GetValue<string>();
        var fileName = UnshippedNames[name];
        ShippedAt(pin).Should().NotContain(file => file.Relative == SpecTree.TemplatesFolder + "/" + fileName);
        EditManifest(manifest => manifest["consumerTemplates"] = new JsonArray(fileName));
        JsonNode.Parse(File.ReadAllBytes(Path.Combine(Copy.Root, SpecTree.ManifestPath)))!.AsObject()
            .ContainsKey("consumerTemplates").Should().BeFalse();
    }

    [When("upgrade runs against each copy")]
    public void WhenUpgradeRunsAgainstEachCopy()
    {
        WhenUpgradeRunsAgainstIt();
        (_copyStdout, _copyStderr, _copyExitCode) = AcceptanceTool.Launch(Copy.Root, "upgrade");
    }

    [Then("the feature template is as the consumer wrote it")]
    public void ThenTheFeatureTemplateIsAsTheConsumerWroteIt()
    {
        _exitCode.Should().Be(0, _stderr);
        File.ReadAllBytes(Path.Combine(Tree.Root, FeatureTemplatePath)).Should().Equal(ConsumerTemplate);
    }

    [Then("after the templates it rewrote, the output names the feature template as skipped, with the reason")]
    public void ThenAfterTheTemplatesItRewroteTheOutputNamesTheFeatureTemplateAsSkipped()
    {
        _exitCode.Should().Be(0, _stderr);
        var lines = OutputLines();
        var rewritten = lines.Select(static (line, index) => (Line: line, Index: index))
            .Where(static entry => entry.Line.StartsWith("rewritten ", StringComparison.Ordinal))
            .ToList();
        rewritten.Should().Contain(
            static entry => entry.Line.StartsWith("rewritten " + SpecTree.TemplatesFolder + "/", StringComparison.Ordinal),
            "upgrade rewrote at least one template: {0}",
            _stdout);
        lines.FindIndex(static line => SkipLine(line) == FeatureTemplatePath)
            .Should().BeGreaterThan(
                rewritten.Max(static entry => entry.Index),
                "the feature template is named as skipped after every rewritten file: {0}",
                _stdout);
    }

    [Then("the output names the feature template as skipped, with the reason")]
    public void ThenTheOutputNamesTheFeatureTemplateAsSkipped()
    {
        _exitCode.Should().Be(0, _stderr);
        OutputLines().Select(SkipLine).Should().Contain(FeatureTemplatePath, _stdout);
    }

    [Then("every template but the feature template is version {word}'s")]
    public void ThenEveryTemplateButTheFeatureTemplateIsVersion(string version)
    {
        _exitCode.Should().Be(0, _stderr);
        var templates = ShippedAt(version)
            .Where(static file => file.Relative.StartsWith(SpecTree.TemplatesFolder + "/", StringComparison.Ordinal) && file.Relative != FeatureTemplatePath)
            .ToList();
        templates.Should().NotBeEmpty();
        var after = SnapshotFiles();
        foreach (var file in templates)
        {
            after[file.Relative].Should().Equal(file.Bytes, file.Relative);
        }
    }

    [Then("the runbook template is still there, as the consumer wrote it")]
    public void ThenTheRunbookTemplateIsStillThereAsTheConsumerWroteIt()
    {
        _exitCode.Should().Be(0, _stderr);
        File.ReadAllBytes(Path.Combine(Tree.Root, RunbookTemplatePath)).Should().Equal(RunbookTemplate);
    }

    [Then("the templates folder holds no new file that version {word} does not ship")]
    public void ThenTheTemplatesFolderHoldsNoNewFileThatTheVersionDoesNotShip(string version)
    {
        var shipped = ShippedAt(version).Select(static file => file.Relative).ToHashSet(StringComparer.Ordinal);
        SnapshotFiles().Keys
            .Where(static path => path.StartsWith(SpecTree.TemplatesFolder + "/", StringComparison.Ordinal))
            .Where(path => !_filesBefore!.ContainsKey(path) && !shipped.Contains(path))
            .Should().BeEmpty();
    }

    [Then("the frontmatter schemas are version {word}'s")]
    public void ThenTheFrontmatterSchemasAreVersion(string version)
    {
        _exitCode.Should().Be(0, _stderr);
        var schemas = ShippedAt(version)
            .Where(static file => file.Relative.StartsWith(SpecTree.FrontmatterSchemaFolder + "/", StringComparison.Ordinal))
            .ToList();
        schemas.Should().NotBeEmpty();
        var after = SnapshotFiles();
        foreach (var file in schemas)
        {
            after[file.Relative].Should().Equal(file.Bytes, file.Relative);
        }
    }

    [Then("the two outputs are the same")]
    public void ThenTheTwoOutputsAreTheSame()
    {
        _stdout.Should().NotBeEmpty(_stderr);
        _stdout.Should().Be(_copyStdout, _copyStderr);
    }

    [Then("the two exit codes are the same")]
    public void ThenTheTwoExitCodesAreTheSame() => _exitCode.Should().Be(_copyExitCode, _stderr + _copyStderr);

    [AfterScenario]
    public void DeleteCopy() => _copy?.Dispose();

    private SpecTree Copy => _copy ?? throw new InvalidOperationException("No second copy of the root was prepared.");

    private const string RunbookTemplatePath = SpecTree.TemplatesFolder + "/runbook.md";

    private static readonly byte[] ConsumerTemplate = "# A synthetic feature template of the consumer's\n"u8.ToArray();

    private static readonly byte[] RunbookTemplate = "# A synthetic runbook template of the consumer's\n"u8.ToArray();

    private static readonly Dictionary<string, string> UnshippedNames = new(StringComparer.Ordinal)
    {
        ["a runbook template, which version 0.2.1 does not ship"] = "runbook.md",
        ["a misspelling of the feature template"] = "featrue.md",
    };

    private string _baselineStdout = string.Empty;
    private int _baselineExitCode = -1;
    private SpecTree? _copy;
    private string _copyStdout = string.Empty;
    private string _copyStderr = string.Empty;
    private int _copyExitCode = -1;
}
