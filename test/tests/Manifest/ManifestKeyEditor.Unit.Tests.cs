using System.Text;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>
/// The key addition beneath <c>specht upgrade</c> (<c>0001-F7</c> B-016, B-052, B-138; § 7 "Manifest keys and the listing"):
/// each key the shipped manifest defines and the consumer's lacks is added, in shipped order and as the shipped text writes
/// it, <c>identifiers</c> never among them, and every byte already there is kept.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ManifestKeyEditorUnitTests
{
    [Theory]
    [ClassData(typeof(MissingManifestKeys))]
    public void AManifestLackingDefinedKeys_WhenKeysAreAdded_ShouldAppendEachShippedMemberVerbatimInShippedOrderAndKeepEveryByte(
        string consumer, string shipped, string expected)
    {
        // Given
        var bytes = Encoding.UTF8.GetBytes(consumer);
        var editor = new ManifestKeyEditor(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()));

        // When
        var added = editor.AddMissing(bytes, Encoding.UTF8.GetBytes(shipped));

        // Then
        Encoding.UTF8.GetString(added).Should().Be(expected);
        bytes.Should().Equal(Encoding.UTF8.GetBytes(consumer));
    }

    [Theory]
    [ClassData(typeof(CompleteManifestKeys))]
    public void AManifestLackingNoDefinedKey_WhenKeysAreAdded_ShouldReturnTheSameBytes(string consumer, string shipped)
    {
        // Given
        var bytes = Encoding.UTF8.GetBytes(consumer);
        var editor = new ManifestKeyEditor(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()));

        // When
        var added = editor.AddMissing(bytes, Encoding.UTF8.GetBytes(shipped));

        // Then
        added.Should().BeSameAs(bytes);
        bytes.Should().Equal(Encoding.UTF8.GetBytes(consumer));
    }

    [Theory]
    [ClassData(typeof(ManifestKeyEncodings))]
    public void ASupportedEncoding_WhenAKeyIsAdded_ShouldKeepItsBomAndEveryOriginalByteAndWriteTheKeyInThatEncoding(string name, bool bom)
    {
        // Given
        var encoding = Encoding.GetEncoding(name);
        byte[] preamble = bom ? encoding.GetPreamble() : [];
        const string text = "{\r\n  \"schemaVersion\": \"0.2.0\",\r\n  \"$comment\": \"😀 kept\"\r\n}\r\n";
        byte[] bytes = [.. preamble, .. encoding.GetBytes(text)];
        var shipped = Encoding.UTF8.GetBytes(
            "{\n  \"schemaVersion\": \"0.2.0\",\n  \"markers\": {\n    \"draft\": \"🟡\",\n    \"claim\": \"^B-[0-9]+$\"\n  }\n}\n");
        var editor = new ManifestKeyEditor(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()));

        // When
        var added = editor.AddMissing(bytes, shipped);

        // Then
        const string expected = "{\r\n  \"schemaVersion\": \"0.2.0\",\r\n  \"$comment\": \"😀 kept\","
            + "\n  \"markers\": {\n    \"draft\": \"🟡\",\n    \"claim\": \"^B-[0-9]+$\"\n  }\r\n}\r\n";
        added.Should().Equal([.. preamble, .. encoding.GetBytes(expected)]);
    }

    [Fact]
    public void APinOnlyManifest_WhenTheShippedVersionsKeysAreAdded_ShouldBeThatVersionsManifestWithoutItsCommentOrItsIdentifiers()
    {
        // Given
        using var resource = typeof(ManifestPinEditor).Assembly.GetManifestResourceStream("manifest/v0.2.0/manifest.json")!;
        using var copy = new MemoryStream();
        resource.CopyTo(copy);
        var shipped = Encoding.UTF8.GetString(copy.ToArray());
        var bytes = Encoding.UTF8.GetBytes("{\n  \"schemaVersion\": \"0.2.0\"\n}\n");
        var editor = new ManifestKeyEditor(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()));

        // When
        var added = editor.AddMissing(bytes, copy.ToArray());

        // Then
        var uncommented = string.Join('\n', shipped.Split('\n').Where(static line => !line.StartsWith("  \"$comment\":", StringComparison.Ordinal)));
        var start = uncommented.IndexOf("\n  \"identifiers\": {", StringComparison.Ordinal);
        var end = uncommented.IndexOf("\n  },", start, StringComparison.Ordinal) + "\n  },".Length;
        var expected = uncommented.Remove(start, end - start);
        uncommented.Should().NotBe(shipped);
        expected.Should().NotBe(uncommented).And.NotContain("\"identifiers\"");
        Encoding.UTF8.GetString(added).Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(NonObjectManifests))]
    public void AManifestWhoseRootIsNotAnObject_WhenKeysAreAdded_ShouldThrowNamingTheManifest(string consumer)
    {
        // Given
        var editor = new ManifestKeyEditor(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()));

        // When
        var add = () => editor.AddMissing(Encoding.UTF8.GetBytes(consumer), Encoding.UTF8.GetBytes("{\"rules\":{}}"));

        // Then
        add.Should().Throw<SpechtManifestException>().WithMessage(".specht/manifest.json: expected a manifest object.");
    }
}
