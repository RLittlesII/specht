namespace Specht.Model;

/// <summary>One case under a claim: a Scenario's keyword line or an Examples body row's line, and whether it is planned.</summary>
/// <implements epic="0201" feature="F7" claim="B-016" />
public sealed record GherkinClaimCaseLine(int Line, bool Planned);
