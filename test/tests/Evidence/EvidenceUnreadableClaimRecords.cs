namespace Specht.Tests.Evidence;

/// <summary>
/// A mapped claim record <c>0201-F1</c> B-025 rejects, and whether the older claim grammar is mapped beside it
/// (<c>0201-F5</c> B-047): a well-formed record beside the older grammar, a record without its <c>plain</c> or <c>id</c>, a
/// <c>plain</c> that does not end with its only <c>{id}</c>, holds another brace or is not text, and an <c>id</c> that is
/// empty, not text, or does not compile.
/// </summary>
public sealed class EvidenceUnreadableClaimRecords : TheoryData<string, bool>
{
    /// <summary>Initializes the rows.</summary>
    public EvidenceUnreadableClaimRecords()
    {
        Add("""{ "plain": "need/{id}", "id": "[0-9]+" }""", true);
        Add("""{ "id": "[0-9]+" }""", false);
        Add("""{ "plain": "need/{id}" }""", false);
        Add("""{ "plain": "need/{id}-x", "id": "[0-9]+" }""", false);
        Add("""{ "plain": "need/{id}/{id}", "id": "[0-9]+" }""", false);
        Add("""{ "plain": "{prefix}/{id}", "id": "[0-9]+" }""", false);
        Add("""{ "plain": 7, "id": "[0-9]+" }""", false);
        Add("""{ "plain": "need/{id}", "id": "" }""", false);
        Add("""{ "plain": "need/{id}", "id": 7 }""", false);
        Add("""{ "plain": "need/{id}", "id": "[0-9" }""", false);
    }
}
