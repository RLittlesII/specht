namespace Specht.Tests.Facts;

/// <summary>
/// Two shipped pins the same specifications are read under: the scenario's pair, and a pair across schema 0.3.0, from
/// which a Feature's identity is read through the manifest's forms (<c>0132-F2</c> B-017).
/// </summary>
public sealed class FactsPinPairs : TheoryData<string, string>
{
    public FactsPinPairs()
    {
        Add("0.1.0", "0.2.0");
        Add("0.2.0", "0.3.0");
    }
}
