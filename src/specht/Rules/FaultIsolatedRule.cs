using Specht.Manifest;
using Specht.Model;
using Specht.Tree;

namespace Specht.Rules;

/// <summary>Isolates evaluation of one registered rule (ADR-0005 (c)).</summary>
/// <implements epic="0001" feature="F1" claim="B-012" />
public sealed class FaultIsolatedRule(ISpechtRule inner) : ISpechtRule
{
    /// <summary>Gets the wrapped rule, including its specialized entry points.</summary>
    public ISpechtRule Inner { get; } = inner;

    /// <inheritdoc />
    public string Id => Inner.Id;

    /// <inheritdoc />
    public SpechtRuleLevel DefaultLevel => Inner.DefaultLevel;

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-012" />
    /// <implements epic="0001" feature="F1" claim="B-017" />
    /// <implements epic="0001" feature="F7" claim="B-151" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
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
                new SpechtFinding(
                    SpechtManifestStore.RelativePath,
                    0,
                    null,
                    $"Rule {model.Schemas.Version.RuleId(Id)} failed with {exception.GetType().Name}.")
                {
                    RuleFailure = exception,
                },
            ];
        }
    }
}
