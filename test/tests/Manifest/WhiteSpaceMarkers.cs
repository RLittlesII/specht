namespace Specht.Tests.Manifest;

/// <summary>
/// Each marker schema <c>0.3.0</c> reads, and a text of white space alone for it (<c>0001-F5</c> B-073; decision 0021
/// call 2).
/// </summary>
public sealed class WhiteSpaceMarkers : TheoryData<string, string>
{
    public WhiteSpaceMarkers()
    {
        foreach (var marker in new[]
                 {
                     "missing", "draft", "blocked", "approved", "done", "covered", "partial", "waived", "withdrawn", "superseded",
                 })
        {
            Add(marker, " ");
        }

        Add("draft", "\t");
        Add("done", " \r\n ");
    }
}
