namespace specht;

/// <summary>
/// One rule violation, rendered as an MSBuild-shaped diagnostic so GitHub
/// Actions annotates it on the pull request diff without extra plumbing.
/// </summary>
/// <param name="RuleId">The <c>SPEC###</c> rule that fired.</param>
/// <param name="Severity">Whether this fails the target.</param>
/// <param name="File">Path relative to the repository root - never absolute.</param>
/// <param name="Line">One-based line the diagnostic points at, or 0 when the file as a whole is at fault.</param>
/// <param name="Identifier">The specification identifier at fault, when there is one.</param>
/// <param name="Message">What is wrong, in terms the author can act on.</param>
public sealed record SpecViolation(
    string RuleId,
    SpecSeverity Severity,
    string File,
    int Line,
    string? Identifier,
    string Message)
{
    /// <summary>Renders the canonical MSBuild diagnostic line.</summary>
    public override string ToString()
    {
        var severity = Severity == SpecSeverity.Error ? "error" : "warning";
        var position = Line > 0 ? $"({Line})" : string.Empty;

        return $"{File}{position}: {severity} {RuleId}: {Message}";
    }
}
