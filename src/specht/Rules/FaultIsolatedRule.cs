using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules;

/// <summary>Isolates evaluation of one registered rule (ADR-0005 (c)).</summary>
/// <implements epic="0001" feature="F1" claim="B-012" />
public sealed class FaultIsolatedRule(ISpecRule inner) : ISpecRule
{
    /// <summary>Gets the wrapped rule, including its specialized entry points.</summary>
    public ISpecRule Inner { get; } = inner;

    /// <inheritdoc />
    public string Id => Inner.Id;

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => Inner.ReportedIds;

    /// <inheritdoc />
    public SpecRuleLevel DefaultLevel => Inner.DefaultLevel;

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-012" />
    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        try
        {
            return Inner.Evaluate(model).ToList();
        }
        catch (Exception exception) when (exception is not (SpechtRootNotFoundException
            or SpechtManifestNotFoundException or SpechtManifestUnreadableException or SpechtManifestException))
        {
            return
            [
                new SpecViolation(Id, SpecSeverity.Error, SpecManifestStore.RelativePath, 0, null,
                    $"Rule {Id} failed with {exception.GetType().Name}.") { IsRuleFailure = true },
            ];
        }
    }
}
