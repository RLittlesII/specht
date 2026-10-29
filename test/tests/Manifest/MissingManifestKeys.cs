namespace Specht.Tests.Manifest;

/// <summary>
/// A consumer's manifest, a shipped manifest and the consumer's manifest once the keys the shipped one defines and it lacks
/// are added (<c>0001-F7</c> B-016): in shipped order and as the shipped text writes them, a present key, a <c>null</c> one
/// and an escaped one keeping their bytes, a name inside a value not counting as a key, and <c>identifiers</c> never added
/// beside the keys that are (B-138).
/// </summary>
public sealed class MissingManifestKeys : TheoryData<string, string, string>
{
    public MissingManifestKeys()
    {
        Add(
            "{\n  \"schemaVersion\": \"0.2.0\"\n}\n",
            "{\n  \"schemaVersion\": \"0.2.0\",\n  \"$comment\": \"synthetic\","
                + "\n  \"zeta\": [\n    \"^B-[0-9]+$\"\n  ],\n  \"alpha\": {\n    \"draft\": \"🟡\"\n  }\n}\n",
            "{\n  \"schemaVersion\": \"0.2.0\",\n  \"zeta\": [\n    \"^B-[0-9]+$\"\n  ],\n  \"alpha\": {\n    \"draft\": \"🟡\"\n  }\n}\n");
        Add(
            "{}",
            "{\"schemaVersion\":\"0.2.0\",\"alpha\": 1}",
            "{\n  \"alpha\": 1}");
        Add(
            "{\"rules\":{\"SPEC031\":\"warning\"},\"claim\":null,\"own\":true}",
            "{\"schemaVersion\":\"0.2.0\",\"rules\":{\"SPEC031\":\"error\",\"SPEC020\":\"error\"},"
                + "\"claim\":{\"tag\":\"@{claim}\"},\"markers\":{\"missing\":\"Missing\"}}",
            "{\"rules\":{\"SPEC031\":\"warning\"},\"claim\":null,\"own\":true,\n  \"markers\":{\"missing\":\"Missing\"}}");
        Add(
            "{\"$meta\":{\"markers\":1}}",
            "{\"markers\":2}",
            "{\"$meta\":{\"markers\":1},\n  \"markers\":2}");
        Add(
            "{\"\\u0072ules\":{}}",
            "{\"rules\":{\"SPEC031\":\"error\"},\"claim\":null}",
            "{\"\\u0072ules\":{},\n  \"claim\":null}");
        Add(
            "{\"own\":1}",
            "{\"schemaVersion\":\"0.2.0\",\"alpha\":1,\"identifiers\":{\"claim\":\"^B-[0-9]{3}$\",\"epic\":\"^[0-9]{4}$\"},\"zeta\":2}",
            "{\"own\":1,\n  \"alpha\":1,\n  \"zeta\":2}");
    }
}
