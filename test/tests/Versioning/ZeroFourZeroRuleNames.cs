namespace Specht.Tests.Versioning;

/// <summary>
/// Each rule key that enters schema 0.4.0 above 0.3.1, and the four-digit id ADR-0029's map gives it there (<c>0001-F7</c>
/// B-151; <c>0001-F12</c> decision 0005): <c>SPEC023</c>, the missing-scenario rule, takes <c>SPEC4008</c>, and
/// <c>SPEC033</c>, the status rule, takes <c>SPEC4010</c>.
/// </summary>
public sealed class ZeroFourZeroRuleNames : TheoryData<string, string>
{
    /// <summary>Initializes one row per pair.</summary>
    public ZeroFourZeroRuleNames()
    {
        Add("SPEC023", "SPEC4008");
        Add("SPEC033", "SPEC4010");
    }
}
