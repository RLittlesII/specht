using System.Text;
using AwesomeAssertions;
using Specht.Manifest;

namespace Specht.Tests.Manifest;

/// <summary>
/// The local copy of a recorded upstream schema source, held to the hashes the manifest records (<c>0001-F7</c> B-024,
/// B-077 to B-081; item 0092): the bytes of each fetched kind's file, as they were read, are that kind's schema text when
/// each file's SHA-256 is the recorded hash, and the manifest is rejected otherwise, naming every file that is absent or
/// does not match by its path under the root, and <c>specht init</c> for an absent one. The files are bytes built in the
/// test and the source is synthetic; nothing is read and nothing is fetched.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class UpstreamSchemaCopyUnitTests
{
    /// <summary>Gets what was read for the item kind's file in place of the bytes whose hash the source records.</summary>
    public static TheoryData<string, byte[]?> Faults =>
        new()
        {
            { "the file is absent (B-077, B-078)", null },
            { "the file holds one byte more than was fetched (B-080, B-081)", [.. Fetched, (byte)'\n'] },
            { "the file is empty (B-080, B-081)", [] },
        };

    [Fact]
    public void EveryFetchedFileMatchingItsRecordedHash_WhenVerified_ShouldGiveTheTextOfEachKindsFile()
    {
        // Given
        var feature = Encoding.UTF8.GetBytes("""{ "title": "the Feature schema that was fetched" }""");
        var item = Encoding.UTF8.GetBytes("""{ "title": "the item schema that was fetched" }""");
        UpstreamSchemaSource source = new UpstreamSchemaSourceFixture().WithFiles(
            new UpstreamSchemaFileFixture().Named("feature", "feature.schema.json").Fetched(feature),
            new UpstreamSchemaFileFixture().Named("task", "task.schema.json").Fetched(item));
        var files = new Dictionary<string, byte[]?>(StringComparer.Ordinal) { ["feature"] = feature, ["task"] = item };

        // When
        var texts = new UpstreamSchemaCopy().Verify(source, files);

        // Then
        texts.Should().BeEquivalentTo(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["feature"] = """{ "title": "the Feature schema that was fetched" }""",
                ["task"] = """{ "title": "the item schema that was fetched" }""",
            });
    }

    [Theory]
    [MemberData(nameof(Faults))]
    public void AFetchedFileThatIsAbsentOrDoesNotMatchItsRecordedHash_WhenVerified_ShouldRejectTheManifestNamingTheFileByItsPathUnderTheRoot(
        string because,
        byte[]? found)
    {
        // Given
        UpstreamSchemaSource source = new UpstreamSchemaSourceFixture().WithFiles(
            new UpstreamSchemaFileFixture().Named("task", "item.json").Fetched(Fetched));
        var files = new Dictionary<string, byte[]?>(StringComparer.Ordinal) { ["task"] = found };

        // When
        var verifying = () => new UpstreamSchemaCopy().Verify(source, files);

        // Then
        verifying.Should().Throw<SpechtManifestException>(because).Which.Message.Should().Contain(".specht/schema/frontmatter/item.json", because);
    }

    [Fact]
    public void AFetchedFileThatIsAbsent_WhenVerified_ShouldNameSpechtInitAsTheCommandThatFetchesIt()
    {
        // Given
        UpstreamSchemaSource source = new UpstreamSchemaSourceFixture().WithFiles(new UpstreamSchemaFileFixture().Fetched(Fetched));
        var files = new Dictionary<string, byte[]?>(StringComparer.Ordinal) { ["task"] = null };

        // When
        var verifying = () => new UpstreamSchemaCopy().Verify(source, files);

        // Then
        verifying.Should().Throw<SpechtManifestException>().Which.Message.Should().Contain("specht init");
    }

    [Fact]
    public void AnAbsentFileAndAFileThatDoesNotMatchBesideOneThatDoes_WhenVerified_ShouldNameBothFilesAtFaultAndNotTheOneThatMatches()
    {
        // Given
        UpstreamSchemaSource source = new UpstreamSchemaSourceFixture().WithFiles(
            new UpstreamSchemaFileFixture().Named("feature", "feature.schema.json").Fetched(Fetched),
            new UpstreamSchemaFileFixture().Named("task", "task.schema.json").Fetched(Fetched),
            new UpstreamSchemaFileFixture().Named("epic", "epic.schema.json").Fetched(Fetched));
        var files = new Dictionary<string, byte[]?>(StringComparer.Ordinal)
        {
            ["feature"] = null,
            ["task"] = [.. Fetched, (byte)' '],
            ["epic"] = Fetched,
        };

        // When
        var verifying = () => new UpstreamSchemaCopy().Verify(source, files);

        // Then
        verifying.Should().Throw<SpechtManifestException>()
            .Which.Message.Should().Contain(".specht/schema/frontmatter/feature.schema.json")
            .And.Contain(".specht/schema/frontmatter/task.schema.json")
            .And.NotContain(".specht/schema/frontmatter/epic.schema.json");
    }

    private static readonly byte[] Fetched = Encoding.UTF8.GetBytes("""{ "title": "a schema that was fetched" }""");
}
