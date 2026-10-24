namespace Specht.Analysis.Tests.Gherkin;

public sealed class ClaimTagCases : TheoryData<string, bool, string, string, string?>
{
    public ClaimTagCases()
    {
        const string strict = "^REQ-FIX-[0-9]{3}$";
        Add("@{claim}", false, strict, "@REQ-FIX-001", "REQ-FIX-001");
        Add("@spec:{claim}", false, strict, "@spec:REQ-FIX-001", "REQ-FIX-001");
        Add("@{claim}-claim", false, strict, "@REQ-FIX-001-claim", "REQ-FIX-001");
        Add("{claim}", false, strict, "REQ-FIX-001", "REQ-FIX-001");
        Add("@ISSUE:{claim}", true, "^I-[0-9]{3}$", "@issue:i-001", "i-001");
        Add("@ISSUE:{claim}", false, "^I-[0-9]{3}$", "@issue:I-001", null);
        Add("@{claim}", true, "^REQ-[A-Z]+$", "@req-abc", "req-abc");
        Add("@{claim}", false, strict, "@REQ-FIX-5", null);
        Add("@{claim}", false, strict, "@REQ-FIX-001extra", null);
        Add("@{claim}", false, strict, "x@REQ-FIX-001", null);
        Add("@{claim}-claim", false, strict, "@REQ-FIX-001-other", null);
        Add("@{claim}", false, "^.*$", "@", string.Empty);
        Add("@{claim}", false, strict, "@", null);
        Add("@a-{claim}-z", false, "^.*$", "@a--z", string.Empty);
        Add("@a-{claim}-z", false, "^.*$", "@a-z", null);
        Add("@spec:{claim}", false, "^.*$", "@spec", null);
        const string typed = "^REQ-[A-Z]+-[0-9]{3}$";
        Add("@{claim}", false, typed, "@REQ-FEAT-001", "REQ-FEAT-001");
        Add("@{claim}", false, typed, "@REQ-FIX-001", "REQ-FIX-001");
        Add("@{claim}", false, typed, "@REQ-fix-001", null);
        Add("@{claim}", false, typed, "@REQ-FIX1-001", null);
        Add("@{claim}", false, typed, "@REQ--001", null);
        Add("@{claim}", false, typed, "@NFR-FEAT-001", null);
        Add("@{claim}", true, typed, "@req-feat-001", "req-feat-001");
        const string named = "^REQ-[A-Za-z0-9]+-[0-9]+$";
        Add("@{claim}", false, named, "@REQ-arithmetic-12", "REQ-arithmetic-12");
        Add("@{claim}", false, named, "@REQ-0201-7", "REQ-0201-7");
        Add("@{claim}", false, named, "@REQ-F7-001", "REQ-F7-001");
        Add("@{claim}", false, named, "@REQ-arithmetic", null);
        Add("@{claim}", false, named, "@REQ-arith-metic-1", null);
        const string types = "^(REQ|NFR)-[A-Za-z0-9]+-[0-9]+$";
        Add("@{claim}", false, types, "@REQ-FEAT-1", "REQ-FEAT-1");
        Add("@{claim}", false, types, "@NFR-0201-14", "NFR-0201-14");
        Add("@{claim}", false, types, "@FIX-FEAT-1", null);
        Add("@{claim}", false, types, "@REQNFR-FEAT-1", null);
        Add("@claim:{claim}", false, typed, "@claim:REQ-FEAT-001", "REQ-FEAT-001");
        Add("@claim:{claim}", false, typed, "@claim:REQ-FIX-001", "REQ-FIX-001");
        Add("@claim:{claim}", false, typed, "@REQ-FEAT-001", null);
        Add("@claim:{claim}", false, typed, "@claim:REQ-FEAT-1", null);
        Add("@claim:{claim}", false, typed, "@claim:@REQ-FEAT-001", null);
        Add("@claim:{claim}", true, types, "@CLAIM:nfr-F7-3", "nfr-F7-3");
        Add("@claim:{claim}", false, types, "@CLAIM:NFR-F7-3", null);
        Add("@claim:{claim}-v2", false, typed, "@claim:REQ-FEAT-001-v2", "REQ-FEAT-001");
        Add("@claim:{claim}-v2", false, typed, "@claim:REQ-FEAT-001", null);
    }
}
