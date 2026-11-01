using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Security.Cryptography;
using AwesomeAssertions;
using Specht.Comparison.Tests.Evidence;
using Specht.Comparison.Tests.Selection;

namespace Specht.Comparison.Tests.Reporting;

[Trait("Tier", "Integration")]
public sealed class ReportBundleAssetStoreIntegrationTests
{
    [Fact]
    public async Task InvalidBundle_WhenPublishing_ShouldSendNoRequest()
    {
        // Given
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var storage = new ReleaseAssetStorage();
        using var handler = new ScriptedHttpHandler(storage.Respond);
        using var client = new HttpClient(handler);

        // When
        var publish = () => new ReportBundleAssetStore(client).PublishAsync(current, "comparison-20-run-1-1.zip", [80, 75, 3, 4, 0],
            TestContext.Current.CancellationToken);

        // Then
        await publish.Should().ThrowAsync<InvalidDataException>();
        storage.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task NewBundle_WhenPublishing_ShouldInsertThenVerifyTheRetainedAsset()
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
        var bundle = new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), new ComparisonDocumentFixture(),
            [new IdentityMappingFixture()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");
        var zip = fileSystem.File.ReadAllBytes(Path.Combine("repo", bundle.Path));
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var storage = new ReleaseAssetStorage();
        storage.Assets.AddRange(Enumerable.Range(1, 100).Select(static id => new ReleaseAsset(id, $"other-{id}.txt", "uploaded", [1])));
        using var handler = new ScriptedHttpHandler(storage.Respond);
        using var client = new HttpClient(handler);

        // When
        var published = await new ReportBundleAssetStore(client).PublishAsync(current, "comparison-20-run-1-1.zip", zip,
            TestContext.Current.CancellationToken);

        // Then
        var retained = storage.Assets.Should().ContainSingle(static asset => asset.Name == "comparison-20-run-1-1.zip").Subject;
        retained.Content.Should().Equal(zip);
        published.Should().Be(new PublishedBundle(retained.Id, "comparison-20-run-1-1.zip", Convert.ToHexStringLower(SHA256.HashData(zip))));
        var requests = storage.Requests;
        var upload = requests.FindIndex(static request => request.Method == HttpMethod.Post);
        requests.Count(static request => request.Method == HttpMethod.Post).Should().Be(1);
        requests[upload].Uri.Should().Be("https://uploads.github.com/repos/example/tool/releases/20/assets?name=comparison-20-run-1-1.zip");
        requests[upload].ContentType.Should().Be("application/zip");
        requests.FindIndex(static request => request.Uri.Contains("page=2", StringComparison.Ordinal)).Should().BeInRange(0, upload - 1);
        requests.Skip(upload + 1).Should().Contain(static request => request.Uri.StartsWith(
            "https://api.github.com/repos/example/tool/releases/20/assets?", StringComparison.Ordinal));
        requests.Skip(upload + 1).Should().Contain(request => request.Uri == $"https://api.github.com/repos/example/tool/releases/assets/{retained.Id}");
        requests.Should().OnlyContain(static request => request.Method == HttpMethod.Get || request.Method == HttpMethod.Post);
    }

