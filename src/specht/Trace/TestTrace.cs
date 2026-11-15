using System.Text.Json.Serialization;

namespace Specht.Trace;

/// <summary>One test a claim's traceability row names, and its resolution (<c>0001-F13</c> B-003, B-019).</summary>
/// <param name="Name">The name the Test cell gives in inline code, with its dotted qualifiers.</param>
/// <param name="Resolution">How the name resolves, or <see cref="TraceResolution.Skipped"/> for a skipped row.</param>
/// <param name="File">The root-relative file holding the test, only when it resolves; otherwise absent.</param>
/// <param name="Line">The one-based line of the test's declaration, only when it resolves; otherwise absent.</param>
/// <implements epic="0001" feature="F13" claim="B-003" />
/// <implements epic="0001" feature="F13" claim="B-019" />
public sealed record TestTrace(
    string Name,
    TraceResolution Resolution,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? File,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? Line);
