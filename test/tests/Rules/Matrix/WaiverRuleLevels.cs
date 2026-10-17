namespace Specht.Tests.Rules.Matrix;

/// <summary>Shipped-version selection and explicit overrides of the two own-off rules.</summary>
public sealed class WaiverRuleLevels : TheoryData<string, string?, int, int>
{
    public WaiverRuleLevels()
    {
        Add("0.2.0", null, 0, 21);
        Add("0.2.0", "off", 0, 21);
        Add("0.2.0", "error", 2, 23);
        Add("0.2.0", "warning", 2, 23);
        Add("0.1.0", null, 0, 21);
    }
}
