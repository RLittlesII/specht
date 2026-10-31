namespace Specht.Analysis.Tests.Documentation;

public sealed class OrderedReplacementCases : TheoryData<string, string, string>
{
    public OrderedReplacementCases()
    {
        Add("<verifies epic=\"0201\" feature=\"F2\" claim=\"B-001\" />", "<verifies epic=\"0001\" feature=\"F2\" claim=\"B-999\" />",
            "<verifies epic=\"0001\" feature=\"F2\" claim=\"B-001\" />");
        Add("<verifies epic=\"0001\" feature=\"F10\" claim=\"B-001\" />", "<verifies epic=\"0001\" feature=\"F2\" claim=\"B-999\" />",
            "<verifies epic=\"0001\" feature=\"F2\" claim=\"B-001\" />");
        Add("<follows epic=\"0201\" feature=\"F2\" adr=\"ADR-0001\" />", "<follows epic=\"0001\" feature=\"F2\" adr=\"ADR-0099\" />",
            "<follows epic=\"0001\" feature=\"F2\" adr=\"ADR-0001\" />");
        Add("<follows epic=\"0001\" feature=\"F2\" adr=\"ADR-0001\" />", "<follows adr=\"ADR-0099\" />", "<follows adr=\"ADR-0012\" />");
    }
}
