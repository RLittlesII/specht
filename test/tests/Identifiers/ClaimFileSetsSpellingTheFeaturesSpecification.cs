namespace Specht.Tests.Identifiers;

/// <summary>
/// A claim file set, under the epic grammar, whose Feature identity is not that of <c>features/one</c>, epic <c>0001</c> and id
/// <c>F1</c>, and whose <c>record</c> spells that Feature's <c>.spec/README.md</c>, with the <c>claim</c> id a request for the
/// Feature mints when the set's files declare <c>B-aa</c>, the first candidate (<c>0001-F19</c> B-078, amended 2026-11-14;
/// <c>0001-F16</c> B-035, B-037; item <c>0440</c>). Where the check reads the <c>record</c> as that README, the set's claims are
/// held and <c>B-bb</c> is minted: a set naming another Feature id, one naming another epic, and one naming another Feature id
/// whose <c>record</c> reaches the README through <c>.</c> and <c>..</c> segments. Where the check reads it as no record, the
/// set matches by identity alone and <c>B-aa</c> is minted: a rooted path, and a path whose first <c>..</c> leaves the root
/// before it returns to the README.
/// </summary>
public sealed class ClaimFileSetsSpellingTheFeaturesSpecification : TheoryData<string, string>
{
    /// <summary>Initializes the three rows read as the README and the two read as no record.</summary>
    public ClaimFileSetsSpellingTheFeaturesSpecification()
    {
        Add("""{ "glob": "claims/*.feature", "epic": "0001", "feature": "F2", "record": "features/one/.spec/README.md" }""", "B-bb");
        Add("""{ "glob": "claims/*.feature", "epic": "0002", "feature": "F1", "record": "features/one/.spec/README.md" }""", "B-bb");
        Add("""{ "glob": "claims/*.feature", "epic": "0001", "feature": "F2", "record": "features/two/../one/./.spec/README.md" }""", "B-bb");
        Add("""{ "glob": "claims/*.feature", "epic": "0001", "feature": "F2", "record": "/features/one/.spec/README.md" }""", "B-aa");
        Add("""{ "glob": "claims/*.feature", "epic": "0001", "feature": "F2", "record": "../repo/features/one/.spec/README.md" }""", "B-aa");
    }
}
