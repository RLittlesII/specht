using Specht.Model;
using Specht.Rules;

namespace Specht.Acceptance.Engine;

/// <summary>Injects a rule fault into engine and command scenarios through their existing rule seam.</summary>
public sealed class FailingRule(int findings = 0, string message = "synthetic failure", Exception? rejection = null) : ISpecRule
{
    public string Id => "SPEC030";
    public IReadOnlyList<string> ReportedIds => ["SPEC030", "SPEC031"];
    public Exception Failure { get; } = rejection ?? new FormatException(message);

    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        foreach (var finding in new ClaimRule().Evaluate(model).Take(findings))
        {
            yield return finding;
        }

        throw Failure;
    }
}
