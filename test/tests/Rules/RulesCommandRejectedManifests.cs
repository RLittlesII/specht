namespace Specht.Tests.Rules;

/// <summary>Representative parser, settings, pin and source-selection failures.</summary>
public sealed class RulesCommandRejectedManifests : TheoryData<string, string>
{
    public RulesCommandRejectedManifests()
    {
        Add("{", ".specht/manifest.json");
        Add("[]", ".specht/manifest.json");
        Add("""{"rules":{"SPEC010":"verbose"}}""", "verbose");
        Add("""{"rules":{"SPEC999":"off"}}""", "SPEC999");
        Add("""{"schemaVersion":"7.0.0"}""", "7.0.0");
        Add("""{"schemaVersion":"0.1"}""", "major.minor.patch");
        Add("""
            {
              "schemaVersion":"0.2.0", "schemaSource":"embedded",
              "spechtSchema": {
                "location":"https://schemas.example.test/", "version":"fixture",
                "hashes": {
                  "feature":"sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                  "task":"sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"
                }
              }
            }
            """, "contradicts");
    }
}
