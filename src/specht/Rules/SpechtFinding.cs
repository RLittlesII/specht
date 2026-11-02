using System.Text.Json.Nodes;

namespace Specht.Rules;

/// <summary>
/// What one rule found wrong, without its rule id or its severity: the evaluator stamps both (ADR-0010 § "One rule per
/// id").
/// </summary>
/// <param name="File">Path relative to the repository root - never absolute.</param>
/// <param name="Line">One-based line the finding points at, or 0 when the file as a whole is at fault.</param>
/// <param name="Identifier">The specification identifier at fault, when there is one.</param>
/// <param name="Message">What is wrong, in terms the author can act on.</param>
/// <implements epic="0001" feature="F1" claim="B-004" />
/// <implements epic="0001" feature="F1" claim="B-017" />
public sealed record SpechtFinding(string File, int Line, string? Identifier, string Message)
{
    /// <summary>
    /// What the rule expected, as the members its family names (<c>0001-F3</c> C-2), or <see langword="null"/> where no
    /// claim names them yet.
    /// </summary>
    public JsonObject? Expected { get; init; }

    internal Exception? RuleFailure { get; init; }

    /// <summary>Compares fields, expected JSON by value and any retained exception by identity.</summary>
    /// <param name="other">The finding to compare with.</param>
    /// <returns>Whether the two are the same finding.</returns>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public bool Equals(SpechtFinding? other) =>
        other is not null
        && string.Equals(File, other.File, StringComparison.Ordinal)
        && Line == other.Line
        && string.Equals(Identifier, other.Identifier, StringComparison.Ordinal)
        && string.Equals(Message, other.Message, StringComparison.Ordinal)
        && ReferenceEquals(RuleFailure, other.RuleFailure)
        && JsonNode.DeepEquals(Expected, other.Expected);

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public override int GetHashCode() => HashCode.Combine(File, Line, Identifier, Message, RuleFailure);
}
