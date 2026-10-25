namespace Specht.Analysis.Tests.Documentation;

public sealed class ProvisionalShapeCases : TheoryData<string, string>
{
    public ProvisionalShapeCases()
    {
        Add("public class Example\n{\n    /// <verifies feature=\"cap:read\" claim=\"need/7\" />\n    public int Value { get; set; }\n}",
            "SPCT0002");
        Add("/// <verifies feature=\"cap:read\" claim=\"need/7\" />\npublic class Example\n{\n}", "SPCT0002");
        Add("public class Example\n{\n    /// <verifies feature=\"cap:read\" />\n    public void Run() { }\n}", "SPCT0002");
        Add("public class Example\n{\n    /// <verifies feature=\"cap:read\" claim=\"need/7\" extra=\"x\" />\n    public void Run() { }\n}", "SPCT0002");
        Add("public class Example\n{\n    /// <verifies feature=\"cap:read\" claim=\"need/7,need/8\" />\n    public void Run() { }\n}", "SPCT0003");
        Add("public class Example\n{\n    /// <verifies feature=\"cap:read\" claim=\"need/8\" />\n    public void Run() { }\n}", "SPCT0004");
    }
}
