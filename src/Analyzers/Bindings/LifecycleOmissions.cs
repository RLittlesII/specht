namespace Specht.Analysis.Bindings;

/// <summary>The pinned schema's explicit eligibility interpretation for omitted lifecycle values.</summary>
public sealed record LifecycleOmissions(bool FeatureEligible, bool ClaimEligible, bool AdrEligible);
