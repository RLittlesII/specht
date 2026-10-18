namespace Specht.Analysis.Tests.Documentation;

public sealed class UnsafeRepairCases : TheoryData<string, string>
{
    public UnsafeRepairCases()
    {
        Add("", "<implements claim=\"need/7\" />");
        Add("<spec feature=\"cap:missing\" />", "<implements claim=\"need/7\" />");
        Add("<spec feature=\"cap:read\" /><spec feature=\"cap:other\" />", "<implements claim=\"need/7\" />");
        Add("<spec feature=\"cap:read\" />", "<implements feature=\"cap:read\" claim=\"need/77\" />");
        Add("<spec feature=\"cap:read\" />", "<implements cliam=\"need/7\" />");
        Add("<spec feature=\"cap:read\" />", "<implements claim=\"need/7\" extra=\"keep\" />");
        Add("<spec feature=\"cap:read\" />", "<implements claim=\"need/7\">keep</implements>");
        Add("<spec feature=\"cap:read\" />", "<implements claim=\"need/7\"><custom /></implements>");
        Add("<spec feature=\"cap:read\" />", "<implements claim=\"need/7\"");
    }
}
