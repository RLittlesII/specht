namespace Specht.Tests.Model;

/// <summary>
/// An edge written by Feature <c>F1</c>, which carries no <c>epic</c>, under a schema 0.3.0 manifest declaring no epic
/// grammar, and the Feature grammar it is read by: an <c>id</c> no specification declares (<c>0001-F5</c> B-027), a
/// qualified edge (B-029), and an edge fitting neither edge form (B-054). Each names no Feature.
/// </summary>
public sealed class EdgesNamingNoFeature : TheoryData<string, string>
{
    public EdgesNamingNoFeature()
    {
        Add("F9", "^F[0-9]+[a-z]?$");
        Add("0002/F2", "^F[0-9]+[a-z]?$");
        Add("F2x", "^F[0-9]+$");
    }
}
