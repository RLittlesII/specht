namespace Specht.Rules;

/// <summary>
/// SPEC011 and SPEC012 - a specification's frontmatter identity agrees with
/// where it sits, and no identity is claimed by two files.
/// </summary>
/// <remarks>
/// SPEC012 is the migration's safety net. Converting a Feature from
/// <c>epics/</c> to a co-located <c>.spec/</c> folder means removing the old
/// tree in the same change; a copy left behind is the failure mode that
/// produces two homes for one specification, which is the thing the layout
/// exists to end.
/// </remarks>
public sealed class IdentityRule : ISpecRule
{
    /// <inheritdoc />
    public string Id => "SPEC011";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => ["SPEC011", "SPEC012"];

    /// <inheritdoc />
    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        foreach (var feature in model.Features.Where(static candidate => candidate.Location.Layout.Name == "epics"))
        {
            var segments = feature.RelativePath.Split('/');

            if (segments.Length < 4 || feature.Epic is null || feature.Id is null)
            {
                continue;
            }

            var epicDirectory = segments[1];
            var featureDirectory = segments[2];

            if (!epicDirectory.StartsWith(feature.Epic + "-", StringComparison.Ordinal))
            {
                yield return new SpecViolation(
                    "SPEC011",
                    SpecSeverity.Error,
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf("epic"),
                    feature.Identity,
                    $"frontmatter epic '{feature.Epic}' does not match the containing directory '{epicDirectory}'");
            }

            if (!featureDirectory.StartsWith(feature.Id + "-", StringComparison.Ordinal))
            {
                yield return new SpecViolation(
                    "SPEC011",
                    SpecSeverity.Error,
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf("id"),
                    feature.Identity,
                    $"frontmatter id '{feature.Id}' does not match the containing directory '{featureDirectory}'");
            }
        }

        var duplicates = model.Features
            .Where(static feature => feature.Epic is not null && feature.Id is not null)
            .GroupBy(static feature => feature.Identity, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1);

        foreach (var group in duplicates)
        {
            var paths = group.Select(static feature => feature.RelativePath).Order(StringComparer.Ordinal).ToList();

            foreach (var feature in group)
            {
                yield return new SpecViolation(
                    "SPEC012",
                    SpecSeverity.Error,
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf("id"),
                    group.Key,
                    $"'{group.Key}' is specified in {paths.Count} places ({string.Join(", ", paths)}) - "
                        + "a migration removes the old tree in the same change");
            }
        }
    }
}
