namespace Specht.Tests.Check;

/// <summary>The read-only boundary holds for successful checks, findings and rejected inputs.</summary>
public sealed class ReadOnlyCheckData : TheoryData<string, int>
{
    public ReadOnlyCheckData()
    {
        Add("clean", 0);
        Add("violation", 1);
        Add("missing manifest", 2);
        Add("invalid manifest", 3);
    }
}
