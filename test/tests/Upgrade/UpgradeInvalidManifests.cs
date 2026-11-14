using Specht.Manifest;

namespace Specht.Tests.Upgrade;

/// <summary>
/// The invalid manifests of <c>0001-F7</c> B-020: one that does not parse, one setting a rule to a level no version defines,
/// one pinned to 0.2.0 that records an upstream schema source and explicitly selects the embedded source, and one pinned to
/// 0.4.0 that sets a rule to such a level beside an id setting the move takes, so the move sets aside only what it moves (B-020
/// as amended), each with the failure the loader throws.
/// </summary>
public sealed class UpgradeInvalidManifests : TheoryData<string, Type>
{
    public UpgradeInvalidManifests()
    {
        Add("{", typeof(SpechtManifestUnreadableException));
        Add("""{"rules":{"SPEC031":"invalid"}}""", typeof(SpechtManifestException));
        Add("""
            {
              "schemaVersion":"0.2.0", "schemaSource":"embedded",
              "spechtSchema": {
                "location":"https://schemas.example/specht/", "version":"v1",
                "hashes": {
                  "feature":"sha256:0000000000000000000000000000000000000000000000000000000000000000",
                  "task":"sha256:0000000000000000000000000000000000000000000000000000000000000000"
                }
              }
            }
            """, typeof(SpechtManifestException));
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"^B-[0-9]{4}$"},"rules":{"SPEC4004":"invalid"}}""", typeof(SpechtManifestException));
    }
}
