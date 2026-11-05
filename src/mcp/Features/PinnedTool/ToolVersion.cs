using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Specht.Mcp.Features.PinnedTool;

/// <summary>
/// The <c>major.minor.patch</c> a tool states for <c>--version</c>. The informational version carries a commit after a plus sign
/// and may carry a pre-release label; both are read past, so <c>0.4.2-preview+abc</c> is <c>0.4.2</c>.
/// </summary>
/// <param name="Major">The major number.</param>
/// <param name="Minor">The minor number.</param>
/// <param name="Patch">The patch number.</param>
/// <implements epic="0132" feature="F3" claim="B-027" />
public readonly partial record struct ToolVersion(int Major, int Minor, int Patch) : IComparable<ToolVersion>
{
    /// <summary>Reads the version a tool wrote.</summary>
    /// <param name="text">What <c>--version</c> wrote; the first line counts.</param>
    /// <param name="version">The version, when <paramref name="text"/> starts with one.</param>
    /// <returns>Whether <paramref name="text"/> states a version.</returns>
    public static bool TryParse(string text, out ToolVersion version)
    {
        var match = Leading().Match(text);
        if (!match.Success
            || !int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var major)
            || !int.TryParse(match.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var minor)
            || !int.TryParse(match.Groups[3].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var patch))
        {
            version = default;

            return false;
        }

        version = new ToolVersion(major, minor, patch);

        return true;
    }

    /// <inheritdoc />
    public int CompareTo(ToolVersion other) =>
        Major != other.Major ? Major.CompareTo(other.Major) : Minor != other.Minor ? Minor.CompareTo(other.Minor) : Patch.CompareTo(other.Patch);

    /// <inheritdoc />
    public override string ToString() => $"{Major}.{Minor}.{Patch}";

    [GeneratedRegex(@"^\s*(\d+)\.(\d+)\.(\d+)", RegexOptions.CultureInvariant)]
    private static partial Regex Leading();
}
