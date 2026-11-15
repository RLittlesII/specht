namespace Specht.Tests.Versioning;

/// <summary>
/// Each rule key that enters schema 0.4.0 above 0.3.1, and the four-digit id ADR-0029's map gives it there (<c>0001-F7</c>
/// B-151; <c>0001-F12</c> decision 0005): <c>SPEC023</c>, the missing-scenario rule, takes <c>SPEC4008</c>, and
/// <c>SPEC033</c>, the status rule, takes <c>SPEC4010</c>; <c>SPEC024</c>, the orphan-companion rule, takes <c>SPEC2003</c>
/// (<c>0001-F6</c> B-013, decision 0013); <c>SPEC025</c>, the scenario-without-claim rule, takes <c>SPEC4011</c>
/// (<c>0001-F16</c> B-010, decision 0004); <c>SPEC026</c>, the Examples claim tag rule, takes <c>SPEC4012</c>
/// (<c>0001-F16</c> B-009, decision 0004); and <c>SPEC027</c>, the claim input fault rule, takes <c>SPEC4013</c>
/// (<c>0001-F16</c> B-014 to B-016, B-018, B-019, B-033, B-034, decision 0004).
/// </summary>
public sealed class ZeroFourZeroRuleNames : TheoryData<string, string>
{
    /// <summary>Initializes one row per pair.</summary>
    public ZeroFourZeroRuleNames()
    {
        Add("SPEC023", "SPEC4008");
        Add("SPEC024", "SPEC2003");
        Add("SPEC033", "SPEC4010");
        Add("SPEC025", "SPEC4011");
        Add("SPEC026", "SPEC4012");
        Add("SPEC027", "SPEC4013");
    }
}
