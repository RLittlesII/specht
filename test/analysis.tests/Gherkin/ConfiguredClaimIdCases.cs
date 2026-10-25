namespace Specht.Analysis.Tests.Gherkin;

public sealed class ConfiguredClaimIdCases : TheoryData<string, string, string, string?>
{
    public ConfiguredClaimIdCases()
    {
        const string typed = "^REQ-[A-Z]+-[0-9]{3}$";
        const string named = "^(REQ|NFR)-[A-Za-z0-9]+-[0-9]+$";
        Add("@{claim}", typed, "@REQ-FEAT-001", "REQ-FEAT-001");
        Add("@{claim}", typed, "@REQ-FIX-001", "REQ-FIX-001");
        Add("@{claim}", typed, "@REQ-fix-001", null);
        Add("@{claim}", typed, "@NFR-FEAT-001", null);
        Add("@{claim}", named, "@REQ-arithmetic-12", "REQ-arithmetic-12");
        Add("@{claim}", named, "@NFR-0201-7", "NFR-0201-7");
        Add("@{claim}", named, "@FIX-0201-7", null);
        Add("@claim:{claim}", typed, "@claim:REQ-FEAT-001", "REQ-FEAT-001");
        Add("@claim:{claim}", typed, "@claim:REQ-FIX-001", "REQ-FIX-001");
        Add("@claim:{claim}", typed, "@REQ-FEAT-001", null);
        Add("@claim:{claim}", typed, "@claim:REQ-FEAT-1", null);
    }
}
