namespace Specht.Tests.FrontmatterCommand;

/// <summary>Typed engine input failures retain the check's codes and root-relative messages.</summary>
public sealed class FrontmatterCommandRoots : TheoryData<string, string?, int, string>
{
    public FrontmatterCommandRoots()
    {
        Add("absent", "{}", 2, "'absent' is not a directory");
        Add("repo", null, 2, ".specht/manifest.json");
        Add("repo", "{", 3, ".specht/manifest.json");
        Add("repo", "{\"schemaVersion\":\"99.0.0\"}", 3, "99.0.0");
    }
}
