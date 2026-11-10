using System;
using System.Globalization;

namespace Specht.Mcp.Features.PinnedTool;

/// <summary>One <c>/proc/&lt;pid&gt;/stat</c> line on Linux, read for the parent process id it states.</summary>
/// <implements epic="0132" feature="F3" claim="B-041" />
public sealed record ProcStatLine(string Text)
{
    /// <summary>Gets the field after the state, taken after the last <c>)</c>, or null when the text ends before it.</summary>
    /// <implements epic="0132" feature="F3" claim="B-041" />
    public int? Parent
    {
        get
        {
            var close = Text.LastIndexOf(')');
            if (close < 0)
            {
                return null;
            }

            var fields = Text[(close + 1)..].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return fields.Length > 1 && int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out var parent) ? parent : null;
        }
    }
}
