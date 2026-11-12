namespace Specht.Tests.Mcp;

/// <summary>Each tool of the reading slice, the arguments its declaration offers and the ones it requires.</summary>
public sealed class DeclaredArguments : TheoryData<string, string[], string[]>
{
    public DeclaredArguments()
    {
        Add("check", ["schemaSource", "strict"], []);
        Add("explain", ["rule"], ["rule"]);
        Add("rules", [], []);
        Add("manifest", ["defaults"], []);
        Add("mint", ["type", "feature"], ["type"]);
    }
}
