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
    }
}
