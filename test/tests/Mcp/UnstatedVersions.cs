namespace Specht.Tests.Mcp;

/// <summary>What a program writes for <c>--version</c> when it states no version.</summary>
public sealed class UnstatedVersions : TheoryData<string>
{
    public UnstatedVersions()
    {
        Add(string.Empty);
        Add("specht");
        Add("v1.2.3");
        Add("1.2");
        Add("version 1.2.3");
    }
}
