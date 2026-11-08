namespace Specht.Tests.Manifest;

/// <summary>
/// Each of the four <c>identifiers</c> members schema <c>0.3.1</c> adds with no default grammar, and a grammar a
/// consumer declares for it (<c>0001-F19</c> B-045, C-14).
/// </summary>
public sealed class NewTypeGrammars : TheoryData<string, string>
{
    /// <summary>Initializes the adr, lesson, decision and scenario rows.</summary>
    public NewTypeGrammars()
    {
        Add("adr", "^[0-9]{4}$");
        Add("lesson", "^[0-9]{4}$");
        Add("decision", "^[0-9]{4}$");
        Add("scenario", "^S-[0-9]+$");
    }
}
