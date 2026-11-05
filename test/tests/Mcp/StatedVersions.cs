namespace Specht.Tests.Mcp;

/// <summary>What a tool writes for its version, with the numbers read from it, the commit and the pre-release label read past.</summary>
public sealed class StatedVersions : TheoryData<string, int, int, int>
{
    public StatedVersions()
    {
        Add("0.4.2", 0, 4, 2);
        Add("0.5.0-preview-g+abc123\n", 0, 5, 0);
        Add("1.20.300+0f1e2d3c", 1, 20, 300);
        Add("2.0.0\nsecond line", 2, 0, 0);
    }
}
