namespace Specht.Tests.Identifiers;

/// <summary>
/// Bounds a Nano ID draws below (<c>0001-F19</c> B-006): one, the two-character alphabet the unit tests use, and the
/// 32 characters of lower-case base32.
/// </summary>
public sealed class RandomSourceBounds : TheoryData<int>
{
    /// <summary>Initializes the 1, 2 and 32 rows.</summary>
    public RandomSourceBounds()
    {
        Add(1);
        Add(2);
        Add(32);
    }
}
