namespace Specht.Tests.Manifest;

/// <summary>
/// The three types <c>0001-F19</c> C-13 leaves out of the first slice, <c>epic</c>, <c>feature</c> and <c>task</c>, whose
/// <c>mint</c> entry the check accepts under schema <c>0.3.1</c> (B-062).
/// </summary>
public sealed class NotMintedTypes : TheoryData<string>
{
    /// <summary>Initializes the <c>epic</c>, <c>feature</c> and <c>task</c> rows.</summary>
    public NotMintedTypes()
    {
        Add("epic");
        Add("feature");
        Add("task");
    }
}
