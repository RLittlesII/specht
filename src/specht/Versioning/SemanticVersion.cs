using System.Globalization;

namespace Specht.Versioning;

/// <summary>A schema version, written <c>major.minor.patch</c> (<c>0001-F7</c> B-039, decision 0005).</summary>
/// <param name="Major">The major number.</param>
/// <param name="Minor">The minor number.</param>
/// <param name="Patch">The patch number.</param>
public readonly record struct SemanticVersion(int Major, int Minor, int Patch) : IComparable<SemanticVersion>
{
    /// <summary>Reads <paramref name="text"/> as a version.</summary>
    /// <param name="text">The text.</param>
    /// <param name="version">The version <paramref name="text"/> names, when it names one.</param>
    /// <returns>Whether <paramref name="text"/> is exactly three numbers separated by dots.</returns>
    public static bool TryParse(string? text, out SemanticVersion version)
    {
        version = default;

        if (text?.Split('.') is not { Length: 3 } parts
            || !TryNumber(parts[0], out var major)
            || !TryNumber(parts[1], out var minor)
            || !TryNumber(parts[2], out var patch))
        {
            return false;
        }

        version = new SemanticVersion(major, minor, patch);

        return true;
    }

    /// <inheritdoc />
    public int CompareTo(SemanticVersion other) =>
        (Major, Minor, Patch).CompareTo((other.Major, other.Minor, other.Patch));

    /// <inheritdoc />
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Major}.{Minor}.{Patch}");

    private static bool TryNumber(string part, out int number) =>
        int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out number)
        && string.Equals(number.ToString(CultureInfo.InvariantCulture), part, StringComparison.Ordinal);
}
