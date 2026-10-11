namespace Specht.Rules;

/// <summary>
/// The rules the engine evaluates, named one by one (ADR-0001 stage C; <c>0001-F1</c> B-004, C-9). A rule that is not
/// on this list does not run, and a new rule is a new schema version (brief § 4).
/// </summary>
public static class SpecRules
{
    /// <summary>
    /// Every rule, in ordinal order of its <see cref="ISpecRule.Id"/>, the order the report's violations are collected
    /// in. Each read gives new instances.
    /// </summary>
    public static IReadOnlyList<ISpecRule> All =>
    [
        new FrontmatterSchemaRule(),
        new SectionStructureRule(),
        new IdentityRule(),
        new FeatureFileRule(),
        new ClaimRule(),
        new ChildItemRule(),
        new DependencyRule(),
        new ApprovalRule(),
    ];
}
