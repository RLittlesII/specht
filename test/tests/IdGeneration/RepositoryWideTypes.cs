namespace Specht.Tests.IdGeneration;

/// <summary>The two repository-wide types <c>0001-F19</c> B-020 gives a default Nano ID size of 8.</summary>
public sealed class RepositoryWideTypes : TheoryData<string>
{
    /// <summary>Initializes the <c>adr</c> and <c>lesson</c> rows.</summary>
    public RepositoryWideTypes()
    {
        Add("adr");
        Add("lesson");
    }
}
