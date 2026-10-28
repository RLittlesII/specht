namespace Specht.Build.Tests;

public sealed class RefusedCutVersions : TheoryData<string>
{
    public RefusedCutVersions()
    {
        Add("0.2.0");
        Add("0.1.5");
        Add("2");
        Add(string.Empty);
    }
}
