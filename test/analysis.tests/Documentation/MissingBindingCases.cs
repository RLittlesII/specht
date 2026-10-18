namespace Specht.Analysis.Tests.Documentation;

public sealed class MissingBindingCases : TheoryData<string, string>
{
    public MissingBindingCases()
    {
        Add("public class Example {\n    public void Run() {}\n}",
            "public class Example {\n    /// <verifies feature=\"cap:read\" claim=\"need/7\" />\n    public void Run() {}\n}");
        Add("public class Example {\r\n\tpublic void Run() {}\r\n}",
            "public class Example {\r\n\t/// <verifies feature=\"cap:read\" claim=\"need/7\" />\r\n\tpublic void Run() {}\r\n}");
        Add("public class Example { public void Run() {} }",
            "public class Example { \n/// <verifies feature=\"cap:read\" claim=\"need/7\" />\npublic void Run() {} }");
    }
}
