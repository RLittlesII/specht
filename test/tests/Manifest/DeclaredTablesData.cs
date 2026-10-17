namespace Specht.Tests.Manifest;

/// <summary>Whole-map replacement before 0.2.0, and whole-list replacement within each role from 0.2.0.</summary>
public sealed class DeclaredTablesData : TheoryData<string, string, bool>
{
    public DeclaredTablesData()
    {
        foreach (var pin in new[] { "0.1.0", "0.2.0" })
        {
            foreach (var tables in new[]
            {
                "{}",
                """{"matrix":["ID","Case","Proof","State"]}""",
                """{"signOff":["Area","Verdict","Approver","Reason"]}""",
                """{"signOff":["Area"]}""",
            })
            {
                Add(pin, tables, pin == "0.2.0");
            }
        }
    }
}
