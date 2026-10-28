namespace Specht.Tests.Manifest;

/// <summary>Synthetic input failures shared by manifest command cases.</summary>
public sealed class ManifestCommandFailures : TheoryData<string?, string, int>
{
    public ManifestCommandFailures()
    {
        Add("{}", "absent", 2);
        Add(null, ".", 2);
        Add("{", ".", 3);
        Add("{\"unknown\":true}", ".", 3);
        Add("{\"schemaVersion\":\"9.0.0\"}", ".", 3);
        Add("{\"schemaVersion\":7}", ".", 3);
        Add("""
            {"schemaVersion":"0.2.0","schemaSource":"embedded","spechtSchema":{
              "location":"https://schemas.example/invented/","version":"synthetic",
              "hashes":{"feature":"sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
              "task":"sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"}}}
            """, ".", 3);
    }
}
