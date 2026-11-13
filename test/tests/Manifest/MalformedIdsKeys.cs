namespace Specht.Tests.Manifest;

/// <summary>
/// The members of a schema 0.4.0 manifest whose <c>ids</c> key <c>0001-F5</c> B-083 rejects, and the member the rejection
/// names: <c>ids</c> not an object, a member that is not one of the seven types, a record that is not an object, and a
/// record member other than <c>plain</c>, <c>form</c>, <c>id</c> and <c>mint</c>.
/// </summary>
public sealed class MalformedIdsKeys : TheoryData<string, string>
{
    /// <summary>Initializes the rows.</summary>
    public MalformedIdsKeys()
    {
        Add("""{ "ids": [] }""", "ids");
        Add("""{ "ids": "B-{id}" }""", "ids");
        Add("""{ "ids": { "task": { "plain": "T-{id}", "id": "[0-9]+" } } }""", "ids.task");
        Add("""{ "ids": { "claim": "B-{id}" } }""", "ids.claim");
        Add("""{ "ids": { "claim": { "prefix": "B-" } } }""", "ids.claim.prefix");
        Add("""{ "ids": { "claim": { "size": 6 } } }""", "ids.claim.size");
    }
}
