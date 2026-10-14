using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>Derives build prerequisites from the committed tool pins (0055-F7 B-007).</summary>
public sealed class BuildToolManifest
{
    /// <summary>Excludes the separately authenticated checker while retaining development-tool settings.</summary>
    public string ForBuild(string manifest)
    {
        var document = JsonNode.Parse(manifest)!;
        document["tools"]!.AsObject().Remove("specht.tool");
        return document.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n";
    }
}
