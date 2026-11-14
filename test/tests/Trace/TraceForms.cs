namespace Specht.Tests.Trace;

/// <summary>The trace's two forms (<c>0001-F13</c> B-006): the lines, and the JSON document <c>--json</c> selects.</summary>
public sealed class TraceForms : TheoryData<bool>
{
    public TraceForms()
    {
        Add(false);
        Add(true);
    }
}
