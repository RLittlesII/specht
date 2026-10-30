using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Tool.Features.Upgrade;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// The comparison and rewrite beneath <c>specht upgrade</c> (<c>0001-F7</c> B-005, B-006, B-007, B-016, B-017, B-018, B-019,
/// B-020, B-021, B-029, B-031, B-052, B-059, B-138, B-139; C-2): the manifest is planned like any file, its planned bytes those with
/// the pinned version's missing keys added, and a missing root or manifest, or a manifest the loader rejects, throws before any
/// file is written; a pin the check rejects throws the rejection the check's schema loader throws for the same root. Under a
/// recorded upstream schema source (B-026, B-027, B-033, B-034, B-035, B-067, B-082, B-083, B-140; C-9, C-10, C-15) the writer
/// resolves with the production fetcher over a synthetic message handler: a differing copy is fetched and written under its
/// kind's file name, a refused fetch writes nothing, a matching copy or an absent one asks for nothing, the record is kept, and
/// no request carries a credential. A template that source hashes and the manifest does not name as the consumer's (B-130,
/// B-132 to B-134, B-136, B-137, B-141; C-8 to C-10) is fetched and written only when its present copy differs from its hash,
/// is kept when it matches even where the pinned version's differs, stays absent when the root lacks it, refuses a fetch it
/// cannot verify before anything is written, and is asked for without a credential. Under the on-disk source with no
/// recorded source (B-030) each frontmatter schema the pinned version ships that the root holds by its shipped name is listed
/// as skipped with its reason and left as it was, and an absent one is not listed; a recorded source, with or without the
/// on-disk selection, gives no skip (B-143), and the embedded source rewrites the schemas and skips nothing.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class UpgradeWriterUnitTests
{
    [Fact]
    public async Task OneDifferingAndOneEqualPresentFile_WhenUpgraded_ShouldRewriteOnlyTheDifferingOneAndNameItRelativeToTheRoot()
    {
        // Given
        var shipped = UpgradeServices.Shipped().At(new SemanticVersion(0, 1, 0));
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        foreach (var file in shipped)
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes) { LastWriteTime = stamp });
        }

        fileSystem.File.WriteAllText("repo/.specht/templates/feature.md", "# A synthetic template with local edits\n");
        fileSystem.File.SetLastWriteTimeUtc("repo/.specht/templates/feature.md", stamp);

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Rewritten.Should().Equal(".specht/templates/feature.md");
        foreach (var file in shipped)
        {
            var path = "repo/" + file.Relative;
            fileSystem.File.ReadAllBytes(path).Should().Equal(file.Bytes, file.Relative);
            var untouched = file.Relative != ".specht/templates/feature.md";
            (fileSystem.File.GetLastWriteTimeUtc(path) == stamp).Should().Be(untouched, file.Relative);
        }
    }

    [Fact]
    public async Task FilesThePinnedVersionDoesNotShip_WhenUpgraded_ShouldBeLeftWithTheirBytes()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(
            "repo/.specht/manifest.json",
            new MockFileData(UpgradeServices.Shipped().At(new SemanticVersion(0, 1, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes));
        fileSystem.AddFile("repo/.specht/templates/consumer-own.md", new MockFileData("# A synthetic consumer template\n"));
        fileSystem.AddFile("repo/.specht/schema/frontmatter/consumer-own.schema.json", new MockFileData("{}"));

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Rewritten.Should().BeEmpty();
        fileSystem.File.ReadAllText("repo/.specht/templates/consumer-own.md").Should().Be("# A synthetic consumer template\n");
        fileSystem.File.ReadAllText("repo/.specht/schema/frontmatter/consumer-own.schema.json").Should().Be("{}");
    }

    [Fact]
    public async Task ShippedFilesMissingFromTheRoot_WhenUpgraded_ShouldNotBeCreatedAndTheResultNamesNoFile()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(
            "repo/.specht/manifest.json",
            new MockFileData(UpgradeServices.Shipped().At(new SemanticVersion(0, 1, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes));

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Pinned.Should().Be(new SemanticVersion(0, 1, 0));
        result.Rewritten.Should().BeEmpty();
        fileSystem.AllFiles.Should().ContainSingle();
    }

    [Theory]
    [ClassData(typeof(UpgradeRootSources))]
    public async Task HandEditedSchemasUnderADiskOrUpstreamSource_WhenUpgraded_ShouldLeaveThemByteIdenticalAndStillRewriteTheTemplates(
        string manifest, string schema)
    {
        // Given
        var edited = Encoding.UTF8.GetBytes(schema);
        var shipped = UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0))
            .Where(static file => file.Relative != ".specht/manifest.json").ToList();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var file in shipped)
        {
            var isSchema = file.Relative.StartsWith(".specht/schema/frontmatter/", StringComparison.Ordinal);
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(isSchema ? edited : file.Bytes));
        }

        fileSystem.File.WriteAllText("repo/.specht/templates/feature.md", "# A synthetic template with local edits\n");

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Rewritten.Should().Equal(".specht/templates/feature.md");
        foreach (var file in shipped)
        {
            var bytes = fileSystem.File.ReadAllBytes("repo/" + file.Relative);
            if (file.Relative.StartsWith(".specht/schema/frontmatter/", StringComparison.Ordinal))
            {
                bytes.Should().Equal(edited, file.Relative);
            }
            else
            {
                bytes.Should().Equal(file.Bytes, file.Relative);
            }
        }
    }

    [Theory]
    [ClassData(typeof(OnDiskSchemaRoots))]
    public async Task PresentShippedSchemasUnderTheOnDiskSourceWithNoRecord_WhenUpgraded_ShouldListEachByItsShippedNameAsSkippedAndLeaveItsBytes(
        string manifest, string[] present, string[] skipped)
    {
        // Given
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest) { LastWriteTime = stamp });
        foreach (var name in present)
        {
            fileSystem.AddFile(
                "repo/.specht/schema/frontmatter/" + name, new MockFileData("{\"title\":\"A synthetic consumer schema\"}\n") { LastWriteTime = stamp });
        }

        var before = fileSystem.AllFiles.ToDictionary(
            static path => path, path => (fileSystem.File.ReadAllBytes(path), fileSystem.File.GetLastWriteTimeUtc(path)));

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Skipped.Should().Equal(skipped.Select(static path => new UpgradeSkip(path, "the on-disk source is selected")));
        result.Rewritten.Should().BeEmpty();
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, (bytes, written)) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes, path);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(written, path);
        }
    }

    [Theory]
    [ClassData(typeof(UpgradeRecordedSourceManifests))]
    public async Task ARecordedSourceWithOrWithoutTheOnDiskSelection_WhenUpgraded_ShouldSkipNoSchema(string manifest)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var file in UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes));
        }

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Skipped.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(UpgradeEmbeddedManifests))]
    public async Task SchemasOfAnotherVersionUnderTheEmbeddedSource_WhenUpgraded_ShouldRewriteThemAndSkipNothing(string manifest)
    {
        // Given
        var pinned = UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0));
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var file in UpgradeServices.Shipped().At(new SemanticVersion(0, 1, 0)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes));
        }

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Skipped.Should().BeEmpty();
        var schemas = pinned.Where(static file => file.Relative.StartsWith(".specht/schema/frontmatter/", StringComparison.Ordinal)).ToList();
        schemas.Should().NotBeEmpty();
        result.Rewritten.Should().Contain(schemas.Select(static file => file.Relative));
        foreach (var file in schemas)
        {
            fileSystem.File.ReadAllBytes("repo/" + file.Relative).Should().Equal(file.Bytes, file.Relative);
        }
    }

    [Theory]
    [ClassData(typeof(UndeclaredEpicGrammarManifests))]
    public async Task AManifestDeclaringNoEpicGrammarOverFeatureAndItemSchemasOnly_WhenUpgradedAndLoaded_ShouldStillDeclareNoEpicGrammar(string manifest)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var file in UpgradeServices.Shipped().At(new SemanticVersion(0, 1, 0))
                     .Where(static file => file.Relative is not (".specht/manifest.json" or ".specht/schema/frontmatter/epic.schema.json")))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes));
        }

        // When
        await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);
        var loaded = EngineServices.Schemas(fileSystem).Load("repo");

        // Then
        loaded.Structure.DeclaresEpicGrammar.Should().BeFalse();
    }

    [Fact]
    public async Task FilesOfAnotherVersionAndAManifestLackingKeys_WhenUpgraded_ShouldAddThePinsKeysAndNameTheManifestFirst()
    {
        // Given
        var pinned = UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0));
        var older = UpgradeServices.Shipped().At(new SemanticVersion(0, 1, 0)).Where(static file => file.Relative != ".specht/manifest.json").ToList();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData("{\"schemaVersion\":\"0.2.0\"}"));
        foreach (var file in older)
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes));
        }

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        var differing = pinned.Where(file => older.Any(present => present.Relative == file.Relative && !present.Bytes.SequenceEqual(file.Bytes)))
            .Select(static file => file.Relative);
        result.Rewritten.Should().Equal([".specht/manifest.json", .. differing]);
        result.Rewritten.Should().BeInAscendingOrder(StringComparer.Ordinal);
        var shipped = JsonNode.Parse(pinned.Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        var written = JsonNode.Parse(fileSystem.File.ReadAllBytes("repo/.specht/manifest.json"))!.AsObject();
        foreach (var (key, value) in shipped.Where(static member => member.Key is not ("schemaVersion" or "identifiers") && !member.Key.StartsWith('$')))
        {
            JsonNode.DeepEquals(written[key], value).Should().BeTrue(key);
        }
    }

    [Fact]
    public async Task CurrentFilesAndAManifestLackingOneKey_WhenUpgraded_ShouldRewriteOnlyTheManifest()
    {
        // Given
        var pinned = UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0));
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        foreach (var file in pinned)
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes) { LastWriteTime = stamp });
        }

        var manifest = JsonNode.Parse(fileSystem.File.ReadAllBytes("repo/.specht/manifest.json"))!.AsObject();
        manifest.Remove("claim").Should().BeTrue();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest.ToJsonString()) { LastWriteTime = stamp });

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Rewritten.Should().Equal(".specht/manifest.json");
        var shipped = JsonNode.Parse(pinned.Single(static file => file.Relative == ".specht/manifest.json").Bytes)!;
        JsonNode.DeepEquals(JsonNode.Parse(fileSystem.File.ReadAllBytes("repo/.specht/manifest.json"))!["claim"], shipped["claim"]).Should().BeTrue();
        foreach (var file in pinned.Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.File.GetLastWriteTimeUtc("repo/" + file.Relative).Should().Be(stamp, file.Relative);
        }
    }

    [Fact]
    public async Task AManifestLackingKeys_WhenUpgraded_ShouldKeepItsSchemaVersionBytes()
    {
        // Given
        const string pin = "\"schemaVersion\" :  \"0.2.0\"";
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData("{ " + pin + " }"));

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Rewritten.Should().Equal(".specht/manifest.json");
        var text = fileSystem.File.ReadAllText("repo/.specht/manifest.json");
        text.Should().StartWith("{ " + pin);
        text.Split("schemaVersion").Should().HaveCount(2);
    }

    [Fact]
    public async Task ATemplateThatCannotBeReplaced_WhenUpgraded_ShouldThrowNamingTheRootRelativeFileAndLeaveItsBytes()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(
            "repo/.specht/manifest.json",
            new MockFileData(UpgradeServices.Shipped().At(new SemanticVersion(0, 1, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes));
        fileSystem.AddFile(
            "repo/.specht/templates/feature.md",
            new MockFileData("# A synthetic template with local edits\n") { Attributes = FileAttributes.ReadOnly });

        // When
        var upgrade = () => UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        (await upgrade.Should().ThrowAsync<SpechtManifestException>())
            .WithMessage(".specht/templates/feature.md: could not be replaced.");
        fileSystem.File.ReadAllText("repo/.specht/templates/feature.md").Should().Be("# A synthetic template with local edits\n");
    }

    [Theory]
    [ClassData(typeof(UpgradeMissingInputs))]
    public async Task AMissingRootOrManifest_WhenUpgraded_ShouldThrowTheLoadersFailureAndWriteNothing(string typedRoot, bool manifest, Type fault)
    {
        // Given
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        if (manifest)
        {
            fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData("{\"schemaVersion\":\"0.2.0\"}") { LastWriteTime = stamp });
        }

        foreach (var file in UpgradeServices.Shipped().At(new SemanticVersion(0, 1, 0)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes) { LastWriteTime = stamp });
        }

        var before = fileSystem.AllFiles.ToDictionary(
            static path => path, path => (fileSystem.File.ReadAllBytes(path), fileSystem.File.GetLastWriteTimeUtc(path)));

        // When
        var upgrade = () => UpgradeServices.Writer(fileSystem).Upgrade(Path.Combine("repo", typedRoot), CancellationToken.None);

        // Then
        await upgrade.Should().ThrowAsync<Exception>().Where(exception => exception.GetType() == fault);
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, (bytes, written)) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(written, path);
        }
    }

    [Theory]
    [ClassData(typeof(UpgradeInvalidManifests))]
    public async Task AnInvalidManifest_WhenUpgraded_ShouldThrowTheLoadersFailureAndWriteNothing(string manifest, Type fault)
    {
        // Given
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest) { LastWriteTime = stamp });
        foreach (var file in UpgradeServices.Shipped().At(new SemanticVersion(0, 1, 0)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes) { LastWriteTime = stamp });
        }

        var before = fileSystem.AllFiles.ToDictionary(
            static path => path, path => (fileSystem.File.ReadAllBytes(path), fileSystem.File.GetLastWriteTimeUtc(path)));

        // When
        var upgrade = () => UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        await upgrade.Should().ThrowAsync<Exception>().Where(exception => exception.GetType() == fault);
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, (bytes, written)) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(written, path);
        }
    }

    [Theory]
    [ClassData(typeof(UpgradeRejectedPins))]
    public async Task APinTheCheckRejects_WhenUpgraded_ShouldThrowTheChecksRejectionAndWriteNothing(string schemaVersion)
    {
        // Given
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData($"{{\"schemaVersion\":{schemaVersion}}}") { LastWriteTime = stamp });
        foreach (var file in UpgradeServices.Shipped().At(new SemanticVersion(0, 1, 0)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes) { LastWriteTime = stamp });
        }

        var before = fileSystem.AllFiles.ToDictionary(
            static path => path, path => (fileSystem.File.ReadAllBytes(path), fileSystem.File.GetLastWriteTimeUtc(path)));
        var check = () => EngineServices.Schemas(fileSystem).Load("repo");

        // When
        var upgrade = () => UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        var rejection = check.Should().ThrowExactly<SpechtManifestException>(schemaVersion).Which.Message;
        (await upgrade.Should().ThrowExactlyAsync<SpechtManifestException>(schemaVersion)).Which.Message.Should().Be(rejection);
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, (bytes, written)) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes, path);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(written, path);
        }
    }

    [Theory]
    [ClassData(typeof(UpstreamFetchedRoots))]
    public async Task DifferingCopiesOfARecordedSource_WhenUpgraded_ShouldWriteEachFetchedSchemaUnderTheFileNameTheManifestGivesItsKind(
        string manifest, string served)
    {
        // Given
        var content = JsonNode.Parse(served)!.AsObject()
            .ToDictionary(static pair => pair.Key, static pair => Encoding.UTF8.GetBytes(pair.Value!.GetValue<string>()), StringComparer.Ordinal);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var name in content.Keys)
        {
            fileSystem.AddFile("repo/.specht/schema/frontmatter/" + name, new MockFileData("{\"title\":\"A synthetic local copy\"}\n"));
        }

        using var source = new HttpClient(new SyntheticSource(request =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content[request.RequestUri!.Segments[^1]]) }));

        // When
        await UpgradeServices.Writer(fileSystem, source).Upgrade("repo", CancellationToken.None);

        // Then
        fileSystem.Directory.GetFiles("repo/.specht/schema/frontmatter").Select(fileSystem.Path.GetFileName)
            .Should().BeEquivalentTo(content.Keys);
        foreach (var (name, bytes) in content)
        {
            fileSystem.File.ReadAllBytes("repo/.specht/schema/frontmatter/" + name).Should().Equal(bytes, name);
        }
    }

    [Theory]
    [ClassData(typeof(UpstreamFetchRefusals))]
    public async Task ARefusedFetch_WhenUpgraded_ShouldThrowNamingTheSourceAndWriteNothing(string manifest, string served, string refused, string failure)
    {
        // Given
        var content = JsonNode.Parse(served)!.AsObject()
            .ToDictionary(static pair => pair.Key, static pair => Encoding.UTF8.GetBytes(pair.Value!.GetValue<string>()), StringComparer.Ordinal);
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest) { LastWriteTime = stamp });
        foreach (var name in content.Keys.Where(static name => name.EndsWith(".schema.json", StringComparison.Ordinal)))
        {
            fileSystem.AddFile(
                "repo/.specht/schema/frontmatter/" + name, new MockFileData("{\"title\":\"A synthetic local copy\"}\n") { LastWriteTime = stamp });
        }

        fileSystem.AddFile("repo/.specht/templates/feature.md", new MockFileData("# A synthetic template with local edits\n") { LastWriteTime = stamp });
        var before = fileSystem.AllFiles.ToDictionary(
            static path => path, path => (fileSystem.File.ReadAllBytes(path), fileSystem.File.GetLastWriteTimeUtc(path)));
        using var source = new HttpClient(new SyntheticSource(request =>
        {
            var name = request.RequestUri!.Segments[^1];
            return name != refused
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content[name]) }
                : failure switch
                {
                    "mismatch" => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent("{}"u8.ToArray()) },
                    "unreachable" => throw new HttpRequestException("Synthetic source is unreachable."),
                    _ => new HttpResponseMessage((HttpStatusCode)int.Parse(failure, System.Globalization.CultureInfo.InvariantCulture)),
                };
        }));

        // When
        var upgrade = () => UpgradeServices.Writer(fileSystem, source).Upgrade("repo", CancellationToken.None);

        // Then
        (await upgrade.Should().ThrowExactlyAsync<SpechtUpstreamSchemaException>()).Which.Message.Should().Contain("https://schemas.example/specht/");
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, (bytes, written)) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes, path);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(written, path);
        }
    }

    [Theory]
    [ClassData(typeof(UpgradeOfflineManifests))]
    public async Task ARootWithNothingToFetch_WhenUpgraded_ShouldSendNoRequest(string manifest)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var file in UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes));
        }

        var requests = new List<Uri>();
        using var source = new HttpClient(new SyntheticSource(request =>
        {
            requests.Add(request.RequestUri!);
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        }));

        // When
        await UpgradeServices.Writer(fileSystem, source).Upgrade("repo", CancellationToken.None);

        // Then
        requests.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(UpstreamFetchedRoots))]
    public async Task ARecordedSourceAndAManifestLackingAKey_WhenUpgraded_ShouldKeepTheRecordedVersionAndHashes(string manifest, string served)
    {
        // Given
        var content = JsonNode.Parse(served)!.AsObject()
            .ToDictionary(static pair => pair.Key, static pair => Encoding.UTF8.GetBytes(pair.Value!.GetValue<string>()), StringComparer.Ordinal);
        var lacking = JsonNode.Parse(manifest)!.AsObject();
        lacking.Remove("claim").Should().BeTrue();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(lacking.ToJsonString()));
        foreach (var name in content.Keys)
        {
            fileSystem.AddFile("repo/.specht/schema/frontmatter/" + name, new MockFileData("{\"title\":\"A synthetic local copy\"}\n"));
        }

        using var source = new HttpClient(new SyntheticSource(request =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content[request.RequestUri!.Segments[^1]]) }));

        // When
        await UpgradeServices.Writer(fileSystem, source).Upgrade("repo", CancellationToken.None);

        // Then
        var written = JsonNode.Parse(fileSystem.File.ReadAllBytes("repo/.specht/manifest.json"))!.AsObject();
        JsonNode.DeepEquals(written["spechtSchema"], lacking["spechtSchema"]).Should().BeTrue();
    }

    [Theory]
    [ClassData(typeof(UpstreamFetchedRoots))]
    public async Task ARecordedSourceWithUserInformationAndATokenOnTheMachine_WhenUpgraded_ShouldSendNoCredential(string manifest, string served)
    {
        // Given
        var content = JsonNode.Parse(served)!.AsObject()
            .ToDictionary(static pair => pair.Key, static pair => Encoding.UTF8.GetBytes(pair.Value!.GetValue<string>()), StringComparer.Ordinal);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var name in content.Keys)
        {
            fileSystem.AddFile("repo/.specht/schema/frontmatter/" + name, new MockFileData("{\"title\":\"A synthetic local copy\"}\n"));
        }

        var requests = new List<(Uri Address, string Headers)>();
        using var source = new HttpClient(new SyntheticSource(request =>
        {
            requests.Add((request.RequestUri!, request.Headers.ToString()));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content[request.RequestUri!.Segments[^1]]) };
        }));
        var token = Environment.GetEnvironmentVariable("SPECHT_SCHEMA_TOKEN");
        Environment.SetEnvironmentVariable("SPECHT_SCHEMA_TOKEN", "synthetic-machine-token");

        // When
        try
        {
            await UpgradeServices.Writer(fileSystem, source).Upgrade("repo", CancellationToken.None);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SPECHT_SCHEMA_TOKEN", token);
        }

        // Then
        requests.Should().NotBeEmpty();
        requests.Should().AllSatisfy(static request =>
        {
            request.Address.UserInfo.Should().BeEmpty();
            request.Headers.Should().NotContain("Authorization").And.NotContain("synthetic-machine-token").And.NotContain("secret");
        });
    }

    [Theory]
    [ClassData(typeof(UpstreamFetchedRoots))]
    public async Task ARecordedKindWhoseCopyIsAbsent_WhenUpgraded_ShouldNotCreateIt(string manifest, string served)
    {
        // Given
        var content = JsonNode.Parse(served)!.AsObject()
            .ToDictionary(static pair => pair.Key, static pair => Encoding.UTF8.GetBytes(pair.Value!.GetValue<string>()), StringComparer.Ordinal);
        var absent = content.Keys.First();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var name in content.Keys.Where(name => name != absent))
        {
            fileSystem.AddFile("repo/.specht/schema/frontmatter/" + name, new MockFileData("{\"title\":\"A synthetic local copy\"}\n"));
        }

        var requested = new List<string>();
        using var source = new HttpClient(new SyntheticSource(request =>
        {
            requested.Add(request.RequestUri!.Segments[^1]);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content[request.RequestUri.Segments[^1]]) };
        }));

        // When
        await UpgradeServices.Writer(fileSystem, source).Upgrade("repo", CancellationToken.None);

        // Then
        fileSystem.File.Exists("repo/.specht/schema/frontmatter/" + absent).Should().BeFalse();
        requested.Should().NotBeEmpty().And.NotContain(absent);
    }

    [Theory]
    [ClassData(typeof(UpstreamTemplateRoots))]
    public async Task ADifferingCopyOfAHashedTemplate_WhenUpgraded_ShouldWriteTheFetchedTemplateUnderItsFileName(string manifest, string name, string served)
    {
        // Given
        var fetched = Encoding.UTF8.GetBytes(served);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var file in UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes));
        }

        fileSystem.File.WriteAllText("repo/.specht/templates/" + name, "# A synthetic template with local edits\n");
        using var source = new HttpClient(new SyntheticSource(request => request.RequestUri!.Segments[^1] == name
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(fetched) }
            : new HttpResponseMessage(HttpStatusCode.NotFound)));

        // When
        await UpgradeServices.Writer(fileSystem, source).Upgrade("repo", CancellationToken.None);

        // Then
        fileSystem.File.ReadAllBytes("repo/.specht/templates/" + name).Should().Equal(fetched, name);
    }

    [Theory]
    [ClassData(typeof(UpstreamTemplateRoots))]
    public async Task AHashedTemplateMatchingItsHashAndNotThePinnedVersions_WhenUpgraded_ShouldLeaveItsBytesAndStillRewriteAnUnhashedOne(
        string manifest, string name, string served)
    {
        // Given
        var recorded = Encoding.UTF8.GetBytes(served);
        var shipped = UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0));
        shipped.Single(file => file.Relative == ".specht/templates/" + name).Bytes.Should().NotEqual(recorded);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var file in shipped.Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes));
        }

        fileSystem.File.WriteAllBytes("repo/.specht/templates/" + name, recorded);
        fileSystem.File.WriteAllText("repo/.specht/templates/adr.md", "# A synthetic template with local edits\n");

        // When
        await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        fileSystem.File.ReadAllBytes("repo/.specht/templates/" + name).Should().Equal(recorded, name);
        fileSystem.File.ReadAllBytes("repo/.specht/templates/adr.md")
            .Should().Equal(shipped.Single(static file => file.Relative == ".specht/templates/adr.md").Bytes, "adr.md is not hashed");
    }

    [Theory]
    [ClassData(typeof(UpstreamTemplateRoots))]
    public async Task AHashedTemplateAbsentFromTheRoot_WhenUpgraded_ShouldNotCreateIt(string manifest, string name, string served)
    {
        // Given
        var fetched = Encoding.UTF8.GetBytes(served);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var file in UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0))
                     .Where(file => file.Relative != ".specht/manifest.json" && file.Relative != ".specht/templates/" + name))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes));
        }

        using var source = new HttpClient(new SyntheticSource(request => request.RequestUri!.Segments[^1] == name
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(fetched) }
            : new HttpResponseMessage(HttpStatusCode.NotFound)));

        // When
        await UpgradeServices.Writer(fileSystem, source).Upgrade("repo", CancellationToken.None);

        // Then
        fileSystem.File.Exists("repo/.specht/templates/" + name).Should().BeFalse(name);
    }

    [Theory]
    [ClassData(typeof(UpstreamTemplateRoots))]
    public async Task AHashedTemplateUnderASourceWithUserInformationAndATokenOnTheMachine_WhenUpgraded_ShouldSendNoCredential(
        string manifest, string name, string served)
    {
        // Given
        var fetched = Encoding.UTF8.GetBytes(served);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var file in UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes));
        }

        fileSystem.File.WriteAllText("repo/.specht/templates/" + name, "# A synthetic template with local edits\n");
        var requests = new List<(Uri Address, string Headers)>();
        using var source = new HttpClient(new SyntheticSource(request =>
        {
            requests.Add((request.RequestUri!, request.Headers.ToString()));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(fetched) };
        }));
        var token = Environment.GetEnvironmentVariable("SPECHT_SCHEMA_TOKEN");
        Environment.SetEnvironmentVariable("SPECHT_SCHEMA_TOKEN", "synthetic-machine-token");

        // When
        try
        {
            await UpgradeServices.Writer(fileSystem, source).Upgrade("repo", CancellationToken.None);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SPECHT_SCHEMA_TOKEN", token);
        }

        // Then
        requests.Should().NotBeEmpty();
        requests.Should().AllSatisfy(static request =>
        {
            request.Address.UserInfo.Should().BeEmpty();
            request.Headers.Should().NotContain("Authorization").And.NotContain("synthetic-machine-token").And.NotContain("secret");
        });
    }

    private sealed class SyntheticSource(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }
}
