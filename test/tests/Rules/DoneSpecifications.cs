namespace Specht.Tests.Rules;

/// <summary>
/// A manifest's <c>markers</c>, and a specification's <c>spec_status</c> and delivery <c>status</c>, the status being the
/// manifest's done marker: the default <c>done</c> when the manifest names none, or the one it names
/// (<c>0001-F7</c> B-118; <c>0001-F5</c> B-063).
/// </summary>
public sealed class DoneSpecifications : TheoryData<string, string, string>
{
    public DoneSpecifications()
    {
        Add("{}", "draft", "done");
        Add("{}", "in-review", "done");
        Add("{}", "approved", "done");
        Add("{}", "superseded", "done");
        Add("""{"done":"delivered"}""", "draft", "delivered");
        Add("""{"done":"delivered"}""", "approved", "delivered");
    }
}
