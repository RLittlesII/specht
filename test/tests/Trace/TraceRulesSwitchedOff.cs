namespace Specht.Tests.Trace;

/// <summary>
/// The pins whose rules hold the unresolved-test rule, each with the id it is set <c>off</c> under in a manifest's
/// <c>rules</c> (<c>0001-F13</c> B-005): <c>SPEC4009</c> under schema <c>0.3.0</c>, the first version that holds it, and
/// under schema <c>0.4.0</c>.
/// </summary>
public sealed class TraceRulesSwitchedOff : TheoryData<string, string>
{
    public TraceRulesSwitchedOff()
    {
        Add("0.3.0", "SPEC4009");
        Add("0.4.0", "SPEC4009");
    }
}
