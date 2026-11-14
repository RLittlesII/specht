namespace Specht.Tests.Model;

/// <summary>
/// The manifest members of one claim file set with a record, the record's frontmatter declaring an identity other than
/// the set's, and the epic and id the native Feature should carry (<c>0001-F16</c> B-037): a record <c>id</c> and a
/// record <c>epic</c> under no epic grammar, and a record <c>epic</c> and <c>id</c> under a declared one.
/// </summary>
public sealed class RecordIdentities : TheoryData<string, string, string?, string>
{
    /// <summary>Initializes one row for each identity key a record can declare apart from its set.</summary>
    public RecordIdentities()
    {
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] } }
            """,
            "id: F9",
            null,
            "F1");
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] } }
            """,
            "id: F1\nepic: \"0009\"",
            null,
            "F1");
        Add(
            """
            { "identifiers": { "epic": "^[0-9]{4}$" }, "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "epic": "0001",
                "record": ".spec/features/arithmetic/README.md" } ] } }
            """,
            "id: F9\nepic: \"0002\"",
            "0001",
            "F1");
    }
}
