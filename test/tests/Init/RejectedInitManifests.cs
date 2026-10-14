namespace Specht.Tests.Init;

/// <summary>Synthetic malformed, rejected and contradictory manifests for 0001-F4 B-017.</summary>
public sealed class RejectedInitManifests : TheoryData<string, string>
{
    public RejectedInitManifests()
    {
        Add("[", "does not parse into the manifest's shape");
        Add("[]", "does not parse into the manifest's shape");
        Add("null", "does not parse into the manifest's shape");
        Add("""{"unknownSetting":true}""", "does not know the key 'unknownSetting'");
        Add("""{"rules":{"SPEC031":"invalid"}}""", "not 'error', 'warning' or 'off'");
        Add("""{"rules":{"SPEC999":"off"}}""", "which schemaVersion 0.1.0 does not hold");
        Add("""{"sections":[]}""", "which sections does not list");
        Add(
            """
            {"schemaVersion":"0.2.0","schemaSource":"embedded","spechtSchema":{
              "location":"https://schemas.example.test/","version":"synthetic-v1","hashes":{
                "feature":"sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                "task":"sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"}}}
            """,
            "schemaSource 'embedded' contradicts the recorded spechtSchema source");
    }
}
