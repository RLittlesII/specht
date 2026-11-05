namespace Specht.Tests.Mcp;

/// <summary>
/// Each tool of rule text, the rule list and the manifest, the argument its call carries, if any, and each exit code with which a
/// command answers.
/// </summary>
public sealed class EmptyReadings : TheoryData<string, string?, string?, int>
{
    public EmptyReadings()
    {
        foreach (var exitCode in new[] { 0, 1 })
        {
            Add("explain", "rule", "SPEC4004", exitCode);
            Add("rules", null, null, exitCode);
            Add("manifest", null, null, exitCode);
        }
    }
}
