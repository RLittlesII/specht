namespace Specht.Tests.Mcp;

/// <summary>Revisions earlier than the oldest the host supports.</summary>
public sealed class EarlierRevisions : TheoryData<string>
{
    public EarlierRevisions()
    {
        Add("1900-01-01");
    }
}
