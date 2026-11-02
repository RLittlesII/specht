namespace Specht.Tests.Rules;

/// <summary>
/// A specification's <c>spec_status</c> and delivery <c>status</c> beside a sign-off row awaiting review, and whether the
/// sign-off rule reports that row at schema 0.3.0 (<c>0001-F7</c> B-122).
/// </summary>
public sealed class SignOffSpecifications : TheoryData<string, string, bool>
{
    public SignOffSpecifications()
    {
        Add("approved", "in-progress", true);
        Add("in-review", "done", false);
        Add("draft", "done", false);
    }
}
