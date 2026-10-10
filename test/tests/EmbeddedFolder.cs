using System.Globalization;
using System.Text.RegularExpressions;

namespace Specht.Tests;

/// <summary>
/// The integer <c>n</c> of the <c>schema/v&lt;n&gt;/</c> and <c>rules/v&lt;n&gt;/</c> folders an embedded schema version was
/// read from, shared by the unit tests and the acceptance steps. It is a folder name and not a schema version: a version is
/// <c>major.minor.patch</c> (<c>0001-F7</c> decision 0005), while the folders, the <c>$id</c> segment and a rule page's
/// <c>Schema version</c> row keep the integer until item 0126 renames them. The engine gives a test no member that says
/// which folder a version came from, so the folder is found by the Feature schema text the version carries.
/// </summary>
internal static partial class EmbeddedFolder
{
    /// <summary>Finds the folder <paramref name="version"/> was embedded under.</summary>
    /// <param name="version">A version of <see cref="SchemaVersions.Embedded"/>.</param>
    /// <returns>The folder's integer.</returns>
    public static int Of(SchemaVersion version)
    {
        var assembly = typeof(SchemaVersions).Assembly;
        var folders = assembly.GetManifestResourceNames()
            .Select(static name => (Name: name, Match: FeatureSchema().Match(name)))
            .Where(static resource => resource.Match.Success)
            .Where(resource =>
            {
                using var reader = new StreamReader(assembly.GetManifestResourceStream(resource.Name)!);

                return string.Equals(reader.ReadToEnd(), version.FeatureSchema, StringComparison.Ordinal);
            })
            .Select(static resource => int.Parse(resource.Match.Groups[1].Value, CultureInfo.InvariantCulture))
            .ToList();

        return folders.Count == 1
            ? folders[0]
            : throw new InvalidOperationException($"Schema version {version.Number} was embedded under {folders.Count} folders, not one.");
    }

    [GeneratedRegex("^schema/v([0-9]+)/feature-spec\\.frontmatter\\.schema\\.json$")]
    private static partial Regex FeatureSchema();
}
