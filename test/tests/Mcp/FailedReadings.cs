namespace Specht.Tests.Mcp;

/// <summary>
/// Each tool of rule text, the rule list and the manifest, the argument its call carries, if any, and each exit code with which a
/// command reports a failure.
/// </summary>
public sealed class FailedReadings : TheoryData<string, string?, string?, int>
{
    public FailedReadings()
    {
        foreach (var exitCode in new[] { 2, 3, 4 })
        {
            Add("explain", "rule", "SPEC9999", exitCode);
            Add("rules", null, null, exitCode);
            Add("manifest", null, null, exitCode);
        }
    }
}
