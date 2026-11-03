namespace Specht.Tests.Manifest;

/// <summary>
/// Each shipped version below schema <c>0.3.0</c>, which keeps its reading of a white-space marker and of a
/// <c>$</c>-prefixed path identity member (<c>0001-F5</c> B-049; decision 0021 call 7).
/// </summary>
public sealed class PinsBelowZeroThreeZero : TheoryData<string>
{
    public PinsBelowZeroThreeZero()
    {
        Add("0.1.0");
        Add("0.2.0");
        Add("0.2.1");
    }
}
