namespace Specht.Tests.Mcp;

/// <summary>A call of the tool <c>mint</c> one of whose arguments starts with a dash: the type, and the Feature beside a type that does not.</summary>
public sealed class DashedMintArguments : TheoryData<string, string?>
{
    public DashedMintArguments()
    {
        Add("--dashed-type-value", null);
        Add("claim", "--dashed-feature-value");
    }
}
