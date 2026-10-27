using System.Text.Json.Nodes;

namespace Specht.Model;

/// <summary>
/// One rule violation, rendered as an MSBuild-shaped diagnostic so GitHub
/// Actions annotates it on the pull request diff without extra plumbing.
/// </summary>
/// <param name="RuleId">The <c>SPEC###</c> rule that fired.</param>
/// <param name="Severity">Whether this fails the target.</param>
/// <param name="File">Path relative to the repository root - never absolute.</param>
/// <param name="Line">One-based line the diagnostic points at, or 0 when the file as a whole is at fault.</param>
/// <param name="Identifier">The specification identifier at fault, when there is one; the line ends with it in square brackets.</param>
/// <param name="Message">What is wrong, in terms the author can act on.</param>
/// <implements epic="0001" feature="F1" claim="B-004" />
/// <implements epic="0001" feature="F1" claim="B-017" />
public sealed record SpecViolation(
    string RuleId,
    SpecSeverity Severity,
    string File,
    int Line,
    string? Identifier,
    string Message)
{
    /// <summary>
    /// What the rule expected, as the members its family names (<c>0001-F3</c> C-2), or <see langword="null"/> where no
    /// claim names them yet. Not part of the diagnostic line.
    /// </summary>
    public JsonObject? Expected { get; init; }

    internal Exception? RuleFailure { get; init; }

    /// <summary>Compares fields, expected JSON by value and any retained exception by identity.</summary>
    /// <param name="other">The violation to compare with.</param>
    /// <returns>Whether the two are the same violation.</returns>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public bool Equals(SpecViolation? other) =>
        other is not null
        && string.Equals(RuleId, other.RuleId, StringComparison.Ordinal)
        && Severity == other.Severity
        && string.Equals(File, other.File, StringComparison.Ordinal)
        && Line == other.Line
        && string.Equals(Identifier, other.Identifier, StringComparison.Ordinal)
        && string.Equals(Message, other.Message, StringComparison.Ordinal)
        && ReferenceEquals(RuleFailure, other.RuleFailure)
        && JsonNode.DeepEquals(Expected, other.Expected);

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public override int GetHashCode() => HashCode.Combine(RuleId, Severity, File, Line, Identifier, Message, RuleFailure);

    /// <summary>Renders the canonical MSBuild diagnostic line.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public override string ToString()
    {
        var severity = Severity == SpecSeverity.Error ? "error" : "warning";
        var position = Line > 0 ? $"({Line})" : string.Empty;

        var identifier = Identifier is null ? string.Empty : $" [{Identifier}]";

        return $"{File}{position}: {severity} {RuleId}: {Message}{identifier}";
    }
}
