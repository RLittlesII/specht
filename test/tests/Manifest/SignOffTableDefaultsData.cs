namespace Specht.Tests.Manifest;

/// <summary>The original and working pins have different sign-off defaults.</summary>
public sealed class SignOffTableDefaultsData : TheoryData<string, bool>
{
    public SignOffTableDefaultsData()
    {
        Add("0.1.0", false);
        Add("0.2.0", true);
    }
}
