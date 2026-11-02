namespace Specht.Tests.Rules;

/// <summary>Each pin that keeps the approved axis, and an approved specification's delivery <c>status</c> (<c>0001-F7</c> B-120).</summary>
public sealed class OlderPinApprovedSpecifications : TheoryData<string, string>
{
    public OlderPinApprovedSpecifications()
    {
        Add("0.1.0", "in-progress");
        Add("0.1.0", "done");
        Add("0.2.0", "in-progress");
        Add("0.2.0", "done");
        Add("0.2.1", "in-progress");
        Add("0.2.1", "done");
    }
}
