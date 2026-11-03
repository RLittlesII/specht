namespace Specht.Tests.Manifest;

/// <summary>
/// A grammar under a pin below schema 0.3.0, its value, and whether that pin rejects it at parse: a group-less task
/// grammar is accepted under every older pin, the claim grammar is compiled at parse from 0.2.0 on and deferred under
/// 0.1.0, and no other grammar is compiled at parse below 0.3.0 (<c>0001-F5</c> B-049, B-014; item <c>0134</c>).
/// </summary>
public sealed class OlderPinGrammarHandling : TheoryData<string, string, string, bool>
{
    public OlderPinGrammarHandling()
    {
        Add("0.1.0", "task", "^[0-9]{4}-[0-9]{3}$", false);
        Add("0.2.0", "task", "^[0-9]{4}-[0-9]{3}$", false);
        Add("0.2.1", "task", "^[0-9]{4}-[0-9]{3}$", false);
        Add("0.1.0", "claim", "[", false);
        Add("0.2.0", "claim", "[", true);
        Add("0.2.1", "claim", "[", true);
        Add("0.2.0", "task", "[", false);
        Add("0.2.1", "feature", "[", false);
    }
}
