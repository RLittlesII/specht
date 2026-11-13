namespace Specht.Analysis.Tests.Policy;

/// <summary>
/// A projected claim record the analysis rejects, and whether the older claim grammar is projected beside it
/// (<c>0201-F1</c> B-025; <c>0001-F5</c> B-084, B-085): a well-formed record beside the older grammar, a record without
/// its <c>plain</c> or <c>id</c>, a <c>plain</c> that does not end with its only <c>{id}</c>, holds another brace or is
/// not text, and an <c>id</c> that is empty, not text, or does not compile.
/// </summary>
public sealed class UnreadableClaimRecords : TheoryData<string, bool>
{
    /// <summary>Initializes the rows.</summary>
    public UnreadableClaimRecords()
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
