using Specht.Model;

namespace Specht.Rules.NativeClaims;

/// <summary>
/// SPEC027 reports each piece of claim input the check could not take claims from, at the place the model recorded it: a
/// claim file the shared Gherkin reader refuses, a set whose glob matches no file, a file two sets match, a Feature-level
/// tag naming another Feature than its set's, and a <c>record</c> the check cannot read or that is rooted.
/// </summary>
/// <implements epic="0001" feature="F16" claim="B-014" />
/// <implements epic="0001" feature="F16" claim="B-015" />
/// <implements epic="0001" feature="F16" claim="B-016" />
/// <implements epic="0001" feature="F16" claim="B-018" />
/// <implements epic="0001" feature="F16" claim="B-019" />
/// <implements epic="0001" feature="F16" claim="B-033" />
/// <implements epic="0001" feature="F16" claim="B-034" />
public sealed class ClaimInputFaultRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC027";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F16" claim="B-014" />
    /// <implements epic="0001" feature="F16" claim="B-015" />
    /// <implements epic="0001" feature="F16" claim="B-016" />
    /// <implements epic="0001" feature="F16" claim="B-018" />
    /// <implements epic="0001" feature="F16" claim="B-019" />
    /// <implements epic="0001" feature="F16" claim="B-033" />
    /// <implements epic="0001" feature="F16" claim="B-034" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model) =>
        model.ClaimFaults.Select(static fault => new SpechtFinding(fault.Path, fault.Line, fault.Identifier, Message(fault.Kind)));

    /// <implements epic="0001" feature="F16" claim="B-014" />
    private static string Message(SpechtClaimFaultKind kind) => kind switch
    {
        SpechtClaimFaultKind.UnreadableFile => "claim file cannot be read as English Gherkin, so it declares no claim",
        SpechtClaimFaultKind.EmptySet => "claim file set matches no claim file",
        SpechtClaimFaultKind.SharedFile => "claim file matches more than one claim file set, so it declares no claim",
        SpechtClaimFaultKind.FeatureTag => "Feature-level tag names a Feature other than its claim file set's",
        SpechtClaimFaultKind.UnreadableRecord => "claim file set's Feature document cannot be read under the root",
        _ => "claim file set's Feature document is an absolute path",
    };
}
