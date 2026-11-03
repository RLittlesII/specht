namespace Specht.Tests.Model;

/// <summary>The shipped pins below schema 0.3.0, each keeping its own reading under a tool shipping 0.3.0 (<c>0001-F5</c> B-049).</summary>
public sealed class OlderPins : TheoryData<string>
{
    public OlderPins()
    {
        Add("0.1.0");
        Add("0.2.0");
        Add("0.2.1");
    }
}
