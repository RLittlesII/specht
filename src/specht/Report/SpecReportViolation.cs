using System.Text.Json.Nodes;

namespace specht.Report;

/// <summary>One violation as the document carries it (B-007).</summary>
/// <param name="RuleId">The <c>SPEC###</c> rule that fired.</param>
/// <param name="Severity">The violation's severity.</param>
/// <param name="File">Path relative to the root, with <c>/</c> separators (B-021).</param>
/// <param name="Line">One-based line, or 0 for the file as a whole.</param>
/// <param name="Identifier">The identifier at fault, where the rule has one.</param>
/// <param name="Message">What is wrong.</param>
/// <param name="Expected">What the rule expected: an object each rule family extends by addition (C-2, C-4).</param>
public sealed record SpecReportViolation(
    string RuleId,
    SpecSeverity Severity,
    string File,
    int Line,
    string? Identifier,
    string Message,
    JsonObject Expected);
