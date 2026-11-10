namespace Specht.Tests.Mint;

/// <summary>
/// The two types of the first slice a request names without <c>--feature</c> (<c>0001-F19</c> C-13, A-7), each of which a
/// test opts in to Nano ID over a one-character alphabet, so the id it mints is known before the request.
/// </summary>
public sealed class OptedInRepositoryWideTypes : TheoryData<string>
{
    /// <summary>Initializes the <c>adr</c> and <c>lesson</c> rows.</summary>
    public OptedInRepositoryWideTypes()
    {
        Add("adr");
        Add("lesson");
    }
}
