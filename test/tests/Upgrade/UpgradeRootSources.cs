namespace Specht.Tests.Upgrade;

/// <summary>
/// The manifests of <c>0001-F7</c> B-029 whose frontmatter schemas come from the root: the on-disk selection and a recorded
/// upstream source.
/// </summary>
public sealed class UpgradeRootSources : TheoryData<string>
{
    public UpgradeRootSources()
    {
        Add("{\"schemaVersion\":\"0.2.0\",\"schemaSource\":\"disk\"}");
        Add(
            "{\"schemaVersion\":\"0.2.0\",\"spechtSchema\":{\"location\":\"https://schemas.example/specht/\","
            + $"\"version\":\"v1\",\"hashes\":{{\"feature\":\"{Hash}\",\"task\":\"{Hash}\"}}}}}}");
    }

    private const string Hash = "sha256:0000000000000000000000000000000000000000000000000000000000000000";
}
