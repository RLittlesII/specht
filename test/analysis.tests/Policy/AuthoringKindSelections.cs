namespace Specht.Analysis.Tests.Policy;

public sealed class AuthoringKindSelections : TheoryData<string, string, string, string[]>
{
    public AuthoringKindSelections()
    {
        Add("implements", "type", "class", new[] { "implements" });
        Add("follows", "type", "record", new[] { "follows" });
        Add("implements", "constructor", "constructor", new[] { "implements" });
        Add("follows", "event", "method", Array.Empty<string>());
    }
}
