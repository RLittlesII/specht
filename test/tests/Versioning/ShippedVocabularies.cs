namespace Specht.Tests.Versioning;

/// <summary>
/// Each shipped version from 0.2.0 on, and the rule ids its vocabulary names (<c>0001-F7</c> B-014, B-151, B-152; C-1): 0.2.0
/// and 0.2.1 keep the three-digit ids, 0.3.0 and 0.3.1 take the four-digit ids of ADR-0029's map in their place, and 0.4.0,
/// which item <c>0436</c> cut from 0.3.1 with no rule id of its own (<c>0001-F5</c> A-8), adds <c>SPEC4008</c> (<c>0001-F12</c>
/// B-011, decision 0005, reading 1). The rules that enter 0.4.0 later add their ids to its row.
/// </summary>
public sealed class ShippedVocabularies : TheoryData<string, string[]>
{
    /// <summary>Initializes the 0.2.0, 0.2.1, 0.3.0, 0.3.1 and 0.4.0 rows.</summary>
    public ShippedVocabularies()
    {
        string[] threeDigit =
        [
            "SPEC001", "SPEC002", "SPEC003", "SPEC004", "SPEC010", "SPEC011", "SPEC012", "SPEC013", "SPEC020", "SPEC021",
            "SPEC030", "SPEC031", "SPEC034", "SPEC035", "SPEC040", "SPEC041", "SPEC043", "SPEC044", "SPEC050", "SPEC051",
            "SPEC052", "SPEC060", "SPEC061",
        ];
        Add("0.2.0", threeDigit);
        Add("0.2.1", threeDigit);
        string[] fourDigit =
        [
            "SPEC1001", "SPEC1002", "SPEC1003", "SPEC1004", "SPEC2001", "SPEC2002", "SPEC3001", "SPEC3002", "SPEC4001",
            "SPEC4002", "SPEC4003", "SPEC4004", "SPEC4005", "SPEC4006", "SPEC4007", "SPEC4009", "SPEC5001", "SPEC5002", "SPEC5003", "SPEC5004",
            "SPEC6001", "SPEC6002", "SPEC6003", "SPEC7001", "SPEC7002",
        ];
        Add("0.3.0", fourDigit);
        Add("0.3.1", fourDigit);
        Add("0.4.0", [.. fourDigit, "SPEC4008"]);
    }
}
