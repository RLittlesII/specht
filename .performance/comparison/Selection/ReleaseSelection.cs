using System;
using System.IO;
using System.Linq;

namespace Specht.Comparison;

/// <summary>Selects the predecessor from a complete publication catalog.</summary>
public static class ReleaseSelection
{
    public static ReleasePair Select(ReleaseRecord current, ReleaseCatalog catalog, string runId)
    {
        ComparisonJson.ValidateRelease(current);
        if (current.Draft || current.PublishedAt is null || string.IsNullOrWhiteSpace(runId))
            throw new InvalidDataException("Selection requires a published current release and run identity.");
        if (!catalog.Complete || catalog.Releases.IsDefault)
            throw new InvalidDataException("Release catalog is incomplete; do not select a baseline.");
        foreach (var release in catalog.Releases) ComparisonJson.ValidateRelease(release);
        if (catalog.Releases.Any(release => release.Repository != current.Repository)
            || catalog.Releases.GroupBy(static release => release.Id).Any(static group => group.Count() != 1))
            throw new InvalidDataException("Release catalog has duplicate identities or a different repository.");
        var published = catalog.Releases.Where(static release => !release.Draft && release.PublishedAt is not null)
            .OrderBy(static release => release.PublishedAt).ThenBy(static release => release.Id).ToArray();
        var recorded = published.SingleOrDefault(release => release.Id == current.Id);
        if (recorded is not null && recorded.PublishedAt != current.PublishedAt)
            throw new InvalidDataException("Current release publication key differs from the recorded first publication.");
        var previous = published.LastOrDefault(release => release.PublishedAt < current.PublishedAt
            || release.PublishedAt == current.PublishedAt && release.Id < current.Id);
        return new ReleasePair(1, current, previous, "publication-order-v1", ComparisonJson.Digest(published), runId);
    }
}
