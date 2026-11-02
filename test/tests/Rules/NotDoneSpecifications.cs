namespace Specht.Tests.Rules;

/// <summary>
/// A manifest's <c>markers</c>, and a specification's <c>spec_status</c> and delivery <c>status</c>, the status not being
/// the manifest's done marker, including <c>done</c> under a manifest that names another (<c>0001-F7</c> B-119).
/// </summary>
public sealed class NotDoneSpecifications : TheoryData<string, string, string>
{
    public NotDoneSpecifications()
    {
        Add("{}", "approved", "in-progress");
        Add("{}", "approved", "ready");
        Add("{}", "draft", "in-review");
        Add("""{"done":"delivered"}""", "approved", "done");
    }
}
