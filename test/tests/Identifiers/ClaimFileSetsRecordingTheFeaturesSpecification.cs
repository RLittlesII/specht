namespace Specht.Tests.Identifiers;

/// <summary>
/// A claim file set, under the epic grammar, whose Feature identity is not that of <c>features/one</c>, epic <c>0001</c> and id
/// <c>F1</c>, and whose <c>record</c>, read as the check reads it (<c>0001-F16</c> B-035), is that Feature's
/// <c>.spec/README.md</c> (<c>0001-F19</c> B-078, amended 2026-11-14; <c>0001-F16</c> B-037; item <c>0440</c>): a set naming
/// another Feature id, one naming another epic, and one naming another Feature id whose <c>record</c> reaches the README
/// through <c>.</c> and <c>..</c> segments.
/// </summary>
public sealed class ClaimFileSetsRecordingTheFeaturesSpecification : TheoryData<string>
{
    /// <summary>Initializes the other Feature id, other epic and resolved record rows.</summary>
    public ClaimFileSetsRecordingTheFeaturesSpecification()
    {
        Add("""{ "glob": "claims/*.feature", "epic": "0001", "feature": "F2", "record": "features/one/.spec/README.md" }""");
        Add("""{ "glob": "claims/*.feature", "epic": "0002", "feature": "F1", "record": "features/one/.spec/README.md" }""");
        Add("""{ "glob": "claims/*.feature", "epic": "0001", "feature": "F2", "record": "features/two/../one/./.spec/README.md" }""");
    }
}
