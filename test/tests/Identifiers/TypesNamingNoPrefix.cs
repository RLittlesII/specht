namespace Specht.Tests.Identifiers;

/// <summary>
/// Each of the seven types <c>0001-F19</c> C-13 opts in, and the prefix B-055 says stands ahead of the draw when its
/// formatting names none: none for <c>adr</c>, <c>lesson</c> and <c>decision</c>, <c>B-</c> for <c>claim</c>, <c>C-</c>
/// for <c>constraint</c>, <c>OQ-</c> for <c>openQuestion</c> and <c>S-</c> for <c>scenario</c>.
/// </summary>
public sealed class TypesNamingNoPrefix : TheoryData<string, string>
{
    /// <summary>Initializes one row for each of C-13's types.</summary>
    public TypesNamingNoPrefix()
    {
        Add("adr", string.Empty);
        Add("lesson", string.Empty);
        Add("decision", string.Empty);
        Add("claim", "B-");
        Add("constraint", "C-");
        Add("openQuestion", "OQ-");
        Add("scenario", "S-");
    }
}
