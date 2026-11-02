using Specht.Model;
using Specht.Rules;

namespace Specht.Acceptance.Engine;

/// <summary>Injects a rule fault into engine and command scenarios through their existing rule seam.</summary>
public sealed class FailingRule(int findings = 0, string message = "synthetic failure", Exception? rejection = null) : ISpechtRule
{
    public string Id => "SPEC030";
    public Exception Failure { get; } = rejection ?? new FormatException(message);

    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        foreach (var finding in new ClaimIdRule().Evaluate(model).Take(findings))
        {
            yield return finding;
        }

        throw Failure;
    }
}
