namespace Specht.Analysis.Tests.Documentation;

public sealed class DocumentedDeclarations : TheoryData<string, string, string, bool>
{
    public DocumentedDeclarations()
    {
        foreach (var declaration in new[] { "class", "struct", "interface", "record" })
        {
            Add("/// <implements feature=\"cap:read\" claim=\"need/7\" />\npublic " + declaration + " Example { }",
                "T:Example", "implements", false);
        }

        Add("/// <spec feature=\"cap:read\" />\npublic class Example { }", "T:Example", "spec", false);
        Add("/// <follows adr=\"ADR-0012\" />\npublic enum Example { A }", "T:Example", "follows", false);
        Add("/// <follows adr=\"ADR-0012\" />\npublic delegate void Example();", "T:Example", "follows", false);
        var members = new Dictionary<string, string>
        {
            ["M:Example.#ctor"] = "public Example() { }",
            ["M:Example.Run"] = "public void Run() { }",
            ["M:Example.Finalize"] = "~Example() { }",
            ["P:Example.Value"] = "public int Value { get; set; }",
            ["P:Example.Item(System.Int32)"] = "public int this[int value] => value;",
            ["F:Example.Value"] = "public int Value;",
            ["E:Example.Changed"] = "public event System.Action Changed { add { } remove { } }",
            ["M:Example.op_Addition(Example,Example)"] = "public static Example operator +(Example a, Example b) => a;",
            ["M:Example.op_Implicit(Example)~System.Int32"] = "public static implicit operator int(Example value) => 0;",
        };
        foreach (var member in members)
        {
            Add("public class Example\n{\n/// <implements feature=\"cap:read\" claim=\"need/7\" />\n" + member.Value + "\n}",
                member.Key, "implements", false);
        }

        Add("public class Example\n{\n/// <verifies feature=\"cap:read\" claim=\"need/7\" />\npublic void Run() {}\n}",
            "M:Example.Run", "verifies", true);
        Add("public enum Example\n{\n/// <follows adr=\"ADR-0012\" />\nA\n}", "F:Example.A", "follows", false);
        Add("public class Example\n{\n/// <implements feature=\"cap:read\" claim=\"need/7\" />\npublic int A, B;\n}",
            "F:Example.B", "implements", false);
    }
}
