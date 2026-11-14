namespace Specht.Tests.Manifest;

/// <summary>
/// A manifest pinned to schema <c>0.4.0</c> that holds neither <c>mint</c> nor an <c>identifiers</c> member for a
/// <c>0001-F19</c> C-13 type (<c>0001-F7</c> B-160): the pin alone, an <c>ids</c> record of the consumer's, and the task,
/// Feature and epic grammars.
/// </summary>
public sealed class ManifestsWithNoOldIdSetting : TheoryData<string>
{
    public ManifestsWithNoOldIdSetting()
    {
        Add("""{"schemaVersion":"0.4.0"}""");
        Add("""{ "schemaVersion": "0.4.0", "ids": { "claim": { "plain": "B-{id}", "id": "[0-9]{5}" } } }""");
        Add("""{"schemaVersion":"0.4.0","identifiers":{"task":"^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$","feature":"^F[0-9]+$","epic":"^[0-9]{4}$"}}""");
    }
}
