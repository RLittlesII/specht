using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;
using Specht.Tool.Features;
using Specht.Tool.Features.Init;

namespace Specht.Acceptance.Versioning;

/// <summary>
/// Upgrade under the on-disk source (0001-F7 B-029 to B-031, B-142, B-143; item 0094): each scenario launches the built
/// tool's upgrade over the Background root, or runs it in process under a recorded source, and reads its standard output.
/// A skip line is read as <c>skipped &lt;path&gt;: &lt;reason&gt;</c> with a reason that is not empty. B-142's row whose
/// manifest names the feature template as the consumer's stays unbound for item 0131.
/// </summary>
public sealed partial class VersioningSteps
{
    [Given("the root is pinned to version {word} with the on-disk source selected")]
    public void GivenTheRootIsPinnedWithTheOnDiskSourceSelected(string version)
    {
        GivenTheManifestPinsVersion(version);
        GivenManifestSource("disk");
    }

    [Given("the root's templates are version {word}'s")]
    public void GivenTheRootsTemplatesAreVersion(string version)
    {
        foreach (var file in ShippedAt(version).Where(static file => file.Relative.StartsWith(SpecTree.TemplatesFolder + "/", StringComparison.Ordinal)))
        {
            var path = Path.Combine(Tree.Root, file.Relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, file.Bytes);
        }
    }

    [Given("the root is pinned to version {word} and already holds what upgrade would write, apart from the files it skips")]
    public void GivenTheRootIsPinnedAndAlreadyHoldsWhatUpgradeWouldWrite(string version)
    {
        GivenTheManifestPinsVersion(version);
        GivenTheRootIsAlreadyAtItsPin(version);
    }

    [Given("the on-disk source is selected and the root's frontmatter schemas are its own")]
    public void GivenTheOnDiskSourceIsSelectedAndTheRootsFrontmatterSchemasAreItsOwn()
    {
        GivenManifestSource("disk");
        var schemas = Directory.GetFiles(Path.Combine(Tree.Root, SpecTree.FrontmatterSchemaFolder))
            .Select(path => Path.GetRelativePath(Tree.Root, path).Replace('\\', '/'))
            .ToList();
        schemas.Should().NotBeEmpty();
        foreach (var schema in schemas)
        {
            File.WriteAllText(Path.Combine(Tree.Root, schema), "{\"title\":\"A synthetic consumer schema\"}\n");
        }

        _expectedSkips.AddRange(schemas);
    }

    [Then("no frontmatter schema under the root is version {word}'s embedded schema")]
    public void ThenNoFrontmatterSchemaUnderTheRootIsThatVersionsEmbeddedSchema(string version)
    {
        _exitCode.Should().Be(0, _stderr);
        var embedded = ShippedAt(version).ToDictionary(static file => file.Relative, static file => file.Bytes, StringComparer.Ordinal);
        var present = SnapshotFiles().Where(static pair => pair.Key.StartsWith(SpecTree.FrontmatterSchemaFolder + "/", StringComparison.Ordinal)).ToList();
        present.Should().NotBeEmpty();
        foreach (var (path, bytes) in present.Where(pair => embedded.ContainsKey(pair.Key)))
        {
            bytes.Should().NotEqual(embedded[path], path);
        }
    }

    [Then("after the templates it rewrote, the output names the Feature and task schemas as skipped, each with the reason")]
    public void ThenAfterTheTemplatesItRewroteTheOutputNamesTheFeatureAndTaskSchemasAsSkipped()
    {
        _exitCode.Should().Be(0, _stderr);
        var lines = OutputLines();
        var rewritten = lines.Select(static (line, index) => (Line: line, Index: index))
            .Where(static entry => entry.Line.StartsWith("rewritten ", StringComparison.Ordinal))
            .ToList();
        var templates = RewrittenFiles().Where(static path => path.StartsWith(SpecTree.TemplatesFolder + "/", StringComparison.Ordinal)).ToList();
        templates.Should().NotBeEmpty(_stdout);
        foreach (var template in templates)
        {
            rewritten.Should().Contain(entry => Regex.IsMatch(entry.Line, Naming(template)), template);
        }

        var lastRewritten = rewritten.Max(static entry => entry.Index);
        foreach (var schema in new[] { SpecTree.FeatureSchemaPath, SpecTree.ItemSchemaPath })
        {
            var index = lines.FindIndex(line => SkipLine(line) == schema);
            index.Should().BeGreaterThan(lastRewritten, "{0} is named as skipped after every rewritten file: {1}", schema, _stdout);
        }
    }

    [Then("the output names no epic schema")]
    public void ThenTheOutputNamesNoEpicSchema() =>
        OutputLines().Should().NotContain(static line => line.Contains(Path.GetFileName(SpecTree.EpicSchemaPath), StringComparison.Ordinal));

    [Then("the templates are version {word}'s")]
    public void ThenTheTemplatesAreVersion(string version)
    {
        _exitCode.Should().Be(0, _stderr);
        var templates = ShippedAt(version).Where(static file => file.Relative.StartsWith(SpecTree.TemplatesFolder + "/", StringComparison.Ordinal)).ToList();
        templates.Should().NotBeEmpty();
        var after = SnapshotFiles();
        foreach (var file in templates)
        {
            after[file.Relative].Should().Equal(file.Bytes, file.Relative);
        }
    }

    [Then("the output names each template it rewrote")]
    public void ThenTheOutputNamesEachTemplateItRewrote()
    {
        var lines = OutputLines();
        var templates = RewrittenFiles().Where(static path => path.StartsWith(SpecTree.TemplatesFolder + "/", StringComparison.Ordinal)).ToList();
        templates.Should().NotBeEmpty(_stdout);
        foreach (var path in templates)
        {
            lines.Should().Contain(line => line.StartsWith("rewritten ", StringComparison.Ordinal) && Regex.IsMatch(line, Naming(path)), path);
        }
    }

    [Then("the standard output says the repository is current, then names each file it skipped")]
    public void ThenTheStandardOutputSaysCurrentThenNamesEachFileItSkipped()
    {
        _expectedSkips.Should().NotBeEmpty();
        var lines = OutputLines();
        lines.Should().NotBeEmpty();
        lines[0].Should().Contain(".specht/ is current at schemaVersion");
        lines.Skip(1).Select(SkipLine).Should().BeEquivalentTo(_expectedSkips, _stdout);
    }

    [Then("the output names no frontmatter schema as skipped")]
    public void ThenTheOutputNamesNoFrontmatterSchemaAsSkipped()
    {
        _exitCode.Should().Be(0, _stderr);
        OutputLines().Select(SkipLine)
            .Should().NotContain(static path => path != null && path.StartsWith(SpecTree.FrontmatterSchemaFolder + "/", StringComparison.Ordinal));
    }

    private static string? SkipLine(string line)
    {
        var match = Regex.Match(line, @"^skipped (?<path>\S+): \S.*$");
        return match.Success ? match.Groups["path"].Value : null;
    }

    private static IReadOnlyList<ShippedFile> ShippedAt(string version) =>
        new ShippedFiles(InitWriter.ShippingCopy(typeof(InitWriter).Assembly)).At(Version(version));

    private readonly List<string> _expectedSkips = [];
}
