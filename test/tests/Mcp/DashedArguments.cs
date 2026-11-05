namespace Specht.Tests.Mcp;

/// <summary>Each tool that passes an argument on to its command, the argument's name and a value of it that starts with a dash.</summary>
public sealed class DashedArguments : TheoryData<string, string, string>
{
    public DashedArguments()
    {
        Add("explain", "rule", "--dashed-rule-value");
        Add("check", "schemaSource", "--dashed-source-value");
    }
}