    [Fact]
    public async Task IdenticalRetainedAsset_WhenPublishing_ShouldConfirmItWithoutUploading()
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
        var bundle = new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), new ComparisonDocumentFixture(),
            [new IdentityMappingFixture()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");
        var zip = fileSystem.File.ReadAllBytes(Path.Combine("repo", bundle.Path));
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var storage = new ReleaseAssetStorage();
        storage.Assets.AddRange(Enumerable.Range(1, 100).Select(static id => new ReleaseAsset(id, $"other-{id}.txt", "uploaded", [1])));
        storage.Assets.Add(new ReleaseAsset(500, "comparison-20-run-1-1.zip", "uploaded", zip));
        using var handler = new ScriptedHttpHandler(storage.Respond);
        using var client = new HttpClient(handler);

        // When
        var published = await new ReportBundleAssetStore(client).PublishAsync(current, "comparison-20-run-1-1.zip", zip,
            TestContext.Current.CancellationToken);

        // Then
        published.Should().Be(new PublishedBundle(500, "comparison-20-run-1-1.zip", Convert.ToHexStringLower(SHA256.HashData(zip))));
        storage.Requests.Should().OnlyContain(static request => request.Method == HttpMethod.Get);
        storage.Assets.Should().HaveCount(101);
    }

    [Fact]
    public async Task DifferentRetainedAsset_WhenPublishing_ShouldRejectWithoutClobberingIt()
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
        var bundle = new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), new ComparisonDocumentFixture(),
            [new IdentityMappingFixture()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");
        var zip = fileSystem.File.ReadAllBytes(Path.Combine("repo", bundle.Path));
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var storage = new ReleaseAssetStorage();
        storage.Assets.Add(new ReleaseAsset(500, "comparison-20-run-1-1.zip", "uploaded", [9, 9, 9]));
        using var handler = new ScriptedHttpHandler(storage.Respond);
        using var client = new HttpClient(handler);

        // When
        var publish = () => new ReportBundleAssetStore(client).PublishAsync(current, "comparison-20-run-1-1.zip", zip,
            TestContext.Current.CancellationToken);

        // Then
        await publish.Should().ThrowAsync<InvalidDataException>();
        storage.Requests.Should().OnlyContain(static request => request.Method == HttpMethod.Get);
        storage.Assets.Should().ContainSingle().Which.Content.Should().Equal(9, 9, 9);
    }

    [Theory]
    [ClassData(typeof(UnconfirmedUploadRows))]
    public async Task UnconfirmedUpload_WhenPublishing_ShouldNotClaimSuccess(string failure)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
        var bundle = new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), new ComparisonDocumentFixture(),
            [new IdentityMappingFixture()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");
        var zip = fileSystem.File.ReadAllBytes(Path.Combine("repo", bundle.Path));
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var storage = new ReleaseAssetStorage();
        if (failure == "rejected upload") storage.UploadStatus = HttpStatusCode.InternalServerError;
        else if (failure == "incomplete upload") storage.UploadedState = "starter";
        else if (failure == "truncated upload") storage.Retain = static content => content[..^1];
        else storage.DownloadStatus = HttpStatusCode.BadGateway;
        using var handler = new ScriptedHttpHandler(storage.Respond);
        using var client = new HttpClient(handler);

        // When
        var publish = () => new ReportBundleAssetStore(client).PublishAsync(current, "comparison-20-run-1-1.zip", zip,
            TestContext.Current.CancellationToken);

        // Then
        await publish.Should().ThrowAsync<InvalidDataException>();
        storage.Requests.Should().OnlyContain(static request => request.Method == HttpMethod.Get || request.Method == HttpMethod.Post);
    }

    [Theory]
    [ClassData(typeof(MismatchedNameRows))]
    public async Task MismatchedAssetName_WhenPublishing_ShouldRejectWithoutUploading(string name)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
        var bundle = new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), new ComparisonDocumentFixture(),
            [new IdentityMappingFixture()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");
        var zip = fileSystem.File.ReadAllBytes(Path.Combine("repo", bundle.Path));
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var storage = new ReleaseAssetStorage();
        using var handler = new ScriptedHttpHandler(storage.Respond);
        using var client = new HttpClient(handler);

        // When
        var publish = () => new ReportBundleAssetStore(client).PublishAsync(current, name, zip, TestContext.Current.CancellationToken);

        // Then
        await publish.Should().ThrowAsync<InvalidDataException>();
        storage.Requests.Should().NotContain(static request => request.Method == HttpMethod.Post);
        storage.Assets.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(ConflictStatusRows))]
    public async Task ConflictingUploadOfThisBundle_WhenPublishing_ShouldConfirmTheRetainedAsset(HttpStatusCode status)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
        var bundle = new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), new ComparisonDocumentFixture(),
            [new IdentityMappingFixture()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");
        var zip = fileSystem.File.ReadAllBytes(Path.Combine("repo", bundle.Path));
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var storage = new ReleaseAssetStorage { UploadStatus = status, Racer = static content => content };
        using var handler = new ScriptedHttpHandler(storage.Respond);
        using var client = new HttpClient(handler);

        // When
        var published = await new ReportBundleAssetStore(client).PublishAsync(current, "comparison-20-run-1-1.zip", zip,
            TestContext.Current.CancellationToken);

        // Then
        published.Should().Be(new PublishedBundle(1000, "comparison-20-run-1-1.zip", Convert.ToHexStringLower(SHA256.HashData(zip))));
        storage.Requests.Count(static request => request.Method == HttpMethod.Post).Should().Be(1);
        storage.Assets.Should().ContainSingle().Which.Content.Should().Equal(zip);
    }

    [Theory]
    [ClassData(typeof(ConflictStatusRows))]
    public async Task ConflictingUploadOfAnotherBundle_WhenPublishing_ShouldRejectWithoutClobberingIt(HttpStatusCode status)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
        var bundle = new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), new ComparisonDocumentFixture(),
            [new IdentityMappingFixture()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");
        var zip = fileSystem.File.ReadAllBytes(Path.Combine("repo", bundle.Path));
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var storage = new ReleaseAssetStorage { UploadStatus = status, Racer = static _ => [9, 9, 9] };
        using var handler = new ScriptedHttpHandler(storage.Respond);
        using var client = new HttpClient(handler);

        // When
        var publish = () => new ReportBundleAssetStore(client).PublishAsync(current, "comparison-20-run-1-1.zip", zip,
            TestContext.Current.CancellationToken);

        // Then
        await publish.Should().ThrowAsync<InvalidDataException>();
        storage.Requests.Count(static request => request.Method == HttpMethod.Post).Should().Be(1);
        storage.Assets.Should().ContainSingle().Which.Content.Should().Equal(9, 9, 9);
    }

    [Theory]
    [ClassData(typeof(UnusableListingRows))]
    public async Task UnusableRetainedListing_WhenPublishing_ShouldRejectWithoutUploading(string defect)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
        var bundle = new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), new ComparisonDocumentFixture(),
            [new IdentityMappingFixture()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");
        var zip = fileSystem.File.ReadAllBytes(Path.Combine("repo", bundle.Path));
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var storage = new ReleaseAssetStorage();
        storage.Assets.Add(new ReleaseAsset(500, "comparison-20-run-1-1.zip", defect == "duplicate name" ? "uploaded" : "starter", zip));
        if (defect == "duplicate name") storage.Assets.Add(new ReleaseAsset(501, "comparison-20-run-1-1.zip", "uploaded", zip));
        using var handler = new ScriptedHttpHandler(storage.Respond);
        using var client = new HttpClient(handler);

        // When
        var publish = () => new ReportBundleAssetStore(client).PublishAsync(current, "comparison-20-run-1-1.zip", zip,
            TestContext.Current.CancellationToken);

        // Then
        await publish.Should().ThrowAsync<InvalidDataException>();
        storage.Requests.Should().OnlyContain(static request => request.Method == HttpMethod.Get);
    }

    public sealed class UnconfirmedUploadRows : TheoryData<string>
    {
        public UnconfirmedUploadRows()
        {
            Add("rejected upload");
            Add("incomplete upload");
            Add("truncated upload");
            Add("unavailable download");
        }
    }

    public sealed class MismatchedNameRows : TheoryData<string>
    {
        public MismatchedNameRows()
        {
            Add("comparison-21-run-1-1.zip");
            Add("report.zip");
            Add("../comparison-20-run-1-1.zip");
            Add("comparison-20-run-1-1.zip.json");
            Add("comparison-20-run-2-1.zip");
            Add("comparison-20-run-1-2.zip");
        }
    }

    public sealed class ConflictStatusRows : TheoryData<HttpStatusCode>
    {
        public ConflictStatusRows()
        {
            Add(HttpStatusCode.Conflict);
            Add(HttpStatusCode.UnprocessableEntity);
        }
    }

    public sealed class UnusableListingRows : TheoryData<string>
    {
        public UnusableListingRows()
        {
            Add("duplicate name");
            Add("incomplete state");
        }
    }
}
