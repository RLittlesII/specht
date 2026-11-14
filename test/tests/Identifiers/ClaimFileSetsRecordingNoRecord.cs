namespace Specht.Tests.Identifiers;

/// <summary>
/// A claim file set, under the epic grammar, whose Feature identity is not that of <c>features/one</c>, epic <c>0001</c> and id
/// <c>F1</c>, and whose <c>record</c> names that Feature's <c>.spec/README.md</c> in a form the check reads as no record
/// (<c>0001-F16</c> B-035; <c>0001-F19</c> § 10, item <c>0440</c>): a rooted path, and a path whose first <c>..</c> leaves the
/// root before it returns to the README.
/// </summary>
public sealed class ClaimFileSetsRecordingNoRecord : TheoryData<string>
{
    /// <summary>Initializes the rooted and outside-the-root rows.</summary>
    public ClaimFileSetsRecordingNoRecord()
    {
        Add("""{ "glob": "claims/*.feature", "epic": "0001", "feature": "F2", "record": "/features/one/.spec/README.md" }""");
        Add("""{ "glob": "claims/*.feature", "epic": "0001", "feature": "F2", "record": "../repo/features/one/.spec/README.md" }""");
    }
}
