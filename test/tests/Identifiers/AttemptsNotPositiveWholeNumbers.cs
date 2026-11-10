namespace Specht.Tests.Identifiers;

/// <summary>
/// A type's <c>attempts</c> that is not a positive whole number (<c>0001-F19</c> B-052, C-12), as JSON: one row per row of
/// the scenario, zero, a negative number, a fraction and a whole number given as text, and an object and a list, which are
/// not numbers at all.
/// </summary>
public sealed class AttemptsNotPositiveWholeNumbers : TheoryData<string>
{
    /// <summary>Initializes the 0, -3, 2.5, "8", {} and [1] rows.</summary>
    public AttemptsNotPositiveWholeNumbers()
    {
        Add("0");
        Add("-3");
        Add("2.5");
        Add("\"8\"");
        Add("{}");
        Add("[1]");
    }
}
