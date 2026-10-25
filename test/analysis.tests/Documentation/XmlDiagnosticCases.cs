namespace Specht.Analysis.Tests.Documentation;

public sealed class XmlDiagnosticCases : TheoryData<string, string, string>
{
    public XmlDiagnosticCases()
    {
        Add("<implements claim=\"need/7\" />", "SPCT0002", "implements");
        Add("<implements feature=\"cap:read\" cliam=\"need/7\" />", "SPCT0002", "implements");
        Add("<implements feature=\"cap:read\" claim=\"need/7\">body</implements>", "SPCT0002", "implements");
        Add("<implements feature=\"cap:read\" claim=\"need/7\"><other /></implements>", "SPCT0002", "implements");
        Add("<implements feature=\"cap:read\" claim=\"need/7\"", "SPCT0002", "implements");
        Add("<spec feature=\"cap:read\" />", "SPCT0002", "spec");
        Add("<implements feature=\"cap:read\" claim=\"need/7,need/8\" />", "SPCT0003", "need/7,need/8");
        Add("<implements feature=\"cap:read\" claim=\"need/8\" />", "SPCT0004", "need/8");
        Add("<implements feature=\"cap:read\" claim=\"need/7\" extra=\"x\" />", "SPCT0002", "implements");
        Add("<implements feature=\"cap:read\" feature=\"cap:read\" claim=\"need/7\" />", "SPCT0002", "implements");
        Add("<implements feature=\"cap:read\" claim=\"need/7&broken;\" />", "SPCT0002", "implements");
        Add("<implements feature=\"cap:read\" claim=\"need/7\"></follows>", "SPCT0002", "implements");
    }
}
