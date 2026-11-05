namespace Specht.Tests.Mcp;

/// <summary>Each tool of rule text, the rule list and the manifest, the argument its call carries, if any, and the arguments of the command it runs.</summary>
public sealed class ReadingCommands : TheoryData<string, string?, string?, string[]>
{
    public ReadingCommands()
    {
        Add("explain", "rule", "SPEC4004", ["--explain", "SPEC4004"]);
        Add("rules", null, null, ["rules", "--json"]);
        Add("manifest", null, null, ["manifest", "--json"]);
    }
}
