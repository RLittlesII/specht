namespace Specht.Analysis.Tests.Documentation;

public sealed class RelationshipReplacementCases : TheoryData<string, string>
{
    public RelationshipReplacementCases()
    {
        Add("<verifies feature=\"cap:read\" claim=\"need/77\" />", "<verifies feature=\"cap:read\" claim=\"need/7\" />");
        Add("<implements feature=\"cap:read\" claim=\"need/77\" />", "<implements feature=\"cap:read\" claim=\"need/7\" />");
        Add("<follows adr=\"ADR-0099\" />", "<follows adr=\"ADR-0012\" />");
    }
}
