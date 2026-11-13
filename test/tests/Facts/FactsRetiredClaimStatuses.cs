namespace Specht.Tests.Facts;

/// <summary>
/// The § 3 Status a retired claim carries: the text the manifest's <c>withdrawn</c> and <c>superseded</c> markers write
/// (<c>0132-F2</c> B-012).
/// </summary>
public sealed class FactsRetiredClaimStatuses : TheoryData<string>
{
    public FactsRetiredClaimStatuses()
    {
        Add("Withdrawn");
        Add("Superseded");
    }
}
