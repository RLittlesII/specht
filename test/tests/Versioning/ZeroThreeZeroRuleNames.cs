namespace Specht.Tests.Versioning;

/// <summary>
/// Each rule key schema 0.3.0 holds, and the four-digit id ADR-0029's map gives it there (<c>0001-F7</c> B-151): the
/// 23 ids of ruling 3 and <c>SPEC032</c>, which ruling 5 gives <c>SPEC4009</c>.
/// </summary>
public sealed class ZeroThreeZeroRuleNames : TheoryData<string, string>
{
    /// <summary>Initializes one row per pair.</summary>
    public ZeroThreeZeroRuleNames()
    {
        Add("SPEC001", "SPEC1001");
        Add("SPEC002", "SPEC1002");
        Add("SPEC003", "SPEC1003");
        Add("SPEC004", "SPEC1004");
        Add("SPEC010", "SPEC2001");
        Add("SPEC013", "SPEC2002");
        Add("SPEC011", "SPEC3001");
        Add("SPEC012", "SPEC3002");
        Add("SPEC020", "SPEC4001");
        Add("SPEC021", "SPEC4002");
        Add("SPEC030", "SPEC4003");
        Add("SPEC031", "SPEC4004");
        Add("SPEC032", "SPEC4009");
        Add("SPEC034", "SPEC4005");
        Add("SPEC035", "SPEC4006");
        Add("SPEC040", "SPEC5001");
        Add("SPEC041", "SPEC5002");
        Add("SPEC043", "SPEC5003");
        Add("SPEC044", "SPEC5004");
        Add("SPEC050", "SPEC6001");
        Add("SPEC051", "SPEC6002");
        Add("SPEC052", "SPEC6003");
        Add("SPEC060", "SPEC7001");
        Add("SPEC061", "SPEC7002");
    }
}
