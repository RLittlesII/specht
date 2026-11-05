namespace Specht.Tests.Mcp;

/// <summary>The states of a root's manifest that the check cannot use, each with whether the file is removed and what a file that stays holds.</summary>
public sealed class MissingOrRejectedManifests : TheoryData<bool, string>
{
    public MissingOrRejectedManifests()
    {
        Add(true, string.Empty);
        Add(false, "{");
        Add(false, "{\"schemaVersion\":\"9.0.0\"}");
    }
}
