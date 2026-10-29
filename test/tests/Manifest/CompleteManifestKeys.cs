namespace Specht.Tests.Manifest;

/// <summary>
/// A consumer's manifest that lacks no key a shipped manifest defines (<c>0001-F7</c> B-016, B-052): every key present, a
/// present key set to <c>null</c>, a shipped manifest defining only <c>schemaVersion</c> and <c>$</c>-prefixed keys, and
/// one whose only key the consumer's lacks is <c>identifiers</c>, which no version defines (B-138).
/// </summary>
public sealed class CompleteManifestKeys : TheoryData<string, string>
{
    public CompleteManifestKeys()
    {
        Add("{\"schemaVersion\":\"0.2.0\",\"rules\":{}}", "{\"schemaVersion\":\"0.2.0\",\"$comment\":\"synthetic\",\"rules\":{\"SPEC031\":\"error\"}}");
        Add("{\"rules\":null}", "{\"schemaVersion\":\"0.2.0\",\"$comment\":\"synthetic\",\"rules\":{\"SPEC031\":\"error\"}}");
        Add("{}", "{\"schemaVersion\":\"0.2.0\",\"$comment\":\"synthetic\",\"$id\":\"synthetic\"}");
        Add(
            "{\"schemaVersion\":\"0.2.0\",\"rules\":{}}",
            "{\"schemaVersion\":\"0.2.0\",\"identifiers\":{\"claim\":\"^B-[0-9]{3}$\",\"epic\":\"^[0-9]{4}$\"},\"rules\":{\"SPEC031\":\"error\"}}");
    }
}
