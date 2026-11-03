namespace Specht.Tests.Model;

/// <summary>
/// A schema 0.3.0 task grammar, the items Feature <c>F1</c>'s folder holds, the items Feature <c>F2</c>'s folder holds,
/// and the one item <c>SPEC5004</c> reports a gap on when no epic grammar is declared: <c>F2</c>'s first item, since
/// each Feature's items run from 01, whether or not the grammar names an <c>epic</c> group (<c>0001-F5</c> B-032;
/// decision 0020 calls 5 and 6).
/// </summary>
public sealed class PerFeatureItems : TheoryData<string, string[], string[], string>
{
    public PerFeatureItems()
    {
        Add("^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$", ["0001-01", "0001-02"], ["0001-03"], "0001-03");
        Add("^F[0-9]+-(?<sequence>[0-9]{2})$", ["F1-01", "F1-02"], ["F2-02"], "F2-02");
    }
}
